using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;

namespace Drunkcod.Safenet.Simulator
{
	public class SafenetInMemoryFileSystem
	{
		readonly SafenetInMemoryDirectory root = new SafenetInMemoryDirectory {
			Info = new SafenetDirectoryInfo {
					CreatedOn = DateTime.UtcNow,
					ModifiedOn = DateTime.UtcNow,
					IsPrivate = true,
					IsVersioned = false,
					Name = "Safenet Simulator Root",
				}
		};

		public SafenetInMemoryDirectory GetOrCreateDirectory(string path) {
			var parts = path.Split('/', '\\');
			var dir = root;
			for(var i = 0; i != parts.Length; ++i)
				dir = dir.GetOrCreateDirectory(parts[i]);
			return dir;
		}

		public void Clear() => root.Clear();
	}

	public class SafenetInMemoryDirectory
	{
		readonly ConcurrentDictionary<string, SafenetInMemoryDirectory> directories = new ConcurrentDictionary<string, SafenetInMemoryDirectory>();
		public SafenetDirectoryInfo Info;
		public IEnumerable<SafenetInMemoryDirectory> SubDirectories => directories.Values;
		public List<SafenetInMemorFile> Files = new List<SafenetInMemorFile>();

		public SafenetInMemoryDirectory GetOrCreateDirectory(string path) {
			return directories.GetOrAdd(path, _ => new SafenetInMemoryDirectory
			{
				Info = new SafenetDirectoryInfo {
					CreatedOn = DateTime.UtcNow,
					ModifiedOn = DateTime.UtcNow,
					IsPrivate = true,
					IsVersioned = false,
					Name = path,
				}
			});
		}

		public void Clear() {
			directories.Clear();
			Files.Clear();
		}
	}

	public class SafenetInMemorFile
	{
		public string Name;
		public MediaTypeHeaderValue MediaType;
		public byte[] Bytes;
		public DateTime CreatedOn;
		public DateTime ModifiedOn;
	}

}