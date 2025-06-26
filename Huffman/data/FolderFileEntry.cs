namespace Huffman.data; 
public struct FolderFileEntry {
    public string FullPath { get; set; }
    public string RelativePath { get; set; }

    public FolderFileEntry(string fullPath, string relativePath) {
        FullPath = fullPath;
        RelativePath = relativePath;
    }
}
