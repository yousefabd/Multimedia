namespace Huffman.data; 
public struct FolderFileEntry {
    public string FullPath { get; set; }
    public string? RelativePath { get; set; }
    public byte[]? Data { get; set; }

    //Constructor for when browsing a file

    public FolderFileEntry(string fullPath, string relativePath) {
        FullPath = fullPath;
        RelativePath = relativePath;
    }
    public FolderFileEntry(string fullPath,string relativePath, byte[] data) {
        FullPath = fullPath;
        RelativePath = relativePath;
        Data = data;
    }

}
