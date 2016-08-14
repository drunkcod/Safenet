using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Drunkcod.Safenet
{
	public class SafenetResponse<T>
	{
		public T Result;
		public SafenetError? Error;
		public HttpStatusCode StatusCode;
	}

	public struct SafenetError
	{
		[JsonProperty("errorCode")] public int ErrorCode;
		[JsonProperty("description")] public string Description;
	}

	public class SafenetDirectoryResponse
	{
		[JsonProperty("info")] public SafenetDirectoryInfo Info;
		[JsonProperty("files")] public SafenetFileInfo[] Files;
		[JsonProperty("subDirectories")] public object[] SubDirectories;
	}

	public class SafenetDirectoryInfo
	{
		[JsonProperty("name")] public string Name;
		[JsonProperty("isPrivate")] public bool IsPrivate;
		[JsonProperty("isVersioned")] public bool IsVersioned;
		[JsonProperty("createdOn")] public DateTime CreatedOn;
		[JsonProperty("modifiedOn")] public DateTime ModifiedOn;
		[JsonProperty("metadata")] public string Metdata;
	}

	public class SafenetFileInfo
	{
		[JsonProperty("name")] public string Name;
		[JsonProperty("size")] public int Size;
		[JsonProperty("createdOn")] public DateTime CreatedOn;
		[JsonProperty("modifiedOn")] public DateTime ModifiedOn;
		[JsonProperty("metadata")] public string Metadata;
	}

	public class SafenetFileResponse
	{
		public DateTime CreatedOn;
		public DateTime ModifiedOn;
		public long? ContentLength;
		public Stream Body;
		public MediaTypeHeaderValue ContentType;
		public ContentRangeHeaderValue ContentRange;
	}
}
