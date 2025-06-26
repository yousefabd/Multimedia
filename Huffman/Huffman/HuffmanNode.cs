using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huffman.Huffman;

public class HuffmanNode
{
    public byte? Symbol{ get; set; }//هاد هو المحرف
    public int Frequency{ get; set; }//هاد هو التكرار
    public HuffmanNode Left { get; set; }
    public HuffmanNode Right { get; set; }
    public bool IsLeaf => Left == null && Right == null;
}
