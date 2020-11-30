using System;
using System.Numerics;

namespace CryptoEllipticCurve.Math
{
    public class EllipticCurve
    {
        public BigInteger A { get; private set; }
        public BigInteger B { get; private set; }
        public BigInteger P { get; private set; }
        public BigInteger N { get; private set; }
        public uint M { get; private set; }

        private EllipticCurvePoint basePoint;
        public EllipticCurvePoint BasePoint
        {
            get
            {
                return basePoint;
            }
            set
            {
                if (!IsOnCurve(value))
                {
                    throw new ArgumentException("Point is not on elliptic curve", nameof(value));
                }
                basePoint = value;
            }
        }

        public EllipticCurve(BigInteger a, BigInteger b, BigInteger p, BigInteger n, uint m, EllipticCurvePoint basePoint = null)
        {
            A = a;
            B = b;
            P = p;
            N = n;
            M = m;

            this.basePoint = basePoint;            
        }

        public bool IsOnCurve(EllipticCurvePoint point)
        {
            if (EllipticCurvePoint.IsInfinity(point))
            {
                return true;
            }

            BigInteger y = point.Y;
            BigInteger x = point.X;

            BigInteger lhs = y.SquareGF(P).AddGF(x.MultGF(y, P));
            BigInteger rhs = x.SquareGF(P).MultGF(x, P).AddGF(x.SquareGF(P).MultGF(A, P)).AddGF(B);

            return lhs == rhs;
        }
    }
}
