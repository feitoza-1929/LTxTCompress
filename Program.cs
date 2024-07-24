using System.Collections;
using System.Text;

PROGRAM_STATE actualState = PROGRAM_STATE.START;

while (true)
{
    switch (actualState)
    {
        case PROGRAM_STATE.START:
            char character = (char)Console.Read();

            actualState = character switch
            {
                'c' => PROGRAM_STATE.COMPRESS,
                'd' => PROGRAM_STATE.DESCOMPRESS,
                _ => throw new Exception("ERROR::INVALID_OPTION")
            };
            Console.Clear();
            break;
        case PROGRAM_STATE.COMPRESS:
            string? filePath = Console.ReadLine().Trim();
            Dictionary<char, int> frequencies = GetFrequencies(filePath);
            Stack<Node> heap = MakeHeap(frequencies);
            Node tree = BuildHuffmanTree(heap);
            Dictionary<char, string> codes = []; 
            GenerateCodes(tree, "", ref codes);
            Compress(frequencies, codes, filePath);
            actualState = PROGRAM_STATE.EXIT;    
            break;
        case PROGRAM_STATE.DESCOMPRESS:
            string? compressedFilePath = Console.ReadLine().Trim();
            string? saveToPath = Console.ReadLine().Trim();
            Descompress(compressedFilePath, saveToPath);
            actualState = PROGRAM_STATE.EXIT;
            break;
        case PROGRAM_STATE.EXIT:
            Environment.Exit(0);
            break;
    }
}

Dictionary<char, int> GetFrequencies(string? filePath)
{
    if(!File.Exists(filePath))
        throw new Exception("ERROR::FILE_NOT_FOUND");


    using StreamReader sr = File.OpenText(filePath);
    Dictionary<char, int> frequencies = [];
    char c;

    while ((c = (char)sr.Read()) != '\uffff')
    {
        if(frequencies.TryGetValue(c, out int _))
        {
            frequencies[c]++;
        }
        else
        {
            frequencies.Add(c, 1);
        }
    }

    sr.Close();
    return frequencies;
}

Node GetLowestNode(ref Stack<Node> heap)
{
    Node lowest = heap.Pop();
    for (int i = 0; i < heap.Count; i++)
    {
        Node temp = heap.Pop();
        if(lowest.Frequency > temp.Frequency)
        {
            heap.Push(lowest);
            lowest = temp;
        }
        else
        {
            heap.Push(temp);
        }
    }

    return lowest;
}

Stack<Node> MakeHeap(Dictionary<char, int> frequencies)
{
    Stack<Node> heap = new Stack<Node>();
    foreach (var item in frequencies)
    {
        heap.Push(new Node(){ Character = item.Key, Frequency = item.Value });
    }
    return heap;
}

Node BuildHuffmanTree(Stack<Node> heap)
{
    while(heap.Count > 1)
    {
        Node left = GetLowestNode(ref heap);
        Node right = GetLowestNode(ref heap);
        Node top = new()
        {
            Character = '\0',
            Frequency = left.Frequency + right.Frequency,
            Left = left,
            Right = right
        };

        heap.Push(top);
    }

    return heap.Pop();
}

void GenerateCodes(Node node, string code, ref Dictionary<char, string> codes)
{
    if (node == null)
        return;

    if (node.Character != '\0')
        codes[node.Character] = code;

    GenerateCodes(node.Left, code + "0", ref codes);
    GenerateCodes(node.Right, code + "1", ref codes);
}

