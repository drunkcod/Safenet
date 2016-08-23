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
			var parts = path.Split(new [] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
			var dir = root;
			for(var i = 0; i != parts.Length; ++i)
				dir = dir.GetOrCreateDirectory(parts[i]);
			return dir;
		}

		public bool DirectoryExists(string path) {
			var parts = path.Split(new [] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
			var dir = root;
			for(var i = 0; i != parts.Length; ++i)
				if(!dir.TryGetDirectory(parts[i], out dir))
					return false;
			return true;
		}


		public void Clear() => root.Clear();
	}

	public class SafenetInMemoryDirectory
	{
		readonly ConcurrentDictionary<string, SafenetInMemoryDirectory> directories = new ConcurrentDictionary<string, SafenetInMemoryDirectory>();
		readonly ConcurrentDictionary<string, SafenetInMemorFile> files = new ConcurrentDictionary<string, SafenetInMemorFile>();

		public SafenetDirectoryInfo Info;
		public IEnumerable<SafenetInMemoryDirectory> SubDirectories => directories.Values;
		public IEnumerable<SafenetInMemorFile> Files => files.Values;

		public bool TryGetDirectory(string path, out SafenetInMemoryDirectory found) =>
			directories.TryGetValue(path, out found);

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

		public bool TryCreateFile(SafenetInMemorFile file) =>
			files.AddOrUpdate(file.Name, _ => file, (_,x) => x) == file;
	
		public bool DeleteFile(string name) {
			SafenetInMemorFile found;
			return files.TryRemove(name, out found);
		}

		public void Clear() {
			directories.Clear();
			files.Clear();
		}
	}

	public class SafenetInMemorFile
	{
		public string Name;
		public MediaTypeHeaderValue MediaType;
		public byte[] Bytes;
		public DateTime CreatedOn;
		public DateTime ModifiedOn;
		public byte[] Metadata;
	}

}