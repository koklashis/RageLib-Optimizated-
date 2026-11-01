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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RageLib.Data
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct UIntFloatUnion
    {
        [FieldOffset(0)] public uint UIntValue;
        [FieldOffset(0)] public float FloatValue;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct ULongDoubleUnion
    {
        [FieldOffset(0)] public ulong ULongValue;
        [FieldOffset(0)] public double DoubleValue;
    }

    /// <summary>
    /// Represents a data reader.
    /// </summary>
    public class DataReader
    {
        protected Stream baseStream;
        protected readonly byte[] primitiveBuffer = new byte[8];

        /// <summary>
        /// Gets or sets the endianess of the underlying stream.
        /// </summary>
        public Endianess Endianess
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the length of the underlying stream.
        /// </summary>
        public virtual long Length
        {
            get
            {
                return baseStream.Length;
            }
        }

        /// <summary>
        /// Gets or sets the position within the underlying stream.
        /// </summary>
        public virtual long Position
        {
            get
            {
                return baseStream.Position;
            }
            set
            {
                baseStream.Position = value;
            }
        }

        /// <summary>
        /// Initializes a new data reader for the specified stream.
        /// </summary>
        public DataReader(Stream stream, Endianess endianess = Endianess.LittleEndian)
        {
            this.baseStream = stream;
            this.Endianess = endianess;
        }

        /// <summary>
        /// Reads bytes into the destination buffer without allocating.
        /// </summary>
        protected virtual void ReadBytesInternal(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            while (bytesRead < count)
            {
                int read = baseStream.Read(buffer, offset + bytesRead, count - bytesRead);
                if (read == 0) throw new EndOfStreamException();
                bytesRead += read;
            }
        }

        /// <summary>
        /// Reads data from the underlying stream.
        /// </summary>
        protected virtual byte[] ReadFromStream(int count, bool ignoreEndianess = false)
        {
            var buffer = new byte[count];
            ReadBytesInternal(buffer, 0, count);

            // handle endianess
            if (!ignoreEndianess && (Endianess == Endianess.BigEndian))
            {
                Array.Reverse(buffer);
            }

            return buffer;
        }

        /// <summary>
        /// Reads a byte without allocation.
        /// </summary>
        public byte ReadByte()
        {
            ReadBytesInternal(primitiveBuffer, 0, 1);
            return primitiveBuffer[0];
        }

        /// <summary>
        /// Reads a sequence of bytes.
        /// </summary>
        public byte[] ReadBytes(int count)
        {
            return ReadFromStream(count, true);
        }

        /// <summary>
        /// Reads a sequence of bytes into an existing buffer.
        /// </summary>
        public void ReadBytes(byte[] buffer, int offset, int count)
        {
            ReadBytesInternal(buffer, offset, count);
        }

        /// <summary>
        /// Reads a signed 16-bit value without allocation.
        /// </summary>
        public short ReadInt16()
        {
            ReadBytesInternal(primitiveBuffer, 0, 2);
            if (Endianess == Endianess.LittleEndian)
                return (short)(primitiveBuffer[0] | (primitiveBuffer[1] << 8));
            else
                return (short)((primitiveBuffer[0] << 8) | primitiveBuffer[1]);
        }

        /// <summary>
        /// Reads a signed 32-bit value without allocation.
        /// </summary>
        public int ReadInt32()
        {
            ReadBytesInternal(primitiveBuffer, 0, 4);
            if (Endianess == Endianess.LittleEndian)
                return primitiveBuffer[0] | (primitiveBuffer[1] << 8) | (primitiveBuffer[2] << 16) | (primitiveBuffer[3] << 24);
            else
                return (primitiveBuffer[0] << 24) | (primitiveBuffer[1] << 16) | (primitiveBuffer[2] << 8) | primitiveBuffer[3];
        }

        /// <summary>
        /// Reads a signed 64-bit value without allocation.
        /// </summary>
        public long ReadInt64()
        {
            ReadBytesInternal(primitiveBuffer, 0, 8);
            if (Endianess == Endianess.LittleEndian)
            {
                uint lo = (uint)(primitiveBuffer[0] | (primitiveBuffer[1] << 8) | (primitiveBuffer[2] << 16) | (primitiveBuffer[3] << 24));
                uint hi = (uint)(primitiveBuffer[4] | (primitiveBuffer[5] << 8) | (primitiveBuffer[6] << 16) | (primitiveBuffer[7] << 24));
                return (long)(((ulong)hi << 32) | lo);
            }
            else
            {
                uint hi = (uint)((primitiveBuffer[0] << 24) | (primitiveBuffer[1] << 16) | (primitiveBuffer[2] << 8) | primitiveBuffer[3]);
                uint lo = (uint)((primitiveBuffer[4] << 24) | (primitiveBuffer[5] << 16) | (primitiveBuffer[6] << 8) | primitiveBuffer[7]);
                return (long)(((ulong)hi << 32) | lo);
            }
        }

        /// <summary>
        /// Reads an unsigned 16-bit value without allocation.
        /// </summary>
        public ushort ReadUInt16()
        {
            return (ushort)ReadInt16();
        }

        /// <summary>
        /// Reads an unsigned 32-bit value without allocation.
        /// </summary>
        public uint ReadUInt32()
        {
            return (uint)ReadInt32();
        }

        /// <summary>
        /// Reads an unsigned 64-bit value without allocation.
        /// </summary>
        public ulong ReadUInt64()
        {
            return (ulong)ReadInt64();
        }

        /// <summary>
        /// Reads a single precision floating point value without allocation.
        /// </summary>
        public float ReadSingle()
        {
            uint val = ReadUInt32();
            var u = default(UIntFloatUnion);
            u.UIntValue = val;
            return u.FloatValue;
        }

        /// <summary>
        /// Reads a double precision floating point value without allocation.
        /// </summary>
        public double ReadDouble()
        {
            ulong val = ReadUInt64();
            var u = default(ULongDoubleUnion);
            u.ULongValue = val;
            return u.DoubleValue;
        }

        /// <summary>
        /// Reads a null-terminated UTF8 string.
        /// </summary>
        public string ReadString()
        {
            var bytes = new List<byte>();
            var temp = ReadByte();
            while (temp != 0)
            {
                bytes.Add(temp);
                temp = ReadByte();
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }
    }
}
