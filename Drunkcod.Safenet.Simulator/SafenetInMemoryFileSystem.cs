using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Drunkcod.Safenet.Simulator
{
	public class SafenetInMemoryFileSystem
	{
		readonly ConcurrentDictionary<string, SafenetInMemoryDirectory> directories = new ConcurrentDictionary<string, SafenetInMemoryDirectory>();

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

		public void Clear() => directories.Clear();
	}

	public class SafenetInMemoryDirectory
	{
		public SafenetDirectoryInfo Info;
		public List<SafenetInMemoryDirectory> SubDirectories = new List<SafenetInMemoryDirectory>();
		public List<SafenetInMemorFile> Files = new List<SafenetInMemorFile>();
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