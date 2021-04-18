using CryptoEllipticCurve.Math;
using CryptoHash.Kupyna;
using CryptoHash.SHA256C;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CryptoEllipticCurve
{
    class Program
    {
        static void Main(string[] args)
        {
            BigInteger p = BigInteger.Parse("300613450595050653169853516389035139504087366260264943450533244356122755214669880763353471793250393988089774081");
            BigInteger a = 1;
            BigInteger b = BigInteger.Parse("43FC8AD242B0B7A6F3D1627AD5654447556B47BF6AA4A64B0C2AFE42CADAB8F93D92394C79A79755437B56995136", NumberStyles.HexNumber);
            BigInteger n = BigInteger.Parse("40000000000000000000000000000000000000000000009C300B75A3FA824F22428FD28CE8812245EF44049B2D49", NumberStyles.HexNumber);

            EllipticCurve curve = new EllipticCurve(a, b, p, n, 367);

            EllipticCurveContext context = new EllipticCurveContext(curve);

            Console.WriteLine($"Generator point:\nX = {curve.BasePoint.X}\nY = {curve.BasePoint.Y}");
            var (privateKey, publicKey) = context.GenenrateKeyPair();
            Console.WriteLine($"Private Key: d = {privateKey}");
            Console.WriteLine($"Public Key:\nX = {publicKey.X}\nY = {publicKey.Y}");
            Console.WriteLine($"Is valid public key: {context.IsValidPublicKey(publicKey)}");
            Console.WriteLine($"Is valid private key: {context.IsValidPrivateKey(privateKey, publicKey)}");

            using var hashFuction = new KupynaHash(256);
            var msg = "Transfer 1 dollar";
            var fake = "Transfer 1000 dollars";
            var msgBytes = Encoding.ASCII.GetBytes(msg);
            var fakeBytes = Encoding.ASCII.GetBytes(fake);
            Console.WriteLine($"Message to sign: {msg}");
            var (r, s) = context.SignMessage(msgBytes, privateKey, publicKey, hashFuction);

            Console.WriteLine($"R = {r}");
            Console.WriteLine($"S = {s}");
            hashFuction.Initialize();
            var result = context.VerifySign(msgBytes, publicKey, r, s, hashFuction);
            Console.WriteLine($"Verify <{msg}>: {result}");
            hashFuction.Initialize();
            result = context.VerifySign(fakeBytes, publicKey, r, s, hashFuction);
            Console.WriteLine($"Verify <{fake}>: {result}");
        }
    }
}
