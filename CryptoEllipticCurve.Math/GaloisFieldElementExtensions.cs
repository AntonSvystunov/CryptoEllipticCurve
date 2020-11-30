using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CryptoEllipticCurve.Math
{
    public static class GaloisFieldElementExtensions
    {
        public static BigInteger AddGF(this BigInteger a, BigInteger b)
        {
            return a ^ b;
        }

        public static BigInteger MultGF(this BigInteger a, BigInteger b, BigInteger modulus)
        {
            BigInteger val_c = 0;
            int range = a.GetBitCount();
            for (int j = 0; j < range; j++)
            {
                var t = (a & (BigInteger.One << j));
                if (t > 0)
                {
                    val_c ^= b;
                }

                b <<= 1;
            }

            return val_c.ModulusGF(modulus);
        }

        public static BigInteger ModulusGF(this BigInteger val, BigInteger modulus)
        {
            if (val <= modulus)
            {
                return val;
            }

            BigInteger rv = val;
            var bitm_l = modulus.GetBitCount();
            while (rv.GetBitCount() >= bitm_l)
            {
                BigInteger mask = modulus << (rv.GetBitCount() - bitm_l);
                rv = rv ^ mask;
            }

            return rv;
        }

        public static BigInteger SquareGF(this BigInteger value, BigInteger modulus)
        {
            return value.MultGF(value, modulus);
        }

        public static BigInteger Trace(this BigInteger value, BigInteger modulus)
        {
            BigInteger result = value;
            int bitLength = modulus.GetBitCount();
            for (int i = 1; i < bitLength - 1; i++)
            {
                result = result.SquareGF(modulus).AddGF(value);
            }
            
            return result;
        }

        public static BigInteger HalfTrace(this BigInteger value, BigInteger modulus)
        {
            BigInteger result = value;
            int bitLength = modulus.GetBitCount();
            for (int i = 1; i <= (bitLength - 1) / 2; i++)
            {
                result = result.SquareGF(modulus).SquareGF(modulus).AddGF(value);
            }

            return result;
        }

        public static BigInteger InverseGF(this BigInteger value, BigInteger modulus)
        {
            BigInteger b = 1, c = 0, u = value, v = modulus;

            while (u.GetBitCount() > 1)
            {
                int j = u.GetBitCount() - v.GetBitCount();
                if (j < 0)
                {
                    (u, v) = (v, u);
                    (c, b) = (b, c);
                    j = -j;
                }
                u = u.AddGF(v << j);
                b = b.AddGF(c << j);
            }

            return b;
        }

        public static BigInteger SqrtGF(this BigInteger value, BigInteger modulus)
        {
            throw new NotImplementedException();
        }
    }
}
