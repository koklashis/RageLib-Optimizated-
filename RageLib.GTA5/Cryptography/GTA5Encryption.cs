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

using RageLib.Cryptography;
using RageLib.GTA5.Cryptography.Helpers;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RageLib.GTA5.Cryptography
{
    /// <summary>
    /// Represents a GTA5 encryption algorithm.
    /// </summary>
    public class GTA5Crypto : IEncryptionAlgorithm
    {
        public byte[] Key { get; set; }

        ////////////////////////////////////////////////////////////////////////////
        // decryption
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Decrypts data.
        /// </summary>
        public byte[] Decrypt(byte[] data)
        {
            return Decrypt(data, Key);
        }

        /// <summary>
        /// Decrypts data.
        /// </summary>
        public static byte[] Decrypt(byte[] data, byte[] key)
        {
            var decryptedData = new byte[data.Length];

            var keyuints = new uint[key.Length / 4];
            Buffer.BlockCopy(key, 0, keyuints, 0, key.Length);

            var tables = GTA5Constants.PC_NG_DECRYPT_TABLES;
            int numBlocks = data.Length / 16;

            if (numBlocks > 512)
            {
                Parallel.For(0, numBlocks, blockIndex =>
                {
                    DecryptBlockFast(data, blockIndex * 16, decryptedData, blockIndex * 16, keyuints, tables);
                });
            }
            else
            {
                for (int blockIndex = 0; blockIndex < numBlocks; blockIndex++)
                {
                    DecryptBlockFast(data, blockIndex * 16, decryptedData, blockIndex * 16, keyuints, tables);
                }
            }

            int remainder = data.Length % 16;
            if (remainder != 0)
            {
                Buffer.BlockCopy(data, data.Length - remainder, decryptedData, data.Length - remainder, remainder);
            }

            return decryptedData;
        }

        /// <summary>
        /// Decrypts data in-place without allocating a new buffer.
        /// </summary>
        public static void DecryptInPlace(byte[] data, byte[] key)
        {
            DecryptInPlace(data, 0, data.Length, key);
        }

        /// <summary>
        /// Decrypts a range of data in-place without allocating a new buffer.
        /// </summary>
        public static void DecryptInPlace(byte[] data, int offset, int length, byte[] key)
        {
            var keyuints = new uint[key.Length / 4];
            Buffer.BlockCopy(key, 0, keyuints, 0, key.Length);

            var tables = GTA5Constants.PC_NG_DECRYPT_TABLES;
            int numBlocks = length / 16;

            if (numBlocks > 512)
            {
                Parallel.For(0, numBlocks, blockIndex =>
                {
                    int blockOffset = offset + blockIndex * 16;
                    DecryptBlockFast(data, blockOffset, data, blockOffset, keyuints, tables);
                });
            }
            else
            {
                for (int blockIndex = 0; blockIndex < numBlocks; blockIndex++)
                {
                    int blockOffset = offset + blockIndex * 16;
                    DecryptBlockFast(data, blockOffset, data, blockOffset, keyuints, tables);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DecryptBlockFast(byte[] src, int srcOffset, byte[] dst, int dstOffset, uint[] keyuints, uint[][][] tab)
        {
            // Initial load from src
            byte d0 = src[srcOffset + 0];
            byte d1 = src[srcOffset + 1];
            byte d2 = src[srcOffset + 2];
            byte d3 = src[srcOffset + 3];
            byte d4 = src[srcOffset + 4];
            byte d5 = src[srcOffset + 5];
            byte d6 = src[srcOffset + 6];
            byte d7 = src[srcOffset + 7];
            byte d8 = src[srcOffset + 8];
            byte d9 = src[srcOffset + 9];
            byte d10 = src[srcOffset + 10];
            byte d11 = src[srcOffset + 11];
            byte d12 = src[srcOffset + 12];
            byte d13 = src[srcOffset + 13];
            byte d14 = src[srcOffset + 14];
            byte d15 = src[srcOffset + 15];

            // Round 0 (RoundA)
            var t = tab[0];
            uint x1 = t[0][d0] ^ t[1][d1] ^ t[2][d2] ^ t[3][d3] ^ keyuints[0];
            uint x2 = t[4][d4] ^ t[5][d5] ^ t[6][d6] ^ t[7][d7] ^ keyuints[1];
            uint x3 = t[8][d8] ^ t[9][d9] ^ t[10][d10] ^ t[11][d11] ^ keyuints[2];
            uint x4 = t[12][d12] ^ t[13][d13] ^ t[14][d14] ^ t[15][d15] ^ keyuints[3];

            // Round 1 (RoundA)
            t = tab[1];
            uint nx1 = t[0][(byte)x1] ^ t[1][(byte)(x1 >> 8)] ^ t[2][(byte)(x1 >> 16)] ^ t[3][(byte)(x1 >> 24)] ^ keyuints[4];
            uint nx2 = t[4][(byte)x2] ^ t[5][(byte)(x2 >> 8)] ^ t[6][(byte)(x2 >> 16)] ^ t[7][(byte)(x2 >> 24)] ^ keyuints[5];
            uint nx3 = t[8][(byte)x3] ^ t[9][(byte)(x3 >> 8)] ^ t[10][(byte)(x3 >> 16)] ^ t[11][(byte)(x3 >> 24)] ^ keyuints[6];
            uint nx4 = t[12][(byte)x4] ^ t[13][(byte)(x4 >> 8)] ^ t[14][(byte)(x4 >> 16)] ^ t[15][(byte)(x4 >> 24)] ^ keyuints[7];
            x1 = nx1; x2 = nx2; x3 = nx3; x4 = nx4;

            // Rounds 2..15 (RoundB)
            for (int k = 2; k <= 15; k++)
            {
                t = tab[k];
                int kIdx = k * 4;
                nx1 = t[0][(byte)x1] ^ t[7][(byte)(x2 >> 24)] ^ t[10][(byte)(x3 >> 16)] ^ t[13][(byte)(x4 >> 8)] ^ keyuints[kIdx + 0];
                nx2 = t[1][(byte)(x1 >> 8)] ^ t[4][(byte)x2] ^ t[11][(byte)(x3 >> 24)] ^ t[14][(byte)(x4 >> 16)] ^ keyuints[kIdx + 1];
                nx3 = t[2][(byte)(x1 >> 16)] ^ t[5][(byte)(x2 >> 8)] ^ t[8][(byte)x3] ^ t[15][(byte)(x4 >> 24)] ^ keyuints[kIdx + 2];
                nx4 = t[3][(byte)(x1 >> 24)] ^ t[6][(byte)(x2 >> 16)] ^ t[9][(byte)(x3 >> 8)] ^ t[12][(byte)x4] ^ keyuints[kIdx + 3];
                x1 = nx1; x2 = nx2; x3 = nx3; x4 = nx4;
            }

            // Round 16 (RoundA)
            t = tab[16];
            nx1 = t[0][(byte)x1] ^ t[1][(byte)(x1 >> 8)] ^ t[2][(byte)(x1 >> 16)] ^ t[3][(byte)(x1 >> 24)] ^ keyuints[64];
            nx2 = t[4][(byte)x2] ^ t[5][(byte)(x2 >> 8)] ^ t[6][(byte)(x2 >> 16)] ^ t[7][(byte)(x2 >> 24)] ^ keyuints[65];
            nx3 = t[8][(byte)x3] ^ t[9][(byte)(x3 >> 8)] ^ t[10][(byte)(x3 >> 16)] ^ t[11][(byte)(x3 >> 24)] ^ keyuints[66];
            nx4 = t[12][(byte)x4] ^ t[13][(byte)(x4 >> 8)] ^ t[14][(byte)(x4 >> 16)] ^ t[15][(byte)(x4 >> 24)] ^ keyuints[67];

            // Direct store to dst
            dst[dstOffset + 0] = (byte)nx1;
            dst[dstOffset + 1] = (byte)(nx1 >> 8);
            dst[dstOffset + 2] = (byte)(nx1 >> 16);
            dst[dstOffset + 3] = (byte)(nx1 >> 24);

            dst[dstOffset + 4] = (byte)nx2;
            dst[dstOffset + 5] = (byte)(nx2 >> 8);
            dst[dstOffset + 6] = (byte)(nx2 >> 16);
            dst[dstOffset + 7] = (byte)(nx2 >> 24);

            dst[dstOffset + 8] = (byte)nx3;
            dst[dstOffset + 9] = (byte)(nx3 >> 8);
            dst[dstOffset + 10] = (byte)(nx3 >> 16);
            dst[dstOffset + 11] = (byte)(nx3 >> 24);

            dst[dstOffset + 12] = (byte)nx4;
            dst[dstOffset + 13] = (byte)(nx4 >> 8);
            dst[dstOffset + 14] = (byte)(nx4 >> 16);
            dst[dstOffset + 15] = (byte)(nx4 >> 24);
        }

        public static byte[] DecryptBlock(byte[] data, uint[] key)
        {
            var result = new byte[16];
            DecryptBlockFast(data, 0, result, 0, key, GTA5Constants.PC_NG_DECRYPT_TABLES);
            return result;
        }

        // round 1,2,16 (preserved for backwards compatibility)
        public static byte[] DecryptRoundA(byte[] data, uint[] key, uint[][] table)
        {
            var x1 =
                table[0][data[0]] ^
                table[1][data[1]] ^
                table[2][data[2]] ^
                table[3][data[3]] ^
                key[0];
            var x2 =
                table[4][data[4]] ^
                table[5][data[5]] ^
                table[6][data[6]] ^
                table[7][data[7]] ^
                key[1];
            var x3 =
                table[8][data[8]] ^
                table[9][data[9]] ^
                table[10][data[10]] ^
                table[11][data[11]] ^
                key[2];
            var x4 =
                table[12][data[12]] ^
                table[13][data[13]] ^
                table[14][data[14]] ^
                table[15][data[15]] ^
                key[3];

            var result = new byte[16];
            result[0] = (byte)(x1 & 0xFF);
            result[1] = (byte)((x1 >> 8) & 0xFF);
            result[2] = (byte)((x1 >> 16) & 0xFF);
            result[3] = (byte)((x1 >> 24) & 0xFF);
            result[4] = (byte)(x2 & 0xFF);
            result[5] = (byte)((x2 >> 8) & 0xFF);
            result[6] = (byte)((x2 >> 16) & 0xFF);
            result[7] = (byte)((x2 >> 24) & 0xFF);
            result[8] = (byte)(x3 & 0xFF);
            result[9] = (byte)((x3 >> 8) & 0xFF);
            result[10] = (byte)((x3 >> 16) & 0xFF);
            result[11] = (byte)((x3 >> 24) & 0xFF);
            result[12] = (byte)(x4 & 0xFF);
            result[13] = (byte)((x4 >> 8) & 0xFF);
            result[14] = (byte)((x4 >> 16) & 0xFF);
            result[15] = (byte)((x4 >> 24) & 0xFF);
            return result;
        }

        // round 3-15 (preserved for backwards compatibility)
        public static byte[] DecryptRoundB(byte[] data, uint[] key, uint[][] table)
        {
            var x1 =
                table[0][data[0]] ^
                table[7][data[7]] ^
                table[10][data[10]] ^
                table[13][data[13]] ^
                key[0];
            var x2 =
                table[1][data[1]] ^
                table[4][data[4]] ^
                table[11][data[11]] ^
                table[14][data[14]] ^
                key[1];
            var x3 =
                table[2][data[2]] ^
                table[5][data[5]] ^
                table[8][data[8]] ^
                table[15][data[15]] ^
                key[2];
            var x4 =
                table[3][data[3]] ^
                table[6][data[6]] ^
                table[9][data[9]] ^
                table[12][data[12]] ^
                key[3];

            var result = new byte[16];
            result[0] = (byte)((x1 >> 0) & 0xFF);
            result[1] = (byte)((x1 >> 8) & 0xFF);
            result[2] = (byte)((x1 >> 16) & 0xFF);
            result[3] = (byte)((x1 >> 24) & 0xFF);
            result[4] = (byte)((x2 >> 0) & 0xFF);
            result[5] = (byte)((x2 >> 8) & 0xFF);
            result[6] = (byte)((x2 >> 16) & 0xFF);
            result[7] = (byte)((x2 >> 24) & 0xFF);
            result[8] = (byte)((x3 >> 0) & 0xFF);
            result[9] = (byte)((x3 >> 8) & 0xFF);
            result[10] = (byte)((x3 >> 16) & 0xFF);
            result[11] = (byte)((x3 >> 24) & 0xFF);
            result[12] = (byte)((x4 >> 0) & 0xFF);
            result[13] = (byte)((x4 >> 8) & 0xFF);
            result[14] = (byte)((x4 >> 16) & 0xFF);
            result[15] = (byte)((x4 >> 24) & 0xFF);
            return result;
        }
    }
}
