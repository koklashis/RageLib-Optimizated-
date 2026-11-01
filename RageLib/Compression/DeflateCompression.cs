/*
    Copyright(c) 2015 Neodymium

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

using System.IO;
using System.IO.Compression;

namespace RageLib.Compression
{
    /// <summary>
    /// Represents a deflate compression algorithm.
    /// </summary>
    public class DeflateCompression : ICompressionAlgorithm
    {
        /// <summary>
        /// Compresses data.
        /// </summary>
        public byte[] Compress(byte[] data)
        {
            return CompressData(data);
        }

        /// <summary>
        /// Decompresses data.
        /// </summary>
        public byte[] Decompress(byte[] data, int decompressedLength)
        {
            return DecompressData(data, decompressedLength);
        }

        /// <summary>
        /// Compresses data reliably without fixed-size memory stream crashes.
        /// </summary>
        public static byte[] CompressData(byte[] data)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(outputStream, CompressionMode.Compress, true))
                {
                    deflateStream.Write(data, 0, data.Length);
                }
                return outputStream.ToArray();
            }
        }

        /// <summary>
        /// Decompresses data safely reading all requested bytes.
        /// </summary>
        public static byte[] DecompressData(byte[] data, int decompressedLength)
        {
            var buffer = new byte[decompressedLength];
            using (var dataStream = new MemoryStream(data))
            using (var deflateStream = new DeflateStream(dataStream, CompressionMode.Decompress))
            {
                ReadFully(deflateStream, buffer, 0, decompressedLength);
            }
            return buffer;
        }

        /// <summary>
        /// Reads full count of bytes from stream into buffer.
        /// </summary>
        public static int ReadFully(Stream stream, byte[] buffer, int offset, int count)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = stream.Read(buffer, offset + totalRead, count - totalRead);
                if (read == 0)
                    break;
                totalRead += read;
            }
            return totalRead;
        }
    }
}
