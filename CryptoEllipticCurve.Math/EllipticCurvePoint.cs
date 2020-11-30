using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CryptoEllipticCurve.Math
{
    public class EllipticCurvePoint 
    {
        public BigInteger X { get; private set; }
        public BigInteger Y { get; private set; }
        public EllipticCurvePoint(BigInteger x, BigInteger y)
        {
            X = x;
            Y = y;
        }

        public static EllipticCurvePoint InfinityPoint = null;

        public static bool IsInfinity(EllipticCurvePoint point) => point == InfinityPoint;
    }
}
