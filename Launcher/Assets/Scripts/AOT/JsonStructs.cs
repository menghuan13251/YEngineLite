using System.Collections.Generic;
using System;

// --- 用于 asset_map.json (资源路径 -> AB包) ---
[Serializable]
public class AssetMapEntry
{
    public string path;
    public string abName;
}

[Serializable]
public class AssetMapWrapper
{
    public List<AssetMapEntry> AssetMapList = new List<AssetMapEntry>();
}

// --- 用于 res_db.json (资源名 -> 资源路径) ---
[Serializable]
public class ResDBEntry
{
    public string res;
    public string path;
}

[Serializable]
public class ResDBWrapper
{
    public List<ResDBEntry> ResMapList = new List<ResDBEntry>();
}

// --- 用于 version.json (文件 -> MD5/Size) ---
[Serializable]
public class FileManifest
{
    public string md5;
    public long size;
}

[Serializable]
public class VersionEntry
{
    public string file;
    public FileManifest manifest;
}

[Serializable]
public class VersionManifestWrapper
{
    public List<VersionEntry> FileList = new List<VersionEntry>();
}