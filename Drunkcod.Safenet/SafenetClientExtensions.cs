using System.IO;
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

		public static bool DirectoryExists(this SafenetClient self, string rootPath, string path) =>
			self.NfsHeadDirectoryAsync(rootPath, path).AwaitResult() == HttpStatusCode.OK;

		static async Task<byte[]> ReadAllBytesAsync(string path) {
			using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 15, FileOptions.Asynchronous)) {
				var bytes = new byte[fs.Length];
				await fs.ReadAsync(bytes, 0, bytes.Length);
				return bytes;
			}
		}
	}

	public static class TaskExtensions
	{
		public static void AwaitResult(this Task self) => 
			self.ConfigureAwait(false).GetAwaiter().GetResult();

		public static T AwaitResult<T>(this Task<T> self) => 
			self.ConfigureAwait(false).GetAwaiter().GetResult();

		public static SafenetResponse AwaitResponse(this Task<SafenetResponse> self) =>
			self.AwaitResult();

		public static T AwaitResponse<T>(this Task<SafenetResponse<T>> self) =>
			self.AwaitResult().Response;
	}
}