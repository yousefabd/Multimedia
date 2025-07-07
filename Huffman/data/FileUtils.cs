namespace Huffman.data; 
public static class FileUtils {
    public static long GetFolderSize(List<FolderFileEntry> entries, string folder) {
        return entries
            .Where(e => e.RelativePath != null && e.RelativePath.StartsWith(folder + "/"))
            .Sum(e => e.Data?.Length ?? 0);
    }
    public static string GetSuggestedArchiveName(List<FolderFileEntry> entries) {
        if (entries.Count == 1)
            return Path.GetFileNameWithoutExtension(entries[0].FullPath);

        if (entries.Count > 1) {
            var firstPath = entries[0].FullPath;
            string? topFolder = Path.GetDirectoryName(firstPath)?.Split(Path.DirectorySeparatorChar).LastOrDefault();
            return !string.IsNullOrEmpty(topFolder) ? topFolder : "Compressed";
        }

        return "Compressed";
    }
}
