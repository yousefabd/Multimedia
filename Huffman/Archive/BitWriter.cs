using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huffman.Archive;

public class BitWriter
{
    private List<byte> buffer = new List<byte>();
    private int bitPos = 0;
    private byte currentByte = 0;
    public void WriteBit(int bit)
    {
        if (bit != 0) currentByte |= (byte)(1 << 7 - bitPos);
        bitPos++;
        if (bitPos == 8)
        {
            buffer.Add(currentByte);
            bitPos = 0;
            currentByte = 0;
        }
    }

    public void WriteBits(string bits)
    {
        foreach (var bit in bits)
        {
            WriteBit(bit == '1' ? 1 : 0);
        }
    }

    public byte[] GetBytes()
    {
        if (bitPos > 0) buffer.Add(currentByte);
        return buffer.ToArray();
    }
}
public class BitReader
{
    private byte[] data;
    private int byteIndex = 0;
    private int bitPos = 0;

    public BitReader(byte[] input)
    {
        data = input;
    }

    public bool ReadBit(out int bit)
    {
        if (byteIndex >= data.Length)
        {
            bit = 0;
            return false;
        }

        bit = data[byteIndex] >> 7 - bitPos & 1;
        bitPos++;
        if (bitPos == 8)
        {
            bitPos = 0;
            byteIndex++;
        }
        return true;
    }
}

