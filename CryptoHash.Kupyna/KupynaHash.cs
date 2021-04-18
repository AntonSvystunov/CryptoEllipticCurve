using System;
using System.Linq;
using System.Security.Cryptography;
using static CryptoHash.Kupyna.KupynaTransformation;

namespace CryptoHash.Kupyna
{
    public class KupynaHash : HashAlgorithm
    {
        private byte[] _buffer;
        private long _count;
        private byte[,] _state;

        private int columns;
        private int nbytes;

        public KupynaHash(int hashSize)
        {
            SetHashSize(hashSize);

            _state = new byte[columns, 8];
            _buffer = new byte[nbytes];

            SetInitialStateValues();
        }

        private void SetHashSize(int hashSize)
        {
            if ((hashSize % 8 != 0) || (hashSize > 512))
            {
                throw new ArgumentException($"{nameof(hashSize)} should be less or equal to 512 bits and be a valid byte size");
            }

            if (hashSize <= 256)
            {
                columns = 8;
                nbytes = columns * 8;
            }
            else
            {
                columns = 16;
                nbytes = columns * 8;
            }

            HashSizeValue = hashSize;
        }

        public override void Initialize()
        {
            Array.Clear(_state, 0, _state.Length);
            Array.Clear(_buffer, 0, _buffer.Length);
            SetInitialStateValues();
        }

        private void SetInitialStateValues()
        {
            _count = 0;
            _state[0, 0] = (byte)(_buffer.Length);
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            HashData(array, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            int buffPos = (int)_count & 0X3F;

            int blockLen = nbytes - buffPos;
            if (blockLen <= 12)
            {
                blockLen += nbytes;
            }

            byte[] paddingBlock = new byte[blockLen];


            long bitCount = _count * 8;
            paddingBlock[0] = 0x01;

            paddingBlock[blockLen - 12] = (byte)((bitCount >> 88) & 0xff);
            paddingBlock[blockLen - 11] = (byte)((bitCount >> 80) & 0xff);
            paddingBlock[blockLen - 10] = (byte)((bitCount >> 72) & 0xff);
            paddingBlock[blockLen - 9] = (byte)((bitCount >> 64) & 0xff);
            paddingBlock[blockLen - 8] = (byte)((bitCount >> 56) & 0xff);
            paddingBlock[blockLen - 7] = (byte)((bitCount >> 48) & 0xff);
            paddingBlock[blockLen - 6] = (byte)((bitCount >> 40) & 0xff);
            paddingBlock[blockLen - 5] = (byte)((bitCount >> 32) & 0xff);
            paddingBlock[blockLen - 4] = (byte)((bitCount >> 24) & 0xff);
            paddingBlock[blockLen - 3] = (byte)((bitCount >> 16) & 0xff);
            paddingBlock[blockLen - 2] = (byte)((bitCount >> 8) & 0xff);
            paddingBlock[blockLen - 1] = (byte)((bitCount >> 0) & 0xff);

            HashData(paddingBlock, 0, paddingBlock.Length);

            var hash = GetHash();
            HashValue = hash;
            return hash;
        }

        private byte[] GetHash()
        {
            byte[] hash = new byte[HashSizeValue / 8];

            byte[,] temp = new byte[columns, 8];
            Array.Copy(_state, temp, _state.Length);
            P(_buffer, temp);
            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < columns; ++j)
                {
                    _state[j, i] ^= temp[j, i];
                }
            }
            Trunc(hash);
            return hash;
        }

        private void Trunc(byte[] hash)
        {
            int hash_nbytes = HashSizeValue / 8;
            var flat = _state.Cast<byte>().ToArray();
            Array.Copy(flat, flat.Length - hash_nbytes, hash, 0, hash.Length);
        }

        private void HashData(byte[] array, int offset, int size)
        {
            int inOffset = offset;
            int inSize = size;
            int buffPos = (int)_count & 0X3F;

            _count += size;

            int hashSizeBytes = nbytes;

            if (buffPos > 0 && (buffPos + inSize) >= hashSizeBytes)
            {
                Buffer.BlockCopy(array, inOffset, _buffer, buffPos, hashSizeBytes - buffPos);
                inOffset += hashSizeBytes - buffPos;
                inSize -= hashSizeBytes - buffPos;
                Transform(_buffer, _state);
            }

            while (inSize >= hashSizeBytes)
            {
                Buffer.BlockCopy(array, inOffset, _buffer, 0, hashSizeBytes);
                inOffset += hashSizeBytes;
                inSize -= hashSizeBytes;
                Transform(_buffer, _state);
            }

            if (inSize > 0)
            {
                Buffer.BlockCopy(array, inOffset, _buffer, 0, inSize);
            }
        }

        private static void Transform(byte[] data, byte[,] state)
        {
            byte[,] temp1 = new byte[state.GetLength(0), state.GetLength(1)];
            byte[,] temp2 = new byte[state.GetLength(0), state.GetLength(1)];

            for (int i = 0; i < state.GetLength(1); ++i)
            {
                for (int j = 0; j < state.GetLength(0); ++j)
                {
                    temp1[j, i] = (byte)(state[j, i] ^ data[j * state.GetLength(1) + i]);
                    temp2[j, i] = data[j * state.GetLength(1) + i];
                }
            }
            P(data, temp1);
            Q(data, temp2);
            for (int i = 0; i < state.GetLength(1); ++i)
            {
                for (int j = 0; j < state.GetLength(0); ++j)
                {
                    state[j, i] ^= (byte)(temp1[j, i] ^ temp2[j, i]);
                }
            }
        }

        private static void P(byte[] data, byte[,] state)
        {
            int rounds = data.Length * 8 <= 256 ? 10 : 14;
            for (int i = 0; i < rounds; ++i)
            {
                AddRoundConstantQ(state, i);
                SubBytes(state);
                ShiftBytes(state);
                MixColumns(state);
            }
        }

        private static void Q(byte[] data, byte[,] state)
        {
            int rounds = data.Length * 8 <= 256 ? 10 : 14;
            for (int i = 0; i < rounds; ++i)
            {
                AddRoundConstantP(state, i);
                SubBytes(state);
                ShiftBytes(state);
                MixColumns(state);
            }
        }
    }
}
