namespace Huffman.data; 
public static class FileUtils {
    private static long GetFolderSize(List<FolderFileEntry> entries, string folder) {
        return entries
            .Where(e => e.RelativePath != null && e.RelativePath.StartsWith(folder + "/"))
            .Sum(e => e.Data?.Length ?? 0);
    }

}
