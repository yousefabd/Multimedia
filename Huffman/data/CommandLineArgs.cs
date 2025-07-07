namespace Huffman.data;
public class CommandLineArgs {
    public bool IsExtract { get; set; }
    public bool IsCompress { get; set; }
    public bool IsCreateEmpty { get; set; }
    public List<string> Paths { get; set; } = new();

    public static CommandLineArgs Parse(string[] args) {
        var result = new CommandLineArgs();

        if (args.Length == 0)
            return result;

        if (args[0].Equals("--compress", StringComparison.OrdinalIgnoreCase)) {
            result.IsCompress = true;
            result.Paths.AddRange(args.Skip(1));
        }
        else if (args[0].Equals("--create-empty", StringComparison.OrdinalIgnoreCase)) {
            result.IsCreateEmpty = true;
            result.Paths.AddRange(args.Skip(1));
        }
        else if (File.Exists(args[0])) {
            result.IsExtract = true;
            result.Paths.Add(args[0]);
        }

        return result;
    }
}