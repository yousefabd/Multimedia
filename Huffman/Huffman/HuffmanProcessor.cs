using Huffman.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huffman.Huffman;

public class HuffmanProcessor
{
    // public Dictionary<byte, string> CodeTable { get; set; } = [];
    public Dictionary<byte, int> MakeRepitionsTable(byte[] bytes)
    {
        var frequencies = new Dictionary<byte, int>();
        foreach (var byt in bytes)
        {
            if (!frequencies.ContainsKey(byt))
            {
                frequencies.Add(byt, 0);
            }
            frequencies[byt]++;
        }
        return frequencies;
    }
    public HuffmanNode BuildHuffTree(Dictionary<byte, int> frequencies)
    {
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
    public void BuildCodeTable(HuffmanNode node, string code, Dictionary<byte, string> codeTable)
    {
        if (node is null)
        {
            return;
        }
        if (node.IsLeaf && node.Symbol.HasValue)
        {
            codeTable[node.Symbol.Value] = code;

        }
        BuildCodeTable(node.Left, code + '0', codeTable);
        BuildCodeTable(node.Right, code + '1', codeTable);

    }
    public HuffmanCompressedFile HuffmanCompress(string inputPath)
    {
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
    public void Compress(List<FolderFileEntry> files, string outputArchivePath)
    {
        using var fs = new BinaryWriter(File.Open(outputArchivePath, FileMode.Create));

        // === Write number of files ===
        fs.Write(files.Count);

        foreach (FolderFileEntry entry in files) {
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
        }
    }
    public void Decompress(string archivePath, string outputDirectory)
    {
        using var reader = new BinaryReader(File.Open(archivePath, FileMode.Open));

        // === Read number of files ===
        int fileCount = reader.ReadInt32(); // Int32: number of files

        for (int i = 0; i < fileCount; i++) {
            // === Read relative path ===
            int pathLength = reader.ReadInt32();           // Int32
            byte[] pathBytes = reader.ReadBytes(pathLength); // Byte[]
            string relativePath = Encoding.UTF8.GetString(pathBytes);

            // === Read frequency table ===
            int tableSize = reader.ReadInt32();            // Int32
            var freqTable = new Dictionary<byte, int>();
            for (int j = 0; j < tableSize; j++) {
                byte symbol = reader.ReadByte();           // Byte
                int freq = reader.ReadInt32();             // Int32
                freqTable[symbol] = freq;
            }

            // === Read compressed data ===
            int encodedLength = reader.ReadInt32();        // Int32
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

            // === Save decompressed file using full path ===
            string outputFilePath = Path.Combine(outputDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath)!);
            File.WriteAllBytes(outputFilePath, outputBytes.ToArray());
        }
    }
}
