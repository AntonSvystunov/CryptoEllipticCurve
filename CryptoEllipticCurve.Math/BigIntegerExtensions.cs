using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CryptoEllipticCurve.Math
{
    static class BigIntegerExtensions
    {
        static readonly BigInteger FastSqrtSmallNumber = 4503599761588224UL; 
        static BigInteger SqrtFast(BigInteger value)
        {
            if (value <= FastSqrtSmallNumber)
            {
                if (value.Sign < 0) throw new ArgumentException("Negative argument.");
                return (ulong)System.Math.Sqrt((ulong)value);
            }

            BigInteger root; 
            int byteLen = value.ToByteArray().Length;
            if (byteLen < 128) 
            {
                root = (BigInteger)System.Math.Sqrt((double)value);
            }
            else 
            {
                root = (BigInteger)System.Math.Sqrt((double)(value >> (byteLen - 127) * 8)) << (byteLen - 127) * 4;
            }

            for (;;)
            {
                var root2 = value / root + root >> 1;
                if (root2 == root || root2 == root + 1) return root;
                root = value / root2 + root2 >> 1;
                if (root == root2 || root == root2 + 1) return root2;
            }
        }

        public static BigInteger SqrtGF(this BigInteger value)
        {
            return SqrtFast(value);
        }

        public static int GetBitCount(this BigInteger a)
        {
            return (int)BigInteger.Log(a, 2) + 1;
        }

        public static int LSB(this BigInteger a)
        {
            return (a == 0 ? -1 : (int)BigInteger.Log(a & -a, 2));
        }
    }
}