void Compress(Dictionary<char, int> frequencies, Dictionary<char, string> codes, string filePath)
{
    using Stream stream = File.Open(filePath.Replace(".txt", ".ltxt"), FileMode.Create);
    using BinaryWriter writer = new BinaryWriter(stream);

    // diff bits flag
    writer.Write((byte)0);
    // total unique chars
    writer.Write((byte)frequencies.Count);

    foreach (var frequency  in frequencies)
    {
        writer.Write((byte)frequency.Key);
        writer.Write(codes[frequency.Key]);
    }

    using StreamReader reader = File.OpenText("dummy.txt");
    char c;

    bool[] bits = [];
    while ((c = (char)reader.Read()) != '\uffff')
    {
        for (int i = 0; i < codes[c].Length; i++)
        {
            if(bits.Length == 7)
            {
                bits = bits.Append(codes[c][i] == '1').ToArray();
                BitArray bitBuffer = new(bits);
                byte[] byteBuffer = new byte[1];
                bitBuffer.CopyTo(byteBuffer, 0);
                writer.Write(byteBuffer[0]);

                bits = [];
            }

            bits = bits.Append(codes[c][i] == '1').ToArray();
        }
    }

    if(bits.Length > 0 && bits.Length < 8)
    {
        BitArray bitBuffer = new(bits);
        byte[] byteBuffer = new byte[1];
        bitBuffer.CopyTo(byteBuffer, 0);
        writer.Write(byteBuffer[0]);
    }

    if (bits.Length > 0)
    {
        writer.BaseStream.Position = 0;
        writer.Write((byte)(8 - bits.Length));
    }

    stream.Close();
    writer.Close();
    reader.Close();
}

void Descompress(string? filePath, string? saveToPath)
{
    if (!File.Exists(filePath))
        throw new Exception("ERROR::FILE_NOT_FOUND");

    using Stream stream = File.Open(filePath, FileMode.Open);
    using BinaryReader reader = new(stream, Encoding.UTF8);

    byte diffBitsFlag = reader.ReadByte();
    byte uniqueChars = reader.ReadByte();
    Dictionary<char, string> codes = [];
    
    for(int i = 0; i < uniqueChars; i++)
    {
        char character = reader.ReadChar();
        string code = reader.ReadString();
        codes.Add(character, code);
    }

    Node tree = RebuildHuffmanTree(codes);
    Node? currentRoot = tree;
    using StreamWriter writer = new(Path.Combine(saveToPath, "text.txt"));

    double totalBytes = Math.Ceiling((reader.BaseStream.Length - reader.BaseStream.Position)/8.0);
    for (long i = 0; i < totalBytes; i++)
    {
        BitArray bitBuffer = new(new byte[] { reader.ReadByte() });
        bool[] bits = new bool[8];
        bitBuffer.CopyTo(bits, 0);

        if (totalBytes - i == 1 && diffBitsFlag > 0)
            bits = bits[0..(7 - (diffBitsFlag - 1))];

        for(int j = 0;  j < bits.Length; j++)
        {
            if(bits[j])
            {
                if (currentRoot?.Character == '\0')
                    currentRoot = currentRoot.Right;
                
                if(currentRoot?.Character != '\0')
                {
                    writer.Write(currentRoot!.Character);
                    currentRoot = tree;
                }
            }

            if(bits[j] is false)
            {
                if (currentRoot?.Character == '\0')
                    currentRoot = currentRoot.Left;

                if (currentRoot?.Character != '\0')
                {
                    writer.Write(currentRoot!.Character);
                    currentRoot = tree;
                }
            }
        }
    }
    
    stream.Close();
    reader.Close();
    writer.Close();
}

Node RebuildHuffmanTree(Dictionary<char, string> codes)
{
    Node root = new(){Character = '\0'};
    foreach(KeyValuePair<char, string> code in codes)
    {
        Node current = root;
        for(int i = 0; i < code.Value.Length; i++)
        {
            if(code.Value[i] == '0')
            {
                current.Left ??= new(){Character = '\0'};
                current = current.Left;
            }
            else
            {
                current.Right ??= new(){Character = '\0'};
                current = current.Right;
            }
        }
        current.Character = code.Key;
    }

    return root;
}

class Node
{
    public char Character { get; set; }
    public int Frequency { get; set; }
    public Node? Right { get; set; }
    public Node? Left { get; set; }
}

enum PROGRAM_STATE
{
    START,
    COMPRESS,
    DESCOMPRESS,
    EXIT
}



