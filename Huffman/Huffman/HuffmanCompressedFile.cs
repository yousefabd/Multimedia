namespace Huffman.Huffman; 
public class HuffmanCompressedFile {
    public string FileName { get; set; } = "";
    public Dictionary<byte, int> FrequencyTable { get; set; } = new();
    public byte[] EncodedData { get; set; } = Array.Empty<byte>();
}
