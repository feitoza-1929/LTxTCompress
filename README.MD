
![Little Text Compress logo](/assets/ltxtcompress_logo.png)

This is a simple console application for compressing and decompressing text, created to explore compression algorithms and data structures.

## Summary
- [Running the Project](#running-the-project)
    - [Requirements](#requirements)
    - [Build and Run](#build-and-run)
- [User Guide](#user-guide)
- [Making Of](#making-of)
    - [Compression](#compression)
    - [Decompression](#decompression)

## Running the Project
You'll need the following dependencies:

### Requirements
- [.NET 8.X](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Linux, Windows, or MacOS

### Build and Run
To build and run the project, open your command line or terminal and enter the following command in the project root directory:

**Linux/Windows/MacOS 64 bits**
```sh
dotnet run LTxTCompress.csproj
```

## User Guide
The application can compress and decompress text files. Upon running the project, the console is able to enter commands. For compression enter `c fileToCompressPath` or `d compressedFilePath extractToPath` for descompresion.

## Making Of
The core of this project is Huffman's algorithm, known for its lossless data compression, making it ideal for text-based data.

### Compression
1. **Character Frequency**: The application calculates the frequency of each character in the text file.
2. **Huffman Tree**: Using these frequencies, a Huffman tree is constructed. Nodes with the lowest frequencies are combined into a new node, which is then reinserted into the heap. This process continues until only one node remains.
3. **Generating Codes**: The tree is traversed to generate unique codes for each character. A table of these codes is created to assist with decompression.

### Decompression
1. **Reading Metadata**: The application reads the metadata containing the characters and their codes, rebuilds the Huffman tree, and then reads the compressed file bit by bit to reconstruct the original text.
