using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Drunkcod.Safenet;

namespace SafeBox
{
	class Program
	{
		static T GetAttribute<T>() where T : Attribute =>
			(T)typeof(Program).Assembly.GetCustomAttribute(typeof(T));
		
		static int Main(string[] args) {
			if(args.Length != 2) { 
				Console.WriteLine("Usage is: SafeBox <folder> <target>");
				return -1;
			}
			var source = new DirectoryInfo(Path.GetFullPath(args[0]));
			var target = args[1];
			if(!source.Exists) {
				Console.WriteLine($"No such directory {source}");
				return -2;
			}
			Console.WriteLine($"Synching {source} -> {target}");
			var safe = new SafenetClient();
			var auth = safe.AuthPostAsync(new SafenetAuthRequest {
				App = new SafenetAppInfo {
					Id = GetAttribute<AssemblyProductAttribute>().Product,
					Name = GetAttribute<AssemblyTitleAttribute>().Title,
					Vendor = GetAttribute<AssemblyCompanyAttribute>().Company,
					Version = typeof(Program).Assembly.GetName().Version.ToString(4),
				}
			});
			if(auth.Result.StatusCode != HttpStatusCode.OK) {
				Console.WriteLine("Auth failed.");
				return -3;
			}
			safe.SetToken(auth.Result.Response.Token);

			return UploadDirectory(source, safe, target);
		}

		static int UploadDirectory(DirectoryInfo source, SafenetClient safe, string target)
		{
			var q = new Queue<DirectoryInfo>();
			q.Enqueue(source);
			while (q.Count != 0)
			{
				var c = q.Dequeue();
				foreach (var item in c.EnumerateDirectories("*"))
					q.Enqueue(item);

				var currentDir = c.FullName.Replace(source.FullName, "");
				var getParent = safe.NfsGetDirectoryAsync("app", Path.GetDirectoryName(target + currentDir));
				if (getParent.Result.StatusCode != HttpStatusCode.OK)
				{
					Console.WriteLine("Failed to get directory");
					return -4;
				}
				var parent = getParent.Result.Response;
				var targetPath = target + currentDir;
				var p = Path.GetFileName(targetPath);
				if (!parent.SubDirectories.Any(x => x.Name == p))
				{
					var makeTarget = safe.NfsPostAsync(new SafenetNfsCreateDirectoryRequest
					{
						RootPath = "app",
						DirectoryPath = targetPath,
						IsPrivate = true,
					});
					if (makeTarget.Result.StatusCode != HttpStatusCode.OK)
					{
						Console.WriteLine($"Failed to create {target}");
						return -5;
					}
				}
				Console.WriteLine(target + currentDir);
				var getCurrentTarget = safe.NfsGetDirectoryAsync("app", targetPath);
				if (getCurrentTarget.Result.StatusCode != HttpStatusCode.OK)
				{
					Console.WriteLine("Failed to get directory");
					return -4;
				}
				var current = getCurrentTarget.Result.Response;
				foreach (var item in c.EnumerateFiles("*"))
				{
					if (current.Files.All(x => x.Name != item.Name))
					{
						var fileX = safe.NfsPostAsync(new SafenetNfsPutFileRequest
						{
							RootPath = "app",
							ContentType = MediaTypeHeaderValue.Parse("application/octet-stream"),
							FilePath = Path.Combine(target + currentDir, item.Name),
							Bytes = File.ReadAllBytes(item.FullName)
						});
						if (fileX.Result.StatusCode == HttpStatusCode.OK)
							Console.WriteLine($"->  {item}");
						else Console.WriteLine($"!!! Failed to upload {item} !!!");
					}
					else Console.WriteLine($"{item} already uploaded.");
				}
			}
			return 0;
		}
	}
}
