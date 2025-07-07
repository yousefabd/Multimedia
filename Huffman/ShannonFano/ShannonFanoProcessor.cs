using Huffman.Archive;
using Huffman.data;
using Huffman.Huffman;
using System.Text;

namespace Huffman.ShannonFano;
public class ShannonFanoProcessor : IArchiveProcessor {
    private Dictionary<byte, string> codeTable = new();

    public Dictionary<byte, int> MakeRepetitionTable(byte[] bytes) {
        var frequencies = new Dictionary<byte, int>();
        foreach (var b in bytes) {
            if (!frequencies.ContainsKey(b))
                frequencies[b] = 0;
            frequencies[b]++;
        }
        return frequencies;
    }

    public void BuildCodeTable(List<KeyValuePair<byte, int>> sortedFreqs, int start, int end, string prefix) {
        if (start == end) {
            codeTable[sortedFreqs[start].Key] = prefix == "" ? "0" : prefix;
            return;
        }

        int total = sortedFreqs.Skip(start).Take(end - start + 1).Sum(kv => kv.Value);
        int acc = 0, split = start;

        for (int i = start; i <= end; i++) {
            acc += sortedFreqs[i].Value;
            if (acc >= total / 2) {
                split = i;
                break;
            }
        }

        BuildCodeTable(sortedFreqs, start, split, prefix + "0");
        BuildCodeTable(sortedFreqs, split + 1, end, prefix + "1");
    }

    public HuffmanCompressedFile ShannonFanoCompress(string inputPath) {
        byte[] data = File.ReadAllBytes(inputPath);
        var freqTable = MakeRepetitionTable(data);
        var sorted = freqTable.OrderByDescending(kv => kv.Value).ToList();

        codeTable.Clear();
        BuildCodeTable(sorted, 0, sorted.Count - 1, "");

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
        int i = 0;
        long totalOriginalSize = 0;
        foreach (var entry in files) {
            //check if cancelled
            cancellationToken.ThrowIfCancellationRequested();
            //check if paused
            pauseEvent.Wait();

            totalOriginalSize += new FileInfo(entry.FullPath).Length;
            var compressed = ShannonFanoCompress(entry.FullPath);
            byte[] relativePathBytes = Encoding.UTF8.GetBytes(entry.RelativePath);

            // === Write relative path ===
            fs.Write(relativePathBytes.Length);      // Int32
            fs.Write(relativePathBytes);             // UTF-8 bytes

            // === Write frequency table ===
            fs.Write(compressed.FrequencyTable.Count);
            foreach (var kv in compressed.FrequencyTable) {
                fs.Write(kv.Key);                    // Byte
                fs.Write(kv.Value);                  // Int32
            }

            fs.Write(compressed.EncodedData.Length); // Int32
            fs.Write(compressed.EncodedData);        // Byte[]

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
        int fileCount = reader.ReadInt32();

        for (int i = 0; i < fileCount; i++) {
            //check if cancelled
            cancellationToken.ThrowIfCancellationRequested();
            //check if paused
            pauseEvent.Wait();

            // === Read relative path ===
            int pathLength = reader.ReadInt32();                    // Int32
            byte[] pathBytes = reader.ReadBytes(pathLength);        // UTF-8 bytes
            string relativePath = Encoding.UTF8.GetString(pathBytes);

            // === Read frequency table ===
            int tableSize = reader.ReadInt32();
            var freqTable = new Dictionary<byte, int>();
            for (int j = 0; j < tableSize; j++) {
                byte b = reader.ReadByte();                         // Byte
                int freq = reader.ReadInt32();                      // Int32
                freqTable[b] = freq;
            }

            int encodedLength = reader.ReadInt32();                 // Int32
            byte[] encodedData = reader.ReadBytes(encodedLength);   // Byte[]

            // Rebuild code table
            var sorted = freqTable.OrderByDescending(kv => kv.Value).ToList();
            codeTable.Clear();
            BuildCodeTable(sorted, 0, sorted.Count - 1, "");
            var decodeMap = codeTable.ToDictionary(kv => kv.Value, kv => kv.Key);

            var bitReader = new BitReader(encodedData);
            var buffer = "";
            var output = new List<byte>();

            while (bitReader.ReadBit(out int bit)) {
                buffer += bit;
                if (decodeMap.TryGetValue(buffer, out byte decodedByte)) {
                    output.Add(decodedByte);
                    buffer = "";
                }
            }

            string fullPath = Path.Combine(outputBaseDirectory, relativePath);
            result.Add(new FolderFileEntry(fullPath, relativePath, output.ToArray()));

            int percent = (int)(((i + 1) / (float)fileCount) * 100);
            progress?.Report(new ProgressReport {
                FileName = relativePath,
                Percentage = percent
            });
        }

        return result;
    }
}