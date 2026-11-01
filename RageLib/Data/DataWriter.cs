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
using System.IO;

namespace RageLib.Data
{
    /// <summary>
    /// Represents a data writer.
    /// </summary>
    public class DataWriter
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
        /// Initializes a new data writer for the specified stream.
        /// </summary>
        public DataWriter(Stream stream, Endianess endianess = Endianess.LittleEndian)
        {
            this.baseStream = stream;
            this.Endianess = endianess;
        }

        /// <summary>
        /// Writes raw bytes to the underlying stream.
        /// </summary>
        protected virtual void WriteBytesInternal(byte[] buffer, int offset, int count)
        {
            baseStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes data to the underlying stream.
        /// </summary>
        protected virtual void WriteToStream(byte[] value, bool ignoreEndianess = false)
        {
            if (!ignoreEndianess && (Endianess == Endianess.BigEndian))
            {
                var buffer = (byte[])value.Clone();
                Array.Reverse(buffer);
                WriteBytesInternal(buffer, 0, buffer.Length);
            }
            else
            {
                WriteBytesInternal(value, 0, value.Length);
            }
        }

        /// <summary>
        /// Writes a byte without allocation.
        /// </summary>
        public void Write(byte value)
        {
            primitiveBuffer[0] = value;
            WriteBytesInternal(primitiveBuffer, 0, 1);
        }

        /// <summary>
        /// Writes a sequence of bytes.
        /// </summary>
        public void Write(byte[] value)
        {
            WriteBytesInternal(value, 0, value.Length);
        }

        /// <summary>
        /// Writes a sequence of bytes from an offset.
        /// </summary>
        public void Write(byte[] value, int offset, int count)
        {
            WriteBytesInternal(value, offset, count);
        }

        /// <summary>
        /// Writes a signed 16-bit value without allocation.
        /// </summary>
        public void Write(short value)
        {
            if (Endianess == Endianess.LittleEndian)
            {
                primitiveBuffer[0] = (byte)value;
                primitiveBuffer[1] = (byte)(value >> 8);
            }
            else
            {
                primitiveBuffer[0] = (byte)(value >> 8);
                primitiveBuffer[1] = (byte)value;
            }
            WriteBytesInternal(primitiveBuffer, 0, 2);
        }

        /// <summary>
        /// Writes a signed 32-bit value without allocation.
        /// </summary>
        public void Write(int value)
        {
            if (Endianess == Endianess.LittleEndian)
            {
                primitiveBuffer[0] = (byte)value;
                primitiveBuffer[1] = (byte)(value >> 8);
                primitiveBuffer[2] = (byte)(value >> 16);
                primitiveBuffer[3] = (byte)(value >> 24);
            }
            else
            {
                primitiveBuffer[0] = (byte)(value >> 24);
                primitiveBuffer[1] = (byte)(value >> 16);
                primitiveBuffer[2] = (byte)(value >> 8);
                primitiveBuffer[3] = (byte)value;
            }
            WriteBytesInternal(primitiveBuffer, 0, 4);
        }

        /// <summary>
        /// Writes a signed 64-bit value without allocation.
        /// </summary>
        public void Write(long value)
        {
            if (Endianess == Endianess.LittleEndian)
            {
                primitiveBuffer[0] = (byte)value;
                primitiveBuffer[1] = (byte)(value >> 8);
                primitiveBuffer[2] = (byte)(value >> 16);
                primitiveBuffer[3] = (byte)(value >> 24);
                primitiveBuffer[4] = (byte)(value >> 32);
                primitiveBuffer[5] = (byte)(value >> 40);
                primitiveBuffer[6] = (byte)(value >> 48);
                primitiveBuffer[7] = (byte)(value >> 56);
            }
            else
            {
                primitiveBuffer[0] = (byte)(value >> 56);
                primitiveBuffer[1] = (byte)(value >> 48);
                primitiveBuffer[2] = (byte)(value >> 40);
                primitiveBuffer[3] = (byte)(value >> 32);
                primitiveBuffer[4] = (byte)(value >> 24);
                primitiveBuffer[5] = (byte)(value >> 16);
                primitiveBuffer[6] = (byte)(value >> 8);
                primitiveBuffer[7] = (byte)value;
            }
            WriteBytesInternal(primitiveBuffer, 0, 8);
        }

        /// <summary>
        /// Writes an unsigned 16-bit value without allocation.
        /// </summary>
        public void Write(ushort value)
        {
            Write((short)value);
        }

        /// <summary>
        /// Writes an unsigned 32-bit value without allocation.
        /// </summary>
        public void Write(uint value)
        {
            Write((int)value);
        }

        /// <summary>
        /// Writes an unsigned 64-bit value without allocation.
        /// </summary>
        public void Write(ulong value)
        {
            Write((long)value);
        }

        /// <summary>
        /// Writes a single precision floating point value without allocation.
        /// </summary>
        public void Write(float value)
        {
            var u = default(UIntFloatUnion);
            u.FloatValue = value;
            Write(u.UIntValue);
        }

        /// <summary>
        /// Writes a double precision floating point value without allocation.
        /// </summary>
        public void Write(double value)
        {
            var u = default(ULongDoubleUnion);
            u.DoubleValue = value;
            Write(u.ULongValue);
        }

        /// <summary>
        /// Writes a null-terminated string.
        /// </summary>
        public void Write(string value)
        {
            foreach (var c in value)
                Write((byte)c);
            Write((byte)0);
        }
    }
}
