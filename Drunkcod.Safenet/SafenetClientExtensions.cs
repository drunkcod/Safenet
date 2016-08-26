using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
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

	public static class SafenetClientExtensions
	{
		public static async Task<SafenetResponse> UploadFileAsync(this SafenetClient self, string sourcePath, string rootPath, string destinationPath) =>
			await self.NfsPostAsync(new SafenetNfsPutFileRequest {
				RootPath = rootPath,
				FilePath = destinationPath,
				ContentType = MediaTypeHeaderValue.Parse("application/octet-stream"),
				Bytes = await ReadAllBytesAsync(sourcePath).ConfigureAwait(false),
			});

		public static async Task<bool> DirectoryExistsAsync(this SafenetClient self, string rootPath, string path) =>
			await self.NfsHeadDirectoryAsync(rootPath, path) == HttpStatusCode.OK;

		static async Task<byte[]> ReadAllBytesAsync(string path) {
			using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 15, FileOptions.Asynchronous)) {
				var bytes = new byte[fs.Length];
				await fs.ReadAsync(bytes, 0, bytes.Length);
				return bytes;
			}
		}

		class DirectoryUpload
		{
			public Task Init = Task.FromResult(0);
			public List<KeyValuePair<string,string>> Refs = new List<KeyValuePair<string, string>>(); 
		}

		public static Task<KeyValuePair<string, SafenetResponse>[]> UploadPathsAsync(this SafenetClient self, IEnumerable<string> sourcePaths, string rootPath, string destinationPath, EventHandler<UploadProgressEventArgs> onProgress) {
			var totals = 0;
			var done = 0;
			var knownDirs = new ConcurrentDictionary<string,DirectoryUpload>(new[] { new KeyValuePair<string, DirectoryUpload>("", new DirectoryUpload()) });

			foreach(var x in GetFilePaths(sourcePaths)) {
				var targetPath = UrlPath.Combine(destinationPath, x.Value);
				
				onProgress(null, new UploadProgressEventArgs(targetPath, Interlocked.Increment(ref totals), done));
				var dir = self.EnsureDirectory(rootPath, UrlPath.GetDirectoryName(targetPath), knownDirs);
				dir.Refs.Add(x);
			}

			return Task.WhenAll(GetRefs(knownDirs.Values).Select(async x => {
				var targetPath = UrlPath.Combine(destinationPath, x.Value);
				var r = await self.UploadFileAsync(x.Key, rootPath, targetPath);
				onProgress(null, new UploadProgressEventArgs(targetPath, totals, Interlocked.Increment(ref done)));
				return new KeyValuePair<string, SafenetResponse>(x.Key, r);
			}));
		}

		static IEnumerable<KeyValuePair<string,string>> GetRefs(ICollection<DirectoryUpload> uploads) {
			var inits = new Task[uploads.Count];
			var refs = new List<KeyValuePair<string,string>>[uploads.Count];
			var n = 0;
			foreach(var item in uploads) {
				inits[n] = item.Init;
				refs[n] = item.Refs;
				++n;
			}
			while(inits.Length != 0) {
				var done = Task.WaitAny(inits);
				foreach(var item in refs[done])
					yield return item;
				var last = inits.Length - 1;
				inits[done] = inits[last];
				refs[done] = refs[last];
				Array.Resize(ref inits, last);
			}
		}

		static DirectoryUpload EnsureDirectory(this SafenetClient self, string rootPath, string path, ConcurrentDictionary<string,DirectoryUpload> knownDirs) =>
			knownDirs.GetOrAdd(path, key => {
				var parent = self.EnsureDirectory(rootPath, UrlPath.GetDirectoryName(key), knownDirs);
				return new DirectoryUpload {
					Init = Task.Factory.StartNew(() => {
						parent.Init.Wait();
						if (self.DirectoryExists(rootPath, key))
							return;
						self.NfsPostAsync(new SafenetNfsCreateDirectoryRequest {
							RootPath = rootPath,
							DirectoryPath = key,
							IsPrivate = false,
						}).EnsureSuccess();
					})
				};
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