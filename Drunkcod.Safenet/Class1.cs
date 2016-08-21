using System;
using System.IO;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Drunkcod.Safenet
{
	public struct SafenetError
	{
		[JsonProperty("description")] public string Description;
		[JsonProperty("errorCode")] public int? ErrorCode;
	}

	public class SafenetAuthResponse
	{
		[JsonProperty("token")] public string Token;
		[JsonProperty("permissions")] public string[] Permissions;
	}

	public class SafenetAppInfo
	{
		[JsonProperty("name")] public string Name;
		[JsonProperty("id")] public string Id;
		[JsonProperty("version")] public string Version;
		[JsonProperty("vendor")] public string Vendor;
	}

	public class SafenetAuthRequest
	{
		[JsonProperty("app")] public SafenetAppInfo App;
		[JsonProperty("permissions")] public string[] Permissions = new string[0];
	}

	public class SafenetDirectoryResponse
	{
		[JsonProperty("info")] public SafenetDirectoryInfo Info;
		[JsonProperty("files")] public SafenetFileInfo[] Files = new SafenetFileInfo[0];
		[JsonProperty("subDirectories")] public SafenetDirectoryInfo[] SubDirectories = new SafenetDirectoryInfo[0];
	}

	public class SafenetDirectoryInfo
	{
		[JsonProperty("name")] public string Name;
		[JsonProperty("isPrivate")] public bool IsPrivate;
		[JsonProperty("isVersioned")] public bool IsVersioned;
		[JsonProperty("createdOn")] public DateTime CreatedOn;
		[JsonProperty("modifiedOn")] public DateTime ModifiedOn;
		[JsonProperty("metadata")] public byte[] Metdata;
	}

	public class SafenetFileInfo
	{
		[JsonProperty("name")] public string Name;
		[JsonProperty("size")] public int Size;
		[JsonProperty("createdOn")] public DateTime CreatedOn;
		[JsonProperty("modifiedOn")] public DateTime ModifiedOn;
		[JsonProperty("metadata")] public byte[] Metadata;
	}

	public class SafenetFileResponse
	{
		public DateTime CreatedOn;
		public DateTime ModifiedOn;
		public long? ContentLength;
		public Stream Body;
		public MediaTypeHeaderValue ContentType;
		public ContentRangeHeaderValue ContentRange;
		public byte[] Metadata;
	}

	public class SafenetDnsRegisterServiceRequest
	{
		[JsonProperty("longName")] public string LongName;
		[JsonProperty("serviceName")] public string ServiceName;
		[JsonProperty("rootPath")] public string RootPath;
		[JsonProperty("serviceHomeDirPath")] public string ServiceHomeDirPath;
	}

	public class SafenetNfsCreateDirectoryRequest
	{
		public string RootPath;
		public string DirectoryPath;
		public bool IsPrivate;
		public byte[] Metadata = new byte[0];
	}

	public class SafenetNfsPutFileRequest
	{
		public string RootPath;
		public string FilePath;
		public MediaTypeHeaderValue ContentType;
		public byte[] Metadata = new byte[0];
		public byte[] Bytes;
	}
}
