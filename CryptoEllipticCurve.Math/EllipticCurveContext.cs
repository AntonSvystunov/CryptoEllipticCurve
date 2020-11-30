using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace CryptoEllipticCurve.Math
{
    public class EllipticCurveContext
    {
        private readonly EllipticCurve curve;
        private readonly RandomBigIntegerGenerator randomNumberGenerator = new RandomBigIntegerGenerator();

        public EllipticCurveContext(EllipticCurve curve, bool initIfNonInitialized = true)
        {
            if (curve == null)
            {
                throw new ArgumentNullException(nameof(curve));
            }

            this.curve = curve;
            if (!Initialized && initIfNonInitialized)
            {
                InitializeBasePoint();
            }
        }

        public bool Initialized => !EllipticCurvePoint.IsInfinity(curve.BasePoint);

        public EllipticCurvePoint GenerateBasePoint()
        {
            EllipticCurvePoint result;
            do
            {
                result = GeneratePoint();
            }
            while (!EllipticCurvePoint.IsInfinity(ScalarMult(curve.N, result)));

            CheckOnCurve(result);

            return result;
        }

        public EllipticCurvePoint GeneratePoint()
        {
            EllipticCurvePoint result = EllipticCurvePoint.InfinityPoint;
            BigInteger u, z;
            int k;

            do
            {
                u = GetRandomIntegerFromField();
                BigInteger w = u.SquareGF(curve.P).MultGF(u, curve.P).AddGF(u.SquareGF(curve.P).MultGF(curve.A, curve.P)).AddGF(curve.B);
                k = SolveQuadraticEquation(u, w, out z);
            }
            while (k == 0);

            result = new EllipticCurvePoint(u, z);
            CheckOnCurve(result);
            return result;
        }       

        public int SolveQuadraticEquation(BigInteger u, BigInteger w, out BigInteger z1)
        {
            // z^2 + u*z = w
            if (u == 0)
            {
                z1 = w.SqrtGF();
                return 1;
            }

            if (w == 0)
            {
                z1 = 0;
                return 2;
            }

            BigInteger u2 = u.InverseGF(curve.P).SquareGF(curve.P);

            BigInteger v = w.MultGF(u2, curve.P);

            if (v.Trace(curve.P) == 1)
            {
                z1 = 0;
                return 0;
            }

            BigInteger t = v.HalfTrace(curve.P);
            z1 = t.MultGF(u, curve.P);
            return 2;
        }

        public BigInteger GetRandomIntegerFromField()
        {
            return randomNumberGenerator.GetBigInteger(curve.P);
        }

        public void InitializeBasePoint()
        {
            var basePoint = GenerateBasePoint();
            curve.BasePoint = basePoint;
        }

        public (BigInteger, EllipticCurvePoint) GenenrateKeyPair()
        {
            if (!Initialized) throw new Exception("Elliptic curve wasn't initialized");
            BigInteger d = GetRandomIntegerFromField();

            EllipticCurvePoint p = NegativePoint(ScalarMult(d, curve.BasePoint));
            return (d, p);
        }

        public EllipticCurvePoint Add(EllipticCurvePoint a, EllipticCurvePoint b)
        {
            if (EllipticCurvePoint.IsInfinity(a))
            {
                CheckOnCurve(b);
                return b;
            }

            if (EllipticCurvePoint.IsInfinity(b))
            {
                CheckOnCurve(a);
                return a;
            }

            CheckOnCurve(a);
            CheckOnCurve(b);

            BigInteger x, y;
            if (a.X == b.X)
            {
                if (a.Y != b.Y || a.X == BigInteger.Zero)
                {
                    return EllipticCurvePoint.InfinityPoint;
                }

                return Twice(a);
            } 
            else
            {
                BigInteger lambda = a.X.AddGF(b.X).InverseGF(curve.P).MultGF(a.Y.AddGF(b.Y), curve.P);
                x = lambda.SquareGF(curve.P).AddGF(lambda).AddGF(a.X).AddGF(b.X).AddGF(curve.A);
                y = a.X.AddGF(x).MultGF(lambda, curve.P).AddGF(x).AddGF(a.Y);
            }

            var result = new EllipticCurvePoint(x, y);
            CheckOnCurve(result);
            return result;
        }

        public EllipticCurvePoint Twice(EllipticCurvePoint a)
        {
            if (EllipticCurvePoint.IsInfinity(a))
            {
                return a;
            }

            BigInteger sigma = a.X.InverseGF(curve.P).MultGF(a.Y, curve.P).AddGF(a.X);
            BigInteger x = sigma.SquareGF(curve.P).AddGF(sigma).AddGF(curve.A);
            BigInteger y = a.X.SquareGF(curve.P).AddGF(sigma.AddGF(BigInteger.One).MultGF(x, curve.P));

            var result = new EllipticCurvePoint(x, y);
            //CheckOnCurve(result);
            return result;
        }

        public EllipticCurvePoint ScalarMult(BigInteger k, EllipticCurvePoint a)
        {
            CheckOnCurve(a);

            if (k.ModulusGF(curve.N) == 0 || EllipticCurvePoint.IsInfinity(a))
            {
                return EllipticCurvePoint.InfinityPoint;
            }

            if (k < 0)
            {
                return ScalarMult(-k, NegativePoint(a));
            }

            EllipticCurvePoint res = EllipticCurvePoint.InfinityPoint;
            EllipticCurvePoint temp = a;

            while (k != 0)
            {
                if ((k & 1) == 1)
                {
                    res = Add(res, temp);
                }

                temp = Twice(temp);

                k >>= 1;
            }

            return res;
        }

        public EllipticCurvePoint NegativePoint(EllipticCurvePoint a)
        {
            CheckOnCurve(a);

            if (EllipticCurvePoint.IsInfinity(a))
            {
                return a;
            }

            var result = new EllipticCurvePoint(a.X, a.X.AddGF(a.Y));

            CheckOnCurve(a);
            return result;
        }

        public bool IsValidPublicKey(EllipticCurvePoint publicKey)
        {
            if (EllipticCurvePoint.IsInfinity(publicKey))
            {
                return false;
            }

            if (publicKey.X >= curve.P || publicKey.Y >= curve.P || publicKey.X * publicKey.Y == 0)
            {
                return false;
            }

            if (!curve.IsOnCurve(publicKey))
            {
                return false;
            }

            return EllipticCurvePoint.IsInfinity(ScalarMult(curve.N, publicKey));
        }

        public bool IsValidPrivateKey(BigInteger privateKey, EllipticCurvePoint publicKey)
        {
            EllipticCurvePoint p = NegativePoint(ScalarMult(privateKey, curve.BasePoint));

            if (EllipticCurvePoint.IsInfinity(p))
            {
                return false;
            }

            return p.X == publicKey.X && p.Y == publicKey.Y;
        }

        private void CheckOnCurve(EllipticCurvePoint a)
        {
            if (!curve.IsOnCurve(a))
            {
                throw new ArgumentException("Point not on curve");
            }
        }
    }
}
