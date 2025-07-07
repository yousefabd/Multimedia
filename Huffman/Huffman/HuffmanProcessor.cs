using Huffman.Archive;
using Huffman.data;
using System.Text;

namespace Huffman.Huffman;

public class HuffmanProcessor : IArchiveProcessor {
    // public Dictionary<byte, string> CodeTable { get; set; } = [];


    public Dictionary<byte, int> MakeRepitionsTable(byte[] bytes) {
        var frequencies = new Dictionary<byte, int>();
        foreach (var byt in bytes) {
            if (!frequencies.ContainsKey(byt)) {
                frequencies.Add(byt, 0);
            }
            frequencies[byt]++;
        }
        return frequencies;
    }
    public HuffmanNode BuildHuffTree(Dictionary<byte, int> frequencies) {
        var pq = new PriorityQueue<HuffmanNode, int>();
        foreach (var kv in frequencies) {
            pq.Enqueue(new HuffmanNode { Symbol = kv.Key, Frequency = kv.Value }, kv.Value);
        }

        while (pq.Count > 1) {
            var left = pq.Dequeue();
            var right = pq.Dequeue();
            pq.Enqueue(new HuffmanNode {
                Frequency = left.Frequency + right.Frequency,
                Left = left,
                Right = right
            }, left.Frequency + right.Frequency);
        }

        return pq.Dequeue();
    }
    public void BuildCodeTable(HuffmanNode node, string code, Dictionary<byte, string> codeTable) {
        if (node is null) {
            return;
        }
        if (node.IsLeaf && node.Symbol.HasValue) {
            codeTable[node.Symbol.Value] = code;

        }
        BuildCodeTable(node.Left, code + '0', codeTable);
        BuildCodeTable(node.Right, code + '1', codeTable);

    }
    public HuffmanCompressedFile HuffmanCompress(string inputPath) {
        byte[] data = File.ReadAllBytes(inputPath);
        var freqTable = MakeRepitionsTable(data);
        var tree = BuildHuffTree(freqTable);

        var codeTable = new Dictionary<byte, string>();
        BuildCodeTable(tree, "", codeTable);

        var writer = new BitWriter();
        foreach (byte b in data) {
            writer.WriteBits(codeTable[b]);
        }

        return new HuffmanCompressedFile {
            FileName = Path.GetFileName(inputPath),
            FrequencyTable = freqTable,
            EncodedData = writer.GetBytes()
        };
    }
    public int Compress(List<FolderFileEntry> files, string outputArchivePath, IProgress<ProgressReport>? progress = null, CancellationToken cancellationToken = default, ManualResetEventSlim pauseEvent = default!) {
        using var fs = new BinaryWriter(File.Open(outputArchivePath, FileMode.Create));

        // === Write number of files ===
        fs.Write(files.Count);
        //too lazy to switch to normal for loop
        int i = 0;
        long totalOriginalSize = 0;
        foreach (FolderFileEntry entry in files) {
            //check if cancelled
            cancellationToken.ThrowIfCancellationRequested();
            //check if paused
            pauseEvent.Wait();

            totalOriginalSize += new FileInfo(entry.FullPath).Length;
            HuffmanCompressedFile compressed = HuffmanCompress(entry.FullPath);

            byte[] relativePathBytes = Encoding.UTF8.GetBytes(entry.RelativePath);
            // === Write relative path ===
            fs.Write(relativePathBytes.Length);         // Int32
            fs.Write(relativePathBytes);                // UTF-8 bytes

            // === Write frequency table ===
            fs.Write(compressed.FrequencyTable.Count);
            foreach (var kvp in compressed.FrequencyTable) {
                fs.Write(kvp.Key);                      // Byte
                fs.Write(kvp.Value);                    // Int32
            }

            // === Write compressed data ===
            fs.Write(compressed.EncodedData.Length);    // Int32
            fs.Write(compressed.EncodedData);           // Byte[]
            int percent = (int)(((i + 1) / (float)files.Count) * 100);
            i++;
            progress?.Report(new ProgressReport {
                FileName = entry.RelativePath,
                Percentage = percent
            });
        }
        long compressedSize = new FileInfo(outputArchivePath).Length;

        double ratio = (double)compressedSize / totalOriginalSize;
        return (int)(100 * ratio);
    }
    public List<FolderFileEntry> Decompress(string archivePath, string outputBaseDirectory, IProgress<ProgressReport>? progress, CancellationToken cancellationToken = default, ManualResetEventSlim pauseEvent = default!) {
        var result = new List<FolderFileEntry>();

        using var reader = new BinaryReader(File.Open(archivePath, FileMode.Open));

        // === Read number of files ===
        int fileCount = reader.ReadInt32(); // Int32: number of files

        for (int i = 0; i < fileCount; i++) {
            //check if cancelled
            cancellationToken.ThrowIfCancellationRequested();
            //check if paused
            pauseEvent.Wait();
            // === Read relative path ===
            int pathLength = reader.ReadInt32();                 // Int32
            byte[] pathBytes = reader.ReadBytes(pathLength);     // Byte[]
            string relativePath = Encoding.UTF8.GetString(pathBytes);

            // === Read frequency table ===
            int tableSize = reader.ReadInt32();                  // Int32
            var freqTable = new Dictionary<byte, int>();
            for (int j = 0; j < tableSize; j++) {
                byte symbol = reader.ReadByte();                 // Byte
                int freq = reader.ReadInt32();                   // Int32
                freqTable[symbol] = freq;
            }

            // === Read compressed data ===
            int encodedLength = reader.ReadInt32();              // Int32
            byte[] encodedData = reader.ReadBytes(encodedLength); // Byte[]

            // === Reconstruct Huffman tree and decode ===
            var tree = BuildHuffTree(freqTable);
            var bitReader = new BitReader(encodedData);
            var outputBytes = new List<byte>();
            HuffmanNode current = tree;

            while (bitReader.ReadBit(out int bit)) {
                current = (bit == 0) ? current.Left : current.Right;

                if (current.IsLeaf) {
                    outputBytes.Add(current.Symbol.Value);
                    current = tree;
                }
            }

            string fullPath = Path.Combine(outputBaseDirectory, relativePath);
            result.Add(new FolderFileEntry(fullPath, relativePath, outputBytes.ToArray()));

            int percent = (int)(((i + 1) / (float)fileCount) * 100);
            progress?.Report(new ProgressReport {
                FileName = relativePath,
                Percentage = percent
            });
        }

        return result;
    }

}
