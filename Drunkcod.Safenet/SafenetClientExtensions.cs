using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Drunkcod.Safenet
{
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

		public static Task<KeyValuePair<string, SafenetResponse>[]> UploadPathsAsync(this SafenetClient self, IEnumerable<string> sourcePaths, string rootPath, string destinationPath) {
			var knownDirs = new ConcurrentDictionary<string,bool>(new[] { new KeyValuePair<string, bool>("", true), });
			if(string.IsNullOrEmpty(destinationPath))
				destinationPath = string.Empty;
			else
				destinationPath += "/";

			return Task.WhenAll(GetFilePaths(sourcePaths).Select(async x => {
				var targetPath = destinationPath + x.Value;
				self.EnsureDirectory(rootPath, UrlPath.GetDirectoryName(targetPath), knownDirs);
				return new KeyValuePair<string, SafenetResponse>(x.Key, await self.UploadFileAsync(x.Key, rootPath, targetPath));
			}));
		}

		static void EnsureDirectory(this SafenetClient self, string rootPath, string path, ConcurrentDictionary<string,bool> knownDirs) {
			knownDirs.GetOrAdd(path, key => {
				if (self.DirectoryExists(rootPath, key))
					return true;
				self.EnsureDirectory(rootPath, UrlPath.GetDirectoryName(key), knownDirs);
				self.NfsPostAsync(new SafenetNfsCreateDirectoryRequest {
					RootPath = rootPath,
					DirectoryPath = key,
					IsPrivate = false,
				}).EnsureSuccess();
					return true;
			});
		} 

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