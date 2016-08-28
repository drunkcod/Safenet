using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Drunkcod.Safenet
{
	public class UploadProgressEventArgs : EventArgs
	{
		public readonly int TotalFiles;
		public readonly int UploadedFiles;
		public string ActiveFile;

		public UploadProgressEventArgs(string file, int total, int uploaded) {
			this.TotalFiles = total;
			this.UploadedFiles = uploaded;
			this.ActiveFile = file;
		}
	}

	static class DictionaryExtensions
	{
		public static TValue GetOrAdd<TKey,TValue>(this IDictionary<TKey,TValue> self, TKey key, Func<TKey,TValue> valueFactory) {
			TValue found;
			if(!self.TryGetValue(key, out found)) {
				found = valueFactory(key);
				self.Add(key, found);
			}
			return found;
		}
	}

	public static class SafenetClientExtensions
	{
		public static async Task<SafenetResponse> UploadFileAsync(this SafenetClient self, string sourcePath, string rootPath, string destinationPath) =>
			await self.NfsPostAsync(new SafenetNfsPutFileRequest {
				RootPath = rootPath,
				FilePath = destinationPath,
				ContentType = MediaTypeHeaderValue.Parse("application/octet-stream"),
				Bytes = await ReadAllBytesAsync(sourcePath).ConfigureAwait(false),
			});

		public static async Task<SafenetResponse> DownloadFileAsyn(this SafenetClient self, string rootPath, string path, string targetPath) {
			var r = await self.NfsGetFileAsync(rootPath, path);
			if(r.StatusCode == HttpStatusCode.OK)
				using(var dst = File.OpenWrite(targetPath))
					await r.Response.Body.CopyToAsync(dst);
			return new SafenetResponse(r.StatusCode, r.Error);
		}

		public static async Task<bool> DirectoryExistsAsync(this SafenetClient self, string rootPath, string path) =>
			await self.NfsHeadDirectoryAsync(rootPath, path) == HttpStatusCode.OK;

		static async Task<byte[]> ReadAllBytesAsync(string path) {
			using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 15, FileOptions.Asynchronous)) {
				var bytes = new byte[fs.Length];
				await fs.ReadAsync(bytes, 0, bytes.Length);
				return bytes;
			}
		}

		public static IEnumerable<Task<KeyValuePair<string, SafenetResponse>>> DownloadDirectoryAsync(this SafenetClient self, string rootPath, string path, string targetPath) {
			var q = new Queue<KeyValuePair<string, string>>();
			q.Enqueue(new KeyValuePair<string, string>(path, targetPath));
			while(q.Count != 0) {
				var c = q.Dequeue();
				Directory.CreateDirectory(c.Value);
				var r = self.NfsGetDirectoryAsync(rootPath, c.Key).AwaitResult();
				foreach(var file in r.Response.Files) {
					var sourcePath = UrlPath.Combine(c.Key, file.Name);
					yield return self.DownloadFileAsyn(rootPath, sourcePath, Path.Combine(c.Value, file.Name)).ContinueWith(x =>
						new KeyValuePair<string, SafenetResponse>(sourcePath, x.Result));
				}
				foreach(var child in r.Response.SubDirectories)
					q.Enqueue(new KeyValuePair<string, string>(UrlPath.Combine(c.Key, child.Name), Path.Combine(c.Value, child.Name)));
			}
		}

		class DirectoryUpload
		{
			readonly Task init;
			readonly List<KeyValuePair<string,string>> refs = new List<KeyValuePair<string, string>>();

			public void Add(KeyValuePair<string,string> item) => refs.Add(item);

			public DirectoryUpload(DirectoryUpload parent, Action<Task> init) {
				this.init = parent.init.ContinueWith(init);
			}

			public DirectoryUpload(Task init) {
				this.init = init;
			}

			public async Task<IEnumerable<KeyValuePair<string,string>>> GetRefsAsync() {
				await init;
				return refs;
			}
		}

		public static IEnumerable<Task<KeyValuePair<string, SafenetResponse>>> UploadPathsAsync(this SafenetClient self, IEnumerable<string> sourcePaths, string rootPath, string destinationPath, EventHandler<UploadProgressEventArgs> onProgress) {
			var totals = 0;
			var done = 0;
			var knownDirs = new Dictionary<string,DirectoryUpload> { { "", new DirectoryUpload(Task.FromResult(0)) } };

			foreach(var x in GetFilePaths(sourcePaths)) {
				var targetPath = UrlPath.Combine(destinationPath, x.Value);
				
				onProgress(null, new UploadProgressEventArgs("Preparing upload...", Interlocked.Increment(ref totals), done));
				var dir = self.EnsureDirectory(rootPath, UrlPath.GetDirectoryName(targetPath), knownDirs);
				dir.Add(x);
			}

			return WithRefs(knownDirs.Values, async x => {
				var targetPath = UrlPath.Combine(destinationPath, x.Value);
				onProgress(null, new UploadProgressEventArgs(targetPath, totals, done));
				var r = await self.UploadFileAsync(x.Key, rootPath, targetPath);
				onProgress(null, new UploadProgressEventArgs(targetPath, totals, Interlocked.Increment(ref done)));
				return new KeyValuePair<string, SafenetResponse>(x.Key, r);
			});
		}

		static IEnumerable<T> WithRefs<T>(ICollection<DirectoryUpload> uploads, Func<KeyValuePair<string,string>,T> convert) =>
			uploads.GetResults(x => x.GetRefsAsync()).SelectMany(x => x.Select(convert));

		static DirectoryUpload EnsureDirectory(this SafenetClient self, string rootPath, string path, Dictionary<string,DirectoryUpload> knownDirs) =>
			knownDirs.GetOrAdd(path, key => {
				var parent = self.EnsureDirectory(rootPath, UrlPath.GetDirectoryName(key), knownDirs);
				return new DirectoryUpload(parent, x => {
						x.Wait();
						if (self.DirectoryExists(rootPath, key))
							return;
						self.NfsPostAsync(new SafenetNfsCreateDirectoryRequest {
							RootPath = rootPath,
							DirectoryPath = key,
							IsPrivate = false,
						}).EnsureSuccess();
				});
			});

		public static bool DirectoryExists(this SafenetClient self, string rootPath, string path) => 
			self.DirectoryExistsAsync(rootPath, path).AwaitResult();

		static IEnumerable<KeyValuePair<string,string>> GetFilePaths(IEnumerable<string> paths) =>
			paths.SelectMany(x => GetFilePaths(Path.GetDirectoryName(x), x));

		static IEnumerable<KeyValuePair<string, string>> GetFilePaths(string root, string item) {
			if (File.Exists(item))
				yield return new KeyValuePair<string, string>(item, item.Substring(1 + root.Length).Replace(Path.DirectorySeparatorChar, '/'));
			else
				foreach(var x in Directory.EnumerateFileSystemEntries(item))
				foreach(var y in GetFilePaths(root, x))
					yield return y;
		}
	}
}