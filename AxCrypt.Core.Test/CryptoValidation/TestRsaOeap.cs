﻿using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Test.CryptoValidation
{
    /// <summary>
    /// ====================
    /// pkcs-1v2-1-vec.zip
    /// ====================
    ///
    /// This directory contains test vectors for RSAES-OAEP and
    /// RSASSA-PSS as defined in PKCS #1 v2.1.
    ///
    /// The files:
    ///
    /// readme.txt              This file.
    ///
    /// oaep-vect.txt           Test vectors for RSAES-OAEP encryption.
    ///
    /// oaep-int.txt            Intermediate values for RSAES-OAEP
    ///                         encryption and RSA decryption with CRT.
    ///                         Also, DER-encoded RSAPrivateKey and
    ///                         RSAPublicKey types.
    ///
    /// pss-vect.txt            Test vectors for RSASSA-PSS signing.
    ///
    /// pss-int.txt             Intermediate values for RSASSA-PSS
    ///                         signing.
    ///
    /// =========================
    /// TEST VECTORS FOR RSA-OAEP
    /// =========================
    ///
    /// # This file contains test vectors for the
    /// # RSAES-OAEP encryption scheme as defined in
    /// # PKCS #1 v2.1. 10 RSA keys of different sizes
    /// # have been generated. For each key, 6 random
    /// # messages of length between 1 and 64 octets
    /// # have been RSAES-OAEP encrypted via a random
    /// # seed of length 20 octets.
    /// #
    /// # The underlying hash function is SHA-1; the
    /// # mask generation function is MGF1 with SHA-1
    /// # as specified in PKCS #1 v2.1.
    /// #
    /// # Integers are represented by strings of octets
    /// # with the leftmost octet being the most
    /// # significant octet. For example,
    /// #
    /// #           9,202,000 = (0x)8c 69 50.
    /// #
    /// # Key lengths:
    /// #
    /// # Key  1: 1024 bits
    /// # Key  2: 1025 bits
    /// # Key  3: 1026 bits
    /// # Key  4: 1027 bits
    /// # Key  5: 1028 bits
    /// # Key  6: 1029 bits
    /// # Key  7: 1030 bits
    /// # Key  8: 1031 bits
    /// # Key  9: 1536 bits
    /// # Key 10: 2048 bits
    /// # =============================================
    /// </summary>
    [TestFixture]
    public static class TestRsaOeap
    {
        internal class InjectedBytesRandomGenerator : IRandomGenerator
        {
            private byte[] _bytes;

            private int _offset = 0;

            public InjectedBytesRandomGenerator(byte[] bytes)
            {
                _bytes = bytes;
            }

            public byte[] Generate(int count)
            {
                byte[] bytes = new byte[count];
                for (int i = 0; i < bytes.Length; ++i)
                {
                    bytes[i] = _bytes[_offset++];
                    _offset %= _bytes.Length;
                }

                return bytes;
            }
        }

        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new FakeAsymmetricFactory("SHA1"));
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
        }

        private static byte[] PositiveFromHex(string hex)
        {
            hex = "00" + hex.Replace(" ", String.Empty).Replace("\r", String.Empty).Replace("\n", String.Empty);
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Odd number of characters is not allowed in a hex string.");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; ++i)
            {
                bytes[i] = Byte.Parse(hex.Substring(i + i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return bytes;
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example1_1()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample1(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            66 28 19 4e 12 07 3d b0 3b a9 4c da 9e f9 53 23
            97 d5 0d ba 79 b9 87 00 4a fe fe 34
            ";

            //# Seed:
            seed = @"
            18 b7 76 ea 21 06 9d 69 77 6a 33 e9 6b ad 48 e1
            dd a0 a5 ef
            ";

            //# Encryption:
            encryption = @"
            35 4f e6 7b 4a 12 6d 5d 35 fe 36 c7 77 79 1a 3f
            7b a1 3d ef 48 4e 2d 39 08 af f7 22 fa d4 68 fb
            21 69 6d e9 5d 0b e9 11 c2 d3 17 4f 8a fc c2 01
            03 5f 7b 6d 8e 69 40 2d e5 45 16 18 c2 1a 53 5f
            a9 d7 bf c5 b8 dd 9f c2 43 f8 cf 92 7d b3 13 22
            d6 e8 81 ea a9 1a 99 61 70 e6 57 a0 5a 26 64 26
            d9 8c 88 00 3f 84 77 c1 22 70 94 a0 d9 fa 1e 8c
            40 24 30 9c e1 ec cc b5 21 00 35 d4 7a c7 2e 8a
            ";

            RunOneCase("RSAES-OAEP Encryption Example 1.1", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example1_2()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample1(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            75 0c 40 47 f5 47 e8 e4 14 11 85 65 23 29 8a c9
            ba e2 45 ef af 13 97 fb e5 6f 9d d5
            ";

            //# Seed:
            seed = @"
            0c c7 42 ce 4a 9b 7f 32 f9 51 bc b2 51 ef d9 25
            fe 4f e3 5f
            ";

            //# Encryption:
            encryption = @"
            64 0d b1 ac c5 8e 05 68 fe 54 07 e5 f9 b7 01 df
            f8 c3 c9 1e 71 6c 53 6f c7 fc ec 6c b5 b7 1c 11
            65 98 8d 4a 27 9e 15 77 d7 30 fc 7a 29 93 2e 3f
            00 c8 15 15 23 6d 8d 8e 31 01 7a 7a 09 df 43 52
            d9 04 cd eb 79 aa 58 3a dc c3 1e a6 98 a4 c0 52
            83 da ba 90 89 be 54 91 f6 7c 1a 4e e4 8d c7 4b
            bb e6 64 3a ef 84 66 79 b4 cb 39 5a 35 2d 5e d1
            15 91 2d f6 96 ff e0 70 29 32 94 6d 71 49 2b 44
            ";

            RunOneCase("RSAES-OAEP Encryption Example 1.2", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example1_3()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample1(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            d9 4a e0 83 2e 64 45 ce 42 33 1c b0 6d 53 1a 82
            b1 db 4b aa d3 0f 74 6d c9 16 df 24 d4 e3 c2 45
            1f ff 59 a6 42 3e b0 e1 d0 2d 4f e6 46 cf 69 9d
            fd 81 8c 6e 97 b0 51
            ";

            //# Seed:
            seed = @"
            25 14 df 46 95 75 5a 67 b2 88 ea f4 90 5c 36 ee
            c6 6f d2 fd
            ";

            //# Encryption:
            encryption = @"
            42 37 36 ed 03 5f 60 26 af 27 6c 35 c0 b3 74 1b
            36 5e 5f 76 ca 09 1b 4e 8c 29 e2 f0 be fe e6 03
            59 5a a8 32 2d 60 2d 2e 62 5e 95 eb 81 b2 f1 c9
            72 4e 82 2e ca 76 db 86 18 cf 09 c5 34 35 03 a4
            36 08 35 b5 90 3b c6 37 e3 87 9f b0 5e 0e f3 26
            85 d5 ae c5 06 7c d7 cc 96 fe 4b 26 70 b6 ea c3
            06 6b 1f cf 56 86 b6 85 89 aa fb 7d 62 9b 02 d8
            f8 62 5c a3 83 36 24 d4 80 0f b0 81 b1 cf 94 eb
            ";

            RunOneCase("RSAES-OAEP Encryption Example 1.3", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example1_4()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample1(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            52 e6 50 d9 8e 7f 2a 04 8b 4f 86 85 21 53 b9 7e
            01 dd 31 6f 34 6a 19 f6 7a 85
            ";

            //# Seed:
            seed = @"
            c4 43 5a 3e 1a 18 a6 8b 68 20 43 62 90 a3 7c ef
            b8 5d b3 fb
            ";

            //# Encryption:
            encryption = @"
            45 ea d4 ca 55 1e 66 2c 98 00 f1 ac a8 28 3b 05
            25 e6 ab ae 30 be 4b 4a ba 76 2f a4 0f d3 d3 8e
            22 ab ef c6 97 94 f6 eb bb c0 5d db b1 12 16 24
            7d 2f 41 2f d0 fb a8 7c 6e 3a cd 88 88 13 64 6f
            d0 e4 8e 78 52 04 f9 c3 f7 3d 6d 82 39 56 27 22
            dd dd 87 71 fe c4 8b 83 a3 1e e6 f5 92 c4 cf d4
            bc 88 17 4f 3b 13 a1 12 aa e3 b9 f7 b8 0e 0f c6
            f7 25 5b a8 80 dc 7d 80 21 e2 2a d6 a8 5f 07 55
            ";

            RunOneCase("RSAES-OAEP Encryption Example 1.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example1_5()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample1(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            8d a8 9f d9 e5 f9 74 a2 9f ef fb 46 2b 49 18 0f
            6c f9 e8 02
            ";

            //# Seed:
            seed = @"
            b3 18 c4 2d f3 be 0f 83 fe a8 23 f5 a7 b4 7e d5
            e4 25 a3 b5
            ";

            //# Encryption:
            encryption = @"
            36 f6 e3 4d 94 a8 d3 4d aa cb a3 3a 21 39 d0 0a
            d8 5a 93 45 a8 60 51 e7 30 71 62 00 56 b9 20 e2
            19 00 58 55 a2 13 a0 f2 38 97 cd cd 73 1b 45 25
            7c 77 7f e9 08 20 2b ef dd 0b 58 38 6b 12 44 ea
            0c f5 39 a0 5d 5d 10 32 9d a4 4e 13 03 0f d7 60
            dc d6 44 cf ef 20 94 d1 91 0d 3f 43 3e 1c 7c 6d
            d1 8b c1 f2 df 7f 64 3d 66 2f b9 dd 37 ea d9 05
            91 90 f4 fa 66 ca 39 e8 69 c4 eb 44 9c bd c4 39
            ";

            RunOneCase("RSAES-OAEP Encryption Example 1.5", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example1_6()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample1(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            26 52 10 50 84 42 71
            ";

            //# Seed:
            seed = @"
            e4 ec 09 82 c2 33 6f 3a 67 7f 6a 35 61 74 eb 0c
            e8 87 ab c2
            ";

            //# Encryption:
            encryption = @"
            42 ce e2 61 7b 1e ce a4 db 3f 48 29 38 6f bd 61
            da fb f0 38 e1 80 d8 37 c9 63 66 df 24 c0 97 b4
            ab 0f ac 6b df 59 0d 82 1c 9f 10 64 2e 68 1a d0
            5b 8d 78 b3 78 c0 f4 6c e2 fa d6 3f 74 e0 ad 3d
            f0 6b 07 5d 7e b5 f5 63 6f 8d 40 3b 90 59 ca 76
            1b 5c 62 bb 52 aa 45 00 2e a7 0b aa ce 08 de d2
            43 b9 d8 cb d6 2a 68 ad e2 65 83 2b 56 56 4e 43
            a6 fa 42 ed 19 9a 09 97 69 74 2d f1 53 9e 82 55
            ";

            RunOneCase("RSAES-OAEP Encryption Example 1.6", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example2_1()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample2(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            8f f0 0c aa 60 5c 70 28 30 63 4d 9a 6c 3d 42 c6
            52 b5 8c f1 d9 2f ec 57 0b ee e7
            ";

            //# Seed:
            seed = @"
            8c 40 7b 5e c2 89 9e 50 99 c5 3e 8c e7 93 bf 94
            e7 1b 17 82
            ";

            //# Encryption:
            encryption = @"
            01 81 af 89 22 b9 fc b4 d7 9d 92 eb e1 98 15 99
            2f c0 c1 43 9d 8b cd 49 13 98 a0 f4 ad 3a 32 9a
            5b d9 38 55 60 db 53 26 83 c8 b7 da 04 e4 b1 2a
            ed 6a ac df 47 1c 34 c9 cd a8 91 ad dc c2 df 34
            56 65 3a a6 38 2e 9a e5 9b 54 45 52 57 eb 09 9d
            56 2b be 10 45 3f 2b 6d 13 c5 9c 02 e1 0f 1f 8a
            bb 5d a0 d0 57 09 32 da cf 2d 09 01 db 72 9d 0f
            ef cc 05 4e 70 96 8e a5 40 c8 1b 04 bc ae fe 72
            0e
            ";

            RunOneCase("RSAES-OAEP Encryption Example 2.1", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example2_2()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample2(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            2d
            ";

            //# Seed:
            seed = @"
            b6 00 cf 3c 2e 50 6d 7f 16 77 8c 91 0d 3a 8b 00
            3e ee 61 d5
            ";

            //# Encryption:
            encryption = @"
            01 87 59 ff 1d f6 3b 27 92 41 05 62 31 44 16 a8
            ae af 2a c6 34 b4 6f 94 0a b8 2d 64 db f1 65 ee
            e3 30 11 da 74 9d 4b ab 6e 2f cd 18 12 9c 9e 49
            27 7d 84 53 11 2b 42 9a 22 2a 84 71 b0 70 99 39
            98 e7 58 86 1c 4d 3f 6d 74 9d 91 c4 29 0d 33 2c
            7a 4a b3 f7 ea 35 ff 3a 07 d4 97 c9 55 ff 0f fc
            95 00 6b 62 c6 d2 96 81 0d 9b fa b0 24 19 6c 79
            34 01 2c 2d f9 78 ef 29 9a ba 23 99 40 cb a1 02
            45
            ";

            RunOneCase("RSAES-OAEP Encryption Example 2.2", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example2_3()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample2(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            74 fc 88 c5 1b c9 0f 77 af 9d 5e 9a 4a 70 13 3d
            4b 4e 0b 34 da 3c 37 c7 ef 8e
            ";

            //# Seed:
            seed = @"
            a7 37 68 ae ea a9 1f 9d 8c 1e d6 f9 d2 b6 34 67
            f0 7c ca e3
            ";

            //# Encryption:
            encryption = @"
            01 88 02 ba b0 4c 60 32 5e 81 c4 96 23 11 f2 be
            7c 2a dc e9 30 41 a0 07 19 c8 8f 95 75 75 f2 c7
            9f 1b 7b c8 ce d1 15 c7 06 b3 11 c0 8a 2d 98 6c
            a3 b6 a9 33 6b 14 7c 29 c6 f2 29 40 9d de c6 51
            bd 1f dd 5a 0b 7f 61 0c 99 37 fd b4 a3 a7 62 36
            4b 8b 32 06 b4 ea 48 5f d0 98 d0 8f 63 d4 aa 8b
            b2 69 7d 02 7b 75 0c 32 d7 f7 4e af 51 80 d2 e9
            b6 6b 17 cb 2f a5 55 23 bc 28 0d a1 0d 14 be 20
            53
            ";

            RunOneCase("RSAES-OAEP Encryption Example 2.3", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example2_4()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample2(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            a7 eb 2a 50 36 93 1d 27 d4 e8 91 32 6d 99 69 2f
            fa dd a9 bf 7e fd 3e 34 e6 22 c4 ad c0 85 f7 21
            df e8 85 07 2c 78 a2 03 b1 51 73 9b e5 40 fa 8c
            15 3a 10 f0 0a
            ";

            //# Seed:
            seed = @"
            9a 7b 3b 0e 70 8b d9 6f 81 90 ec ab 4f b9 b2 b3
            80 5a 81 56
            ";

            //# Encryption:
            encryption = @"
            00 a4 57 8c bc 17 63 18 a6 38 fb a7 d0 1d f1 57
            46 af 44 d4 f6 cd 96 d7 e7 c4 95 cb f4 25 b0 9c
            64 9d 32 bf 88 6d a4 8f ba f9 89 a2 11 71 87 ca
            fb 1f b5 80 31 76 90 e3 cc d4 46 92 0b 7a f8 2b
            31 db 58 04 d8 7d 01 51 4a cb fa 91 56 e7 82 f8
            67 f6 be d9 44 9e 0e 9a 2c 09 bc ec c6 aa 08 76
            36 96 5e 34 b3 ec 76 6f 2f e2 e4 30 18 a2 fd de
            b1 40 61 6a 0e 9d 82 e5 33 10 24 ee 06 52 fc 76
            41
            ";

            RunOneCase("RSAES-OAEP Encryption Example 2.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example2_5()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample2(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            2e f2 b0 66 f8 54 c3 3f 3b dc bb 59 94 a4 35 e7
            3d 6c 6c
            ";

            //# Seed:
            seed = @"
            eb 3c eb bc 4a dc 16 bb 48 e8 8c 8a ec 0e 34 af
            7f 42 7f d3
            ";

            //# Encryption:
            encryption = @"
            00 eb c5 f5 fd a7 7c fd ad 3c 83 64 1a 90 25 e7
            7d 72 d8 a6 fb 33 a8 10 f5 95 0f 8d 74 c7 3e 8d
            93 1e 86 34 d8 6a b1 24 62 56 ae 07 b6 00 5b 71
            b7 f2 fb 98 35 12 18 33 1c e6 9b 8f fb dc 9d a0
            8b bc 9c 70 4f 87 6d eb 9d f9 fc 2e c0 65 ca d8
            7f 90 90 b0 7a cc 17 aa 7f 99 7b 27 ac a4 88 06
            e8 97 f7 71 d9 51 41 fe 45 26 d8 a5 30 1b 67 86
            27 ef ab 70 7f d4 0f be bd 6e 79 2a 25 61 3e 7a
            ec
            ";

            RunOneCase("RSAES-OAEP Encryption Example 2.5", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example2_6()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample2(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
            8a 7f b3 44 c8 b6 cb 2c f2 ef 1f 64 3f 9a 32 18
            f6 e1 9b ba 89 c0
            ";

            //# Seed:
            seed = @"
            4c 45 cf 4d 57 c9 8e 3d 6d 20 95 ad c5 1c 48 9e
            b5 0d ff 84
            ";

            //# Encryption:
            encryption = @"
            01 08 39 ec 20 c2 7b 90 52 e5 5b ef b9 b7 7e 6f
            c2 6e 90 75 d7 a5 43 78 c6 46 ab df 51 e4 45 bd
            57 15 de 81 78 9f 56 f1 80 3d 91 70 76 4a 9e 93
            cb 78 79 86 94 02 3e e7 39 3c e0 4b c5 d8 f8 c5
            a5 2c 17 1d 43 83 7e 3a ca 62 f6 09 eb 0a a5 ff
            b0 96 0e f0 41 98 dd 75 4f 57 f7 fb e6 ab f7 65
            cf 11 8b 4c a4 43 b2 3b 5a ab 26 6f 95 23 26 ac
            45 81 10 06 44 32 5f 8b 72 1a cd 5d 04 ff 14 ef
            3a
            ";

            RunOneCase("RSAES-OAEP Encryption Example 2.6", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example3_1()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample3(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
08 78 20 b5 69 e8 fa 8d
            ";

            //# Seed:
            seed = @"
8c ed 6b 19 62 90 80 57 90 e9 09 07 40 15 e6 a2
0b 0c 48 94
            ";

            //# Encryption:
            encryption = @"
02 6a 04 85 d9 6a eb d9 6b 43 82 08 50 99 b9 62
e6 a2 bd ec 3d 90 c8 db 62 5e 14 37 2d e8 5e 2d
5b 7b aa b6 5c 8f af 91 bb 55 04 fb 49 5a fc e5
c9 88 b3 f6 a5 2e 20 e1 d6 cb d3 56 6c 5c d1 f2
b8 31 8b b5 42 cc 0e a2 5c 4a ab 99 32 af a2 07
60 ea dd ec 78 43 96 a0 7e a0 ef 24 d4 e6 f4 d3
7e 50 52 a7 a3 1e 14 6a a4 80 a1 11 bb e9 26 40
13 07 e0 0f 41 00 33 84 2b 6d 82 fe 5c e4 df ae
80
            ";

            RunOneCase("RSAES-OAEP Encryption Example 3.1", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example3_2()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample3(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
46 53 ac af 17 19 60 b0 1f 52 a7 be 63 a3 ab 21
dc 36 8e c4 3b 50 d8 2e c3 78 1e 04
            ";

            //# Seed:
            seed = @"
b4 29 1d 65 67 55 08 48 cc 15 69 67 c8 09 ba ab
6c a5 07 f0
            ";

            //# Encryption:
            encryption = @"
02 4d b8 9c 78 02 98 9b e0 78 38 47 86 30 84 94
1b f2 09 d7 61 98 7e 38 f9 7c b5 f6 f1 bc 88 da
72 a5 0b 73 eb af 11 c8 79 c4 f9 5d f3 7b 85 0b
8f 65 d7 62 2e 25 b1 b8 89 e8 0f e8 0b ac a2 06
9d 6e 0e 1d 82 99 53 fc 45 90 69 de 98 ea 97 98
b4 51 e5 57 e9 9a bf 8f e3 d9 cc f9 09 6e bb f3
e5 25 5d 3b 4e 1c 6d 2e ca df 06 7a 35 9e ea 86
40 5a cd 47 d5 e1 65 51 7c ca fd 47 d6 db ee 4b
f5
            ";

            RunOneCase("RSAES-OAEP Encryption Example 3.2", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example3_3()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample3(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
d9 4c d0 e0 8f a4 04 ed 89
            ";

            //# Seed:
            seed = @"
ce 89 28 f6 05 95 58 25 40 08 ba dd 97 94 fa dc
d2 fd 1f 65
            ";

            //# Encryption:
            encryption = @"
02 39 bc e6 81 03 24 41 52 88 77 d6 d1 c8 bb 28
aa 3b c9 7f 1d f5 84 56 36 18 99 57 97 68 38 44
ca 86 66 47 32 f4 be d7 a0 aa b0 83 aa ab fb 72
38 f5 82 e3 09 58 c2 02 4e 44 e5 70 43 b9 79 50
fd 54 3d a9 77 c9 0c dd e5 33 7d 61 84 42 f9 9e
60 d7 78 3a b5 9c e6 dd 9d 69 c4 7a d1 e9 62 be
c2 2d 05 89 5c ff 8d 3f 64 ed 52 61 d9 2b 26 78
51 03 93 48 49 90 ba 3f 7f 06 81 8a e6 ff ce 8a
3a
            ";

            RunOneCase("RSAES-OAEP Encryption Example 3.3", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example3_4()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample3(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
6c c6 41 b6 b6 1e 6f 96 39 74 da d2 3a 90 13 28
4e f1
            ";

            //# Seed:
            seed = @"
6e 29 79 f5 2d 68 14 a5 7d 83 b0 90 05 48 88 f1
19 a5 b9 a3
            ";

            //# Encryption:
            encryption = @"
02 99 4c 62 af d7 6f 49 8b a1 fd 2c f6 42 85 7f
ca 81 f4 37 3c b0 8f 1c ba ee 6f 02 5c 3b 51 2b
42 c3 e8 77 91 13 47 66 48 03 9d be 04 93 f9 24
62 92 fa c2 89 50 60 0e 7c 0f 32 ed f9 c8 1b 9d
ec 45 c3 bd e0 cc 8d 88 47 59 01 69 90 7b 7d c5
99 1c eb 29 bb 07 14 d6 13 d9 6d f0 f1 2e c5 d8
d3 50 7c 8e e7 ae 78 dd 83 f2 16 fa 61 de 10 03
63 ac a4 8a 7e 91 4a e9 f4 2d df be 94 3b 09 d9
a0
            ";

            RunOneCase("RSAES-OAEP Encryption Example 3.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example3_5()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample3(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
df 51 51 83 2b 61 f4 f2 58 91 fb 41 72 f3 28 d2
ed df 83 71 ff cf db e9 97 93 92 95 f3 0e ca 69
18 01 7c fd a1 15 3b f7 a6 af 87 59 32 23
            ";

            //# Seed:
            seed = @"
2d 76 0b fe 38 c5 9d e3 4c dc 8b 8c 78 a3 8e 66
28 4a 2d 27
            ";

            //# Encryption:
            encryption = @"
01 62 04 2f f6 96 95 92 a6 16 70 31 81 1a 23 98
34 ce 63 8a bf 54 fe c8 b9 94 78 12 2a fe 2e e6
7f 8c 5b 18 b0 33 98 05 bf db c5 a4 e6 72 0b 37
c5 9c fb a9 42 46 4c 59 7f f5 32 a1 19 82 15 45
fd 2e 59 b1 14 e6 1d af 71 82 05 29 f5 02 9c f5
24 95 43 27 c3 4e c5 e6 f5 ba 7e fc c4 de 94 3a
b8 ad 4e d7 87 b1 45 43 29 f7 0d b7 98 a3 a8 f4
d9 2f 82 74 e2 b2 94 8a de 62 7c e8 ee 33 e4 3c
60
            ";

            RunOneCase("RSAES-OAEP Encryption Example 3.5", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example3_6()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample3(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
3c 3b ad 89 3c 54 4a 6d 52 0a b0 22 31 91 88 c8
d5 04 b7 a7 88 b8 50 90 3b 85 97 2e aa 18 55 2e
11 34 a7 ad 60 98 82 62 54 ff 7a b6 72 b3 d8 eb
31 58 fa c6 d4 cb ae f1
            ";

            //# Seed:
            seed = @"
f1 74 77 9c 5f d3 cf e0 07 ba dc b7 a3 6c 9b 55
bf cf bf 0e
            ";

            //# Encryption:
            encryption = @"
00 11 20 51 e7 5d 06 49 43 bc 44 78 07 5e 43 48
2f d5 9c ee 06 79 de 68 93 ee c3 a9 43 da a4 90
b9 69 1c 93 df c0 46 4b 66 23 b9 f3 db d3 e7 00
83 26 4f 03 4b 37 4f 74 16 4e 1a 00 76 37 25 e5
74 74 4b a0 b9 db 83 43 4f 31 df 96 f6 e2 a2 6f
6d 8e ba 34 8b d4 68 6c 22 38 ac 07 c3 7a ac 37
85 d1 c7 ee a2 f8 19 fd 91 49 17 98 ed 8e 9c ef
5e 43 b7 81 b0 e0 27 6e 37 c4 3f f9 49 2d 00 57
30
            ";

            RunOneCase("RSAES-OAEP Encryption Example 3.6", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example4_1()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample4(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
4a 86 60 95 34 ee 43 4a 6c bc a3 f7 e9 62 e7 6d
45 5e 32 64 c1 9f 60 5f 6e 5f f6 13 7c 65 c5 6d
7f b3 44 cd 52 bc 93 37 4f 3d 16 6c 9f 0c 6f 9c
50 6b ad 19 33 09 72 d2
            ";

            //# Seed:
            seed = @"
1c ac 19 ce 99 3d ef 55 f9 82 03 f6 85 28 96 c9
5c cc a1 f3
            ";

            //# Encryption:
            encryption = @"
04 cc e1 96 14 84 5e 09 41 52 a3 fe 18 e5 4e 33
30 c4 4e 5e fb c6 4a e1 68 86 cb 18 69 01 4c c5
78 1b 1f 8f 9e 04 53 84 d0 11 2a 13 5c a0 d1 2e
9c 88 a8 e4 06 34 16 de aa e3 84 4f 60 d6 e9 6f
e1 55 14 5f 45 25 b9 a3 44 31 ca 37 66 18 0f 70
e1 5a 5e 5d 8e 8b 1a 51 6f f8 70 60 9f 13 f8 96
93 5c ed 18 82 79 a5 8e d1 3d 07 11 42 77 d7 5c
65 68 60 7e 0a b0 92 fd 80 3a 22 3e 4a 8e e0 b1
a8
            ";

            RunOneCase("RSAES-OAEP Encryption Example 4.1", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example4_2()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample4(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
b0 ad c4 f3 fe 11 da 59 ce 99 27 73 d9 05 99 43
c0 30 46 49 7e e9 d9 f9 a0 6d f1 16 6d b4 6d 98
f5 8d 27 ec 07 4c 02 ee e6 cb e2 44 9c 8b 9f c5
08 0c 5c 3f 44 33 09 25 12 ec 46 aa 79 37 43 c8
            ";

            //# Seed:
            seed = @"
f5 45 d5 89 75 85 e3 db 71 aa 0c b8 da 76 c5 1d
03 2a e9 63
            ";

            //# Encryption:
            encryption = @"
00 97 b6 98 c6 16 56 45 b3 03 48 6f bf 5a 2a 44
79 c0 ee 85 88 9b 54 1a 6f 0b 85 8d 6b 65 97 b1
3b 85 4e b4 f8 39 af 03 39 9a 80 d7 9b da 65 78
c8 41 f9 0d 64 57 15 b2 80 d3 71 43 99 2d d1 86
c8 0b 94 9b 77 5c ae 97 37 0e 4e c9 74 43 13 6c
6d a4 84 e9 70 ff db 13 23 a2 08 47 82 1d 3b 18
38 1d e1 3b b4 9a ae a6 65 30 c4 a4 b8 27 1f 3e
ae 17 2c d3 66 e0 7e 66 36 f1 01 9d 2a 28 ae d1
5e
            ";

            RunOneCase("RSAES-OAEP Encryption Example 4.2", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example4_3()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample4(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
bf 6d 42 e7 01 70 7b 1d 02 06 b0 c8 b4 5a 1c 72
64 1f f1 28 89 21 9a 82 bd ea 96 5b 5e 79 a9 6b
0d 01 63 ed 9d 57 8e c9 ad a2 0f 2f bc f1 ea 3c
40 89 d8 34 19 ba 81 b0 c6 0f 36 06 da 99
            ";

            //# Seed:
            seed = @"
ad 99 7f ee f7 30 d6 ea 7b e6 0d 0d c5 2e 72 ea
cb fd d2 75
            ";

            //# Encryption:
            encryption = @"
03 01 f9 35 e9 c4 7a bc b4 8a cb be 09 89 5d 9f
59 71 af 14 83 9d a4 ff 95 41 7e e4 53 d1 fd 77
31 90 72 bb 72 97 e1 b5 5d 75 61 cd 9d 1b b2 4c
1a 9a 37 c6 19 86 43 08 24 28 04 87 9d 86 eb d0
01 dc e5 18 39 75 e1 50 69 89 b7 0e 5a 83 43 41
54 d5 cb fd 6a 24 78 7e 60 eb 0c 65 8d 2a c1 93
30 2d 11 92 c6 e6 22 d4 a1 2a d4 b5 39 23 bc a2
46 df 31 c6 39 5e 37 70 2c 6a 78 ae 08 1f b9 d0
65
            ";

            RunOneCase("RSAES-OAEP Encryption Example 4.3", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example4_4()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample4(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
fb 2e f1 12 f5 e7 66 eb 94 01 92 97 93 47 94 f7
be 2f 6f c1 c5 8e
            ";

            //# Seed:
            seed = @"
13 64 54 df 57 30 f7 3c 80 7a 7e 40 d8 c1 a3 12
ac 5b 9d d3
            ";

            //# Encryption:
            encryption = @"
02 d1 10 ad 30 af b7 27 be b6 91 dd 0c f1 7d 0a
f1 a1 e7 fa 0c c0 40 ec 1a 4b a2 6a 42 c5 9d 0a
79 6a 2e 22 c8 f3 57 cc c9 8b 65 19 ac eb 68 2e
94 5e 62 cb 73 46 14 a5 29 40 7c d4 52 be e3 e4
4f ec e8 42 3c c1 9e 55 54 8b 8b 99 4b 84 9c 7e
cd e4 93 3e 76 03 7e 1d 0c e4 42 75 b0 87 10 c6
8e 43 01 30 b9 29 73 0e d7 7e 09 b0 15 64 2c 55
93 f0 4e 4f fb 94 10 79 81 02 a8 e9 6f fd fe 11
e4
            ";

            RunOneCase("RSAES-OAEP Encryption Example 4.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example4_5()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample4(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
28 cc d4 47 bb 9e 85 16 6d ab b9 e5 b7 d1 ad ad
c4 b9 d3 9f 20 4e 96 d5 e4 40 ce 9a d9 28 bc 1c
22 84
            ";

            //# Seed:
            seed = @"
bc a8 05 7f 82 4b 2e a2 57 f2 86 14 07 ee f6 3d
33 20 86 81
            ";

            //# Encryption:
            encryption = @"
00 db b8 a7 43 9d 90 ef d9 19 a3 77 c5 4f ae 8f
e1 1e c5 8c 3b 85 83 62 e2 3a d1 b8 a4 43 10 79
90 66 b9 93 47 aa 52 56 91 d2 ad c5 8d 9b 06 e3
4f 28 8c 17 03 90 c5 f0 e1 1c 0a a3 64 59 59 f1
8e e7 9e 8f 2b e8 d7 ac 5c 23 d0 61 f1 8d d7 4b
8c 5f 2a 58 fc b5 eb 0c 54 f9 9f 01 a8 32 47 56
82 92 53 65 83 34 09 48 d7 a8 c9 7c 4a cd 1e 98
d1 e2 9d c3 20 e9 7a 26 05 32 a8 aa 7a 75 8a 1e
c2
            ";

            RunOneCase("RSAES-OAEP Encryption Example 4.5", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example4_6()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample4(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
f2 22 42 75 1e c6 b1
            ";

            //# Seed:
            seed = @"
2e 7e 1e 17 f6 47 b5 dd d0 33 e1 54 72 f9 0f 68
12 f3 ac 4e
            ";

            //# Encryption:
            encryption = @"
00 a5 ff a4 76 8c 8b be ca ee 2d b7 7e 8f 2e ec
99 59 59 33 54 55 20 83 5e 5b a7 db 94 93 d3 e1
7c dd ef e6 a5 f5 67 62 44 71 90 8d b4 e2 d8 3a
0f be e6 06 08 fc 84 04 95 03 b2 23 4a 07 dc 83
b2 7b 22 84 7a d8 92 0f f4 2f 67 4e f7 9b 76 28
0b 00 23 3d 2b 51 b8 cb 27 03 a9 d4 2b fb c8 25
0c 96 ec 32 c0 51 e5 7f 1b 4b a5 28 db 89 c3 7e
4c 54 e2 7e 6e 64 ac 69 63 5a e8 87 d9 54 16 19
a9
            ";

            RunOneCase("RSAES-OAEP Encryption Example 4.6", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example5_1()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample5(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
af 71 a9 01 e3 a6 1d 31 32 f0 fc 1f db 47 4f 9e
a6 57 92 57 ff c2 4d 16 41 70 14 5b 3d bd e8
            ";

            //# Seed:
            seed = @"
44 c9 2e 28 3f 77 b9 49 9c 60 3d 96 36 60 c8 7d
2f 93 94 61
            ";

            //# Encryption:
            encryption = @"
03 60 46 a4 a4 7d 9e d3 ba 9a 89 13 9c 10 50 38
eb 74 92 b0 5a 5d 68 bf d5 3a cc ff 45 97 f7 a6
86 51 b4 7b 4a 46 27 d9 27 e4 85 ee d7 b4 56 64
20 e8 b4 09 87 9e 5d 60 6e ae 25 1d 22 a5 df 79
9f 79 20 bf c1 17 b9 92 57 2a 53 b1 26 31 46 bc
ea 03 38 5c c5 e8 53 c9 a1 01 c8 c3 e1 bd a3 1a
51 98 07 49 6c 6c b5 e5 ef b4 08 82 3a 35 2b 8f
a0 66 1f b6 64 ef ad d5 93 de b9 9f ff 5e d0 00
e5
            ";

            RunOneCase("RSAES-OAEP Encryption Example 5.1", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example5_2()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample5(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
a3 b8 44 a0 82 39 a8 ac 41 60 5a f1 7a 6c fd a4
d3 50 13 65 85 90 3a 41 7a 79 26 87 60 51 9a 4b
4a c3 30 3e c7 3f 0f 87 cf b3 23 99
            ";

            //# Seed:
            seed = @"
cb 28 f5 86 06 59 fc ee e4 9c 3e ea fc e6 25 a7
08 03 bd 32
            ";

            //# Encryption:
            encryption = @"
03 d6 eb 65 4e dc e6 15 bc 59 f4 55 26 5e d4 e5
a1 82 23 cb b9 be 4e 40 69 b4 73 80 4d 5d e9 6f
54 dc aa a6 03 d0 49 c5 d9 4a a1 47 0d fc d2 25
40 66 b7 c7 b6 1f f1 f6 f6 77 0e 32 15 c5 13 99
fd 4e 34 ec 50 82 bc 48 f0 89 84 0a d0 43 54 ae
66 dc 0f 1b d1 8e 46 1a 33 cc 12 58 b4 43 a2 83
7a 6d f2 67 59 aa 23 02 33 49 86 f8 73 80 c9 cc
9d 53 be 9f 99 60 5d 2c 9a 97 da 7b 09 15 a4 a7
ad
            ";

            RunOneCase("RSAES-OAEP Encryption Example 5.2", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example5_3()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample5(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
30 8b 0e cb d2 c7 6c b7 7f c6 f7 0c 5e dd 23 3f
d2 f2 09 29 d6 29 f0 26 95 3b b6 2a 8f 4a 3a 31
4b de 19 5d e8 5b 5f 81 6d a2 aa b0 74 d2 6c b6
ac dd f3 23 ae 3b 9c 67 8a c3 cf 12 fb dd e7
            ";

            //# Seed:
            seed = @"
22 85 f4 0d 77 04 82 f9 a9 ef a2 c7 2c b3 ac 55
71 6d c0 ca
            ";

            //# Encryption:
            encryption = @"
07 70 95 21 81 64 9f 9f 9f 07 ff 62 6f f3 a2 2c
35 c4 62 44 3d 90 5d 45 6a 9f d0 bf f4 3c ac 2c
a7 a9 f5 54 e9 47 8b 9a cc 3a c8 38 b0 20 40 ff
d3 e1 84 7d e2 e4 25 39 29 f9 dd 9e e4 04 43 25
a9 b0 5c ab b8 08 b2 ee 84 0d 34 e1 5d 10 5a 3f
1f 7b 27 69 5a 1a 07 a2 d7 3f e0 8e ca aa 3c 9c
9d 4d 5a 89 ff 89 0d 54 72 7d 7a e4 0c 0e c1 a8
dd 86 16 5d 8e e2 c6 36 81 41 01 6a 48 b5 5b 69
67
            ";

            RunOneCase("RSAES-OAEP Encryption Example 5.3", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example5_4()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample5(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
15 c5 b9 ee 11 85
            ";

            //# Seed:
            seed = @"
49 fa 45 d3 a7 8d d1 0d fd 57 73 99 d1 eb 00 af
7e ed 55 13
            ";

            //# Encryption:
            encryption = @"
08 12 b7 67 68 eb cb 64 2d 04 02 58 e5 f4 44 1a
01 85 21 bd 96 68 7e 6c 5e 89 9f cd 6c 17 58 8f
f5 9a 82 cc 8a e0 3a 4b 45 b3 12 99 af 17 88 c3
29 f7 dc d2 85 f8 cf 4c ed 82 60 6b 97 61 26 71
a4 5b ed ca 13 34 42 14 4d 16 17 d1 14 f8 02 85
7f 0f 9d 73 97 51 c5 7a 3f 9e e4 00 91 2c 61 e2
e6 99 2b e0 31 a4 3d d4 8f a6 ba 14 ee f7 c4 22
b5 ed c4 e7 af a0 4f dd 38 f4 02 d1 c8 bb 71 9a
bf
            ";

            RunOneCase("RSAES-OAEP Encryption Example 5.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example5_5()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample5(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
21 02 6e 68 00 c7 fa 72 8f ca ab a0 d1 96 ae 28
d7 a2 ac 4f fd 8a bc e7 94 f0 98 5f 60 c8 a6 73
72 77 36 5d 3f ea 11 db 89 23 a2 02 9a
            ";

            //# Seed:
            seed = @"
f0 28 74 13 23 4c c5 03 47 24 a0 94 c4 58 6b 87
af f1 33 fc
            ";

            //# Encryption:
            encryption = @"
07 b6 0e 14 ec 95 4b fd 29 e6 0d 00 47 e7 89 f5
1d 57 18 6c 63 58 99 03 30 67 93 ce d3 f6 82 41
c7 43 52 9a ba 6a 63 74 f9 2e 19 e0 16 3e fa 33
69 7e 19 6f 76 61 df aa a4 7a ac 6b de 5e 51 de
b5 07 c7 2c 58 9a 2c a1 69 3d 96 b1 46 03 81 24
9b 2c db 9e ac 44 76 9f 24 89 c5 d3 d2 f9 9f 0e
e3 c7 ee 5b f6 4a 5a c7 9c 42 bd 43 3f 14 9b e8
cb 59 54 83 61 64 05 95 51 3c 97 af 7b c2 50 97
23
            ";

            RunOneCase("RSAES-OAEP Encryption Example 5.5", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example5_6()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample5(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
54 1e 37 b6 8b 6c 88 72 b8 4c 02
            ";

            //# Seed:
            seed = @"
d9 fb a4 5c 96 f2 1e 6e 26 d2 9e b2 cd cb 65 85
be 9c b3 41
            ";

            //# Encryption:
            encryption = @"
08 c3 6d 4d da 33 42 3b 2e d6 83 0d 85 f6 41 1b
a1 dc f4 70 a1 fa e0 eb ef ee 7c 08 9f 25 6c ef
74 cb 96 ea 69 c3 8f 60 f3 9a be e4 41 29 bc b4
c9 2d e7 f7 97 62 3b 20 07 4e 3d 9c 28 99 70 1e
d9 07 1e 1e fa 0b dd 84 d4 c3 e5 13 03 02 d8 f0
24 0b ab a4 b8 4a 71 cc 03 2f 22 35 a5 ff 0f ae
27 7c 3e 8f 91 12 be f4 4c 9a e2 0d 17 5f c9 a4
05 8b fc 93 0b a3 1b 02 e2 e4 f4 44 48 37 10 f2
4a
            ";

            RunOneCase("RSAES-OAEP Encryption Example 5.6", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example6_1()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample6(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
40 46 ca 8b aa 33 47 ca 27 f4 9e 0d 81 f9 cc 1d
71 be 9b a5 17 d4
            ";

            //# Seed:
            seed = @"
dd 0f 6c fe 41 5e 88 e5 a4 69 a5 1f bb a6 df d4
0a db 43 84
            ";

            //# Encryption:
            encryption = @"
06 30 ee bc d2 85 6c 24 f7 98 80 6e 41 f9 e6 73
45 ed a9 ce da 38 6a cc 9f ac ae a1 ee ed 06 ac
e5 83 70 97 18 d9 d1 69 fa df 41 4d 5c 76 f9 29
96 83 3e f3 05 b7 5b 1e 4b 95 f6 62 a2 0f ae dc
3b ae 0c 48 27 a8 bf 8a 88 ed bd 57 ec 20 3a 27
a8 41 f0 2e 43 a6 15 ba b1 a8 ca c0 70 1d e3 4d
eb de f6 2a 08 80 89 b5 5e c3 6e a7 52 2f d3 ec
8d 06 b6 a0 73 e6 df 83 31 53 bc 0a ef d9 3b d1
a3
            ";

            RunOneCase("RSAES-OAEP Encryption Example 6.1", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example6_2()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample6(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
5c c7 2c 60 23 1d f0 3b 3d 40 f9 b5 79 31 bc 31
10 9f 97 25 27 f2 8b 19 e7 48 0c 72 88 cb 3c 92
b2 25 12 21 4e 4b e6 c9 14 79 2d da bd f5 7f aa
8a a7
            ";

            //# Seed:
            seed = @"
8d 14 bd 94 6a 13 51 14 8f 5c ae 2e d9 a0 c6 53
e8 5e bd 85
            ";

            //# Encryption:
            encryption = @"
0e bc 37 37 61 73 a4 fd 2f 89 cc 55 c2 ca 62 b2
6b 11 d5 1c 3c 7c e4 9e 88 45 f7 4e 76 07 31 7c
43 6b c8 d2 3b 96 67 df eb 9d 08 72 34 b4 7b c6
83 71 75 ae 5c 05 59 f6 b8 1d 7d 22 41 6d 3e 50
f4 ac 53 3d 8f 08 12 f2 db 9e 79 1f e9 c7 75 ac
8b 6a d0 f5 35 ad 9c eb 23 a4 a0 20 14 c5 8a b3
f8 d3 16 14 99 a2 60 f3 93 48 e7 14 ae 2a 1d 34
43 20 8f d8 b7 22 cc fd fb 39 3e 98 01 1f 99 e6
3f
            ";

            RunOneCase("RSAES-OAEP Encryption Example 6.2", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example6_3()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample6(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
b2 0e 65 13 03 09 2f 4b cc b4 30 70 c0 f8 6d 23
04 93 62 ed 96 64 2f c5 63 2c 27 db 4a 52 e3 d8
31 f2 ab 06 8b 23 b1 49 87 9c 00 2f 6b f3 fe ee
97 59 11 12 56 2c
            ";

            //# Seed:
            seed = @"
6c 07 5b c4 55 20 f1 65 c0 bf 5e a4 c5 df 19 1b
c9 ef 0e 44
            ";

            //# Encryption:
            encryption = @"
0a 98 bf 10 93 61 93 94 43 6c f6 8d 8f 38 e2 f1
58 fd e8 ea 54 f3 43 5f 23 9b 8d 06 b8 32 18 44
20 24 76 ae ed 96 00 94 92 48 0c e3 a8 d7 05 49
8c 4c 8c 68 f0 15 01 dc 81 db 60 8f 60 08 73 50
c8 c3 b0 bd 2e 9e f6 a8 14 58 b7 c8 01 b8 9f 2e
4f e9 9d 49 00 ba 6a 4b 5e 5a 96 d8 65 dc 67 6c
77 55 92 87 94 13 0d 62 80 a8 16 0a 19 0f 2d f3
ea 7c f9 aa 02 71 d8 8e 9e 69 05 ec f1 c5 15 2d
65
            ";

            RunOneCase("RSAES-OAEP Encryption Example 6.3", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example6_4()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample6(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
68 4e 30 38 c5 c0 41 f7
            ";

            //# Seed:
            seed = @"
3b bc 3b d6 63 7d fe 12 84 69 01 02 9b f5 b0 c0
71 03 43 9c
            ";

            //# Encryption:
            encryption = @"
00 8e 7a 67 ca cf b5 c4 e2 4b ec 7d ee 14 91 17
f1 95 98 ce 8c 45 80 8f ef 88 c6 08 ff 9c d6 e6
95 26 3b 9a 3c 0a d4 b8 ba 4c 95 23 8e 96 a8 42
2b 85 35 62 9c 8d 53 82 37 44 79 ad 13 fa 39 97
4b 24 2f 9a 75 9e ea f9 c8 3a d5 a8 ca 18 94 0a
01 62 ba 75 58 76 df 26 3f 4b d5 0c 65 25 c5 60
90 26 7c 1f 0e 09 ce 08 99 a0 cf 35 9e 88 12 0a
bd 9b f8 93 44 5b 3c ae 77 d3 60 73 59 ae 9a 52
f8
            ";

            RunOneCase("RSAES-OAEP Encryption Example 6.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example6_5()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample6(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
32 48 8c b2 62 d0 41 d6 e4 dd 35 f9 87 bf 3c a6
96 db 1f 06 ac 29 a4 46 93
            ";

            //# Seed:
            seed = @"
b4 6b 41 89 3e 8b ef 32 6f 67 59 38 3a 83 07 1d
ae 7f ca bc
            ";

            //# Encryption:
            encryption = @"
00 00 34 74 41 6c 7b 68 bd f9 61 c3 85 73 79 44
d7 f1 f4 0c b3 95 34 3c 69 3c c0 b4 fe 63 b3 1f
ed f1 ea ee ac 9c cc 06 78 b3 1d c3 2e 09 77 48
95 14 c4 f0 90 85 f6 29 8a 96 53 f0 1a ea 40 45
ff 58 2e e8 87 be 26 ae 57 5b 73 ee f7 f3 77 49
21 e3 75 a3 d1 9a dd a0 ca 31 aa 18 49 88 7c 1f
42 ca c9 67 7f 7a 2f 4e 92 3f 6e 5a 86 8b 38 c0
84 ef 18 75 94 dc 9f 7f 04 8f ea 2e 02 95 53 84
ab
            ";

            RunOneCase("RSAES-OAEP Encryption Example 6.5", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example6_6()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample6(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
50 ba 14 be 84 62 72 02 79 c3 06 ba
            ";

            //# Seed:
            seed = @"
0a 24 03 31 2a 41 e3 d5 2f 06 0f bc 13 a6 7d e5
cf 76 09 a7
            ";

            //# Encryption:
            encryption = @"
0a 02 6d da 5f c8 78 5f 7b d9 bf 75 32 7b 63 e8
5e 2c 0f de e5 da db 65 eb dc ac 9a e1 de 95 c9
2c 67 2a b4 33 aa 7a 8e 69 ce 6a 6d 88 97 fa c4
ac 4a 54 de 84 1a e5 e5 bb ce 76 87 87 9d 79 63
4c ea 7a 30 68 40 65 c7 14 d5 24 09 b9 28 25 6b
bf 53 ea bc d5 23 1e b7 25 95 04 53 73 99 bd 29
16 4b 72 6d 33 a4 6d a7 01 36 0a 41 68 a0 91 cc
ab 72 d4 4a 62 fe d2 46 c0 ff ea 5b 13 48 ab 54
70
            ";

            RunOneCase("RSAES-OAEP Encryption Example 6.6", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example7_1()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample7(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
47 aa e9 09
            ";

            //# Seed:
            seed = @"
43 dd 09 a0 7f f4 ca c7 1c aa 46 32 ee 5e 1c 1d
ae e4 cd 8f
            ";

            //# Encryption:
            encryption = @"
16 88 e4 ce 77 94 bb a6 cb 70 14 16 9e cd 55 9c
ed e2 a3 0b 56 a5 2b 68 d9 fe 18 cf 19 73 ef 97
b2 a0 31 53 95 1c 75 5f 62 94 aa 49 ad bd b5 58
45 ab 68 75 fb 39 86 c9 3e cf 92 79 62 84 0d 28
2f 9e 54 ce 8b 69 0f 7c 0c b8 bb d7 34 40 d9 57
1d 1b 16 cd 92 60 f9 ea b4 78 3c c4 82 e5 22 3d
c6 09 73 87 17 83 ec 27 b0 ae 0f d4 77 32 cb c2
86 a1 73 fc 92 b0 0f b4 ba 68 24 64 7c d9 3c 85
c1
            ";

            RunOneCase("RSAES-OAEP Encryption Example 7.1", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example7_2()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample7(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
1d 9b 2e 22 23 d9 bc 13 bf b9 f1 62 ce 73 5d b4
8b a7 c6 8f 68 22 a0 a1 a7 b6 ae 16 58 34 e7
            ";

            //# Seed:
            seed = @"
3a 9c 3c ec 7b 84 f9 bd 3a de cb c6 73 ec 99 d5
4b 22 bc 9b
            ";

            //# Encryption:
            encryption = @"
10 52 ed 39 7b 2e 01 e1 d0 ee 1c 50 bf 24 36 3f
95 e5 04 f4 a0 34 34 a0 8f d8 22 57 4e d6 b9 73
6e db b5 f3 90 db 10 32 14 79 a8 a1 39 35 0e 2b
d4 97 7c 37 78 ef 33 1f 3e 78 ae 11 8b 26 84 51
f2 0a 2f 01 d4 71 f5 d5 3c 56 69 37 17 1b 2d bc
2d 4b de 45 9a 57 99 f0 37 2d 65 74 23 9b 23 23
d2 45 d0 bb 81 c2 86 b6 3c 89 a3 61 01 73 37 e4
90 2f 88 a4 67 f4 c7 f2 44 bf d5 ab 46 43 7f f3
b6
            ";

            RunOneCase("RSAES-OAEP Encryption Example 7.2", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example7_3()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample7(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
d9 76 fc
            ";

            //# Seed:
            seed = @"
76 a7 5e 5b 61 57 a5 56 cf 88 84 bb 2e 45 c2 93
dd 54 5c f5
            ";

            //# Encryption:
            encryption = @"
21 55 cd 84 3f f2 4a 4e e8 ba db 76 94 26 00 28
a4 90 81 3b a8 b3 69 a4 cb f1 06 ec 14 8e 52 98
70 7f 59 65 be 7d 10 1c 10 49 ea 85 84 c2 4c d6
34 55 ad 9c 10 4d 68 62 82 d3 fb 80 3a 4c 11 c1
c2 e9 b9 1c 71 78 80 1d 1b 66 40 f0 03 f5 72 8d
f0 07 b8 a4 cc c9 2b ce 05 e4 1a 27 27 8d 7c 85
01 8c 52 41 43 13 a5 07 77 89 00 1d 4f 01 91 0b
72 aa d0 5d 22 0a a1 4a 58 73 3a 74 89 bc 54 55
6b
            ";

            RunOneCase("RSAES-OAEP Encryption Example 7.3", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example7_4()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample7(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
d4 73 86 23 df 22 3a a4 38 43 df 84 67 53 4c 41
d0 13 e0 c8 03 c6 24 e2 63 66 6b 23 9b de 40 a5
f2 9a eb 8d e7 9e 3d aa 61 dd 03 70 f4 9b d4 b0
13 83 4b 98 21 2a ef 6b 1c 5e e3 73 b3 cb
            ";

            //# Seed:
            seed = @"
78 66 31 4a 6a d6 f2 b2 50 a3 59 41 db 28 f5 86
4b 58 58 59
            ";

            //# Encryption:
            encryption = @"
0a b1 4c 37 3a eb 7d 43 28 d0 aa ad 8c 09 4d 88
b9 eb 09 8b 95 f2 10 54 a2 90 82 52 2b e7 c2 7a
31 28 78 b6 37 91 7e 3d 81 9e 6c 3c 56 8d b5 d8
43 80 2b 06 d5 1d 9e 98 a2 be 0b f4 0c 03 14 23
b0 0e df bf f8 32 0e fb 91 71 bd 20 44 65 3a 4c
b9 c5 12 2f 6c 65 e8 3c da 2e c3 c1 26 02 7a 9c
1a 56 ba 87 4d 0f ea 23 f3 80 b8 2c f2 40 b8 cf
54 00 04 75 8c 4c 77 d9 34 15 7a 74 f3 fc 12 bf
ac
            ";

            RunOneCase("RSAES-OAEP Encryption Example 7.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example7_5()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample7(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
bb 47 23 1c a5 ea 1d 3a d4 6c 99 34 5d 9a 8a 61
            ";

            //# Seed:
            seed = @"
b2 16 6e d4 72 d5 8d b1 0c ab 2c 6b 00 0c cc f1
0a 7d c5 09
            ";

            //# Encryption:
            encryption = @"
02 83 87 a3 18 27 74 34 79 8b 4d 97 f4 60 06 8d
f5 29 8f ab a5 04 1b a1 17 61 a1 cb 73 16 b2 41
84 11 4e c5 00 25 7e 25 89 ed 3b 60 7a 1e bb e9
7a 6c c2 e0 2b f1 b6 81 f4 23 12 a3 3b 7a 77 d8
e7 85 5c 4a 6d e0 3e 3c 04 64 3f 78 6b 91 a2 64
a0 d6 80 5e 2c ea 91 e6 81 77 eb 7a 64 d9 25 5e
4f 27 e7 13 b7 cc ec 00 dc 20 0e bd 21 c2 ea 2b
b8 90 fe ae 49 42 df 94 1d c3 f9 78 90 ed 34 74
78
            ";

            RunOneCase("RSAES-OAEP Encryption Example 7.5", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example7_6()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample7(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
21 84 82 70 95 d3 5c 3f 86 f6 00 e8 e5 97 54 01
32 96
            ";

            //# Seed:
            seed = @"
52 67 3b de 2c a1 66 c2 aa 46 13 1a c1 dc 80 8d
67 d7 d3 b1
            ";

            //# Encryption:
            encryption = @"
14 c6 78 a9 4a d6 05 25 ef 39 e9 59 b2 f3 ba 5c
09 7a 94 ff 91 2b 67 db ac e8 05 35 c1 87 ab d4
7d 07 54 20 b1 87 21 52 bb a0 8f 7f c3 1f 31 3b
bf 92 73 c9 12 fc 4c 01 49 a9 b0 cf b7 98 07 e3
46 eb 33 20 69 61 1b ec 0f f9 bc d1 68 f1 f7 c3
3e 77 31 3c ea 45 4b 94 e2 54 9e ec f0 02 e2 ac
f7 f6 f2 d2 84 5d 4f e0 aa b2 e5 a9 2d df 68 c4
80 ae 11 24 79 35 d1 f6 25 74 84 22 16 ae 67 41
15
            ";

            RunOneCase("RSAES-OAEP Encryption Example 7.6", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example8_1()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample8(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
05 0b 75 5e 5e 68 80 f7 b9 e9 d6 92 a7 4c 37 aa
e4 49 b3 1b fe a6 de ff 83 74 7a 89 7f 6c 2c 82
5b b1 ad bf 85 0a 3c 96 99 4b 5d e5 b3 3c bc 7d
4a 17 91 3a 79 67
            ";

            //# Seed:
            seed = @"
77 06 ff ca 1e cf b1 eb ee 2a 55 e5 c6 e2 4c d2
79 7a 41 25
            ";

            //# Encryption:
            encryption = @"
09 b3 68 3d 8a 2e b0 fb 29 5b 62 ed 1f b9 29 0b
71 44 57 b7 82 53 19 f4 64 78 72 af 88 9b 30 40
94 72 02 0a d1 29 12 bf 19 b1 1d 48 19 f4 96 14
82 4f fd 84 d0 9c 0a 17 e7 d1 73 09 d1 29 19 79
04 10 aa 29 95 69 9f 6a 86 db e3 24 2b 5a cc 23
af 45 69 10 80 d6 b1 ae 81 0f b3 e3 05 70 87 f0
97 00 92 ce 00 be 95 62 ff 40 53 b6 26 2c e0 ca
a9 3e 13 72 3d 2e 3a 5b a0 75 d4 5f 0d 61 b5 4b
61
            ";

            RunOneCase("RSAES-OAEP Encryption Example 8.1", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example8_2()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample8(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
4e b6 8d cd 93 ca 9b 19 df 11 1b d4 36 08 f5 57
02 6f e4 aa 1d 5c fa c2 27 a3 eb 5a b9 54 8c 18
a0 6d de d2 3f 81 82 59 86 b2 fc d7 11 09 ec ef
7e ff 88 87 3f 07 5c 2a a0 c4 69 f6 9c 92 bc
            ";

            //# Seed:
            seed = @"
a3 71 7d a1 43 b4 dc ff bc 74 26 65 a8 fa 95 05
85 54 83 43
            ";

            //# Encryption:
            encryption = @"
2e cf 15 c9 7c 5a 15 b1 47 6a e9 86 b3 71 b5 7a
24 28 4f 4a 16 2a 8d 0c 81 82 e7 90 5e 79 22 56
f1 81 2b a5 f8 3f 1f 7a 13 0e 42 dc c0 22 32 84
4e dc 14 a3 1a 68 ee 97 ae 56 4a 38 3a 34 11 65
64 24 c5 f6 2d db 64 60 93 c3 67 be 1f cd a4 26
cf 00 a0 6d 8a cb 7e 57 77 6f bb d8 55 ac 3d f5
06 fc 16 b1 d7 c3 f2 11 0f 3d 80 68 e9 1e 18 63
63 83 1c 84 09 68 0d 8d a9 ec d8 cf 1f a2 0e e3
9d
            ";

            RunOneCase("RSAES-OAEP Encryption Example 8.2", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example8_3()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample8(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
86 04 ac 56 32 8c 1a b5 ad 91 78 61
            ";

            //# Seed:
            seed = @"
ee 06 20 90 73 cc a0 26 bb 26 4e 51 85 bf 8c 68
b7 73 9f 86
            ";

            //# Encryption:
            encryption = @"
4b c8 91 30 a5 b2 da bb 7c 2f cf 90 eb 5d 0e af
9e 68 1b 71 46 a3 8f 31 73 a3 d9 cf ec 52 ea 9e
0a 41 93 2e 64 8a 9d 69 34 4c 50 da 76 3f 51 a0
3c 95 76 21 31 e8 05 22 54 dc d2 24 8c ba 40 fd
31 66 77 86 ce 05 a2 b7 b5 31 ac 9d ac 9e d5 84
a5 9b 67 7c 1a 8a ed 8c 5d 15 d6 8c 05 56 9e 2b
e7 80 bf 7d b6 38 fd 2b fd 2a 85 ab 27 68 60 f3
77 73 38 fc a9 89 ff d7 43 d1 3e e0 8e 0c a9 89
3f
            ";

            RunOneCase("RSAES-OAEP Encryption Example 8.3", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example8_4()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample8(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
fd da 5f bf 6e c3 61 a9 d9 a4 ac 68 af 21 6a 06
86 f4 38 b1 e0 e5 c3 6b 95 5f 74 e1 07 f3 9c 0d
dd cc
            ";

            //# Seed:
            seed = @"
99 0a d5 73 dc 48 a9 73 23 5b 6d 82 54 36 18 f2
e9 55 10 5d
            ";

            //# Encryption:
            encryption = @"
2e 45 68 47 d8 fc 36 ff 01 47 d6 99 35 94 b9 39
72 27 d5 77 75 2c 79 d0 f9 04 fc b0 39 d4 d8 12
fe a6 05 a7 b5 74 dd 82 ca 78 6f 93 75 23 48 43
8e e9 f5 b5 45 49 85 d5 f0 e1 69 9e 3e 7a d1 75
a3 2e 15 f0 3d eb 04 2a b9 fe 1d d9 db 1b b8 6f
8c 08 9c cb 45 e7 ef 0c 5e e7 ca 9b 72 90 ca 6b
15 be d4 70 39 78 8a 8a 93 ff 83 e0 e8 d6 24 4c
71 00 63 62 de ef 69 b6 f4 16 fb 3c 68 43 83 fb
d0
            ";

            RunOneCase("RSAES-OAEP Encryption Example 8.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example8_5()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample8(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
4a 5f 49 14 be e2 5d e3 c6 93 41 de 07
            ";

            //# Seed:
            seed = @"
ec c6 3b 28 f0 75 6f 22 f5 2a c8 e6 ec 12 51 a6
ec 30 47 18
            ";

            //# Encryption:
            encryption = @"
1f b9 35 6f d5 c4 b1 79 6d b2 eb f7 d0 d3 93 cc
81 0a df 61 45 de fc 2f ce 71 4f 79 d9 38 00 d5
e2 ac 21 1e a8 bb ec ca 4b 65 4b 94 c3 b1 8b 30
dd 57 6c e3 4d c9 54 36 ef 57 a0 94 15 64 59 23
35 9a 5d 7b 41 71 ef 22 c2 46 70 f1 b2 29 d3 60
3e 91 f7 66 71 b7 df 97 e7 31 7c 97 73 44 76 d5
f3 d1 7d 21 cf 82 b5 ba 9f 83 df 2e 58 8d 36 98
4f d1 b5 84 46 8b d2 3b 2e 87 5f 32 f6 89 53 f7
b2
            ";

            RunOneCase("RSAES-OAEP Encryption Example 8.5", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example8_6()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample8(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
8e 07 d6 6f 7b 88 0a 72 56 3a bc d3 f3 50 92 bc
33 40 9f b7 f8 8f 24 72 be
            ";

            //# Seed:
            seed = @"
39 25 c7 1b 36 2d 40 a0 a6 de 42 14 55 79 ba 1e
7d d4 59 fc
            ";

            //# Encryption:
            encryption = @"
3a fd 9c 66 00 14 7b 21 79 8d 81 8c 65 5a 0f 4c
92 12 db 26 d0 b0 df dc 2a 75 94 cc b3 d2 2f 5b
f1 d7 c3 e1 12 cd 73 fc 7d 50 9c 7a 8b af dd 3c
27 4d 13 99 00 9f 96 09 ec 4b e6 47 7e 45 3f 07
5a a3 3d b3 82 87 0c 1c 34 09 ae f3 92 d7 38 6a
e3 a6 96 b9 9a 94 b4 da 05 89 44 7e 95 5d 16 c9
8b 17 60 2a 59 bd 73 62 79 fc d8 fb 28 0c 44 62
d5 90 bf a9 bf 13 fe d5 70 ea fd e9 73 30 a2 c2
10
            ";

            RunOneCase("RSAES-OAEP Encryption Example 8.6", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example9_1()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample9(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
f7 35 fd 55 ba 92 59 2c 3b 52 b8 f9 c4 f6 9a aa
1c be f8 fe 88 ad d0 95 59 54 12 46 7f 9c f4 ec
0b 89 6c 59 ed a1 62 10 e7 54 9c 8a bb 10 cd bc
21 a1 2e c9 b6 b5 b8 fd 2f 10 39 9e b6
            ";

            //# Seed:
            seed = @"
8e c9 65 f1 34 a3 ec 99 31 e9 2a 1c a0 dc 81 69
d5 ea 70 5c
            ";

            //# Encryption:
            encryption = @"
26 7b cd 11 8a ca b1 fc 8b a8 1c 85 d7 30 03 cb
86 10 fa 55 c1 d9 7d a8 d4 8a 7c 7f 06 89 6a 4d
b7 51 aa 28 42 55 b9 d3 6a d6 5f 37 65 3d 82 9f
1b 37 f9 7b 80 01 94 25 45 b2 fc 2c 55 a7 37 6c
a7 a1 be 4b 17 60 c8 e0 5a 33 e5 aa 25 26 b8 d9
8e 31 70 88 e7 83 4c 75 5b 2a 59 b1 26 31 a1 82
c0 5d 5d 43 ab 17 79 26 4f 84 56 f5 15 ce 57 df
df 51 2d 54 93 da b7 b7 33 8d c4 b7 d7 8d b9 c0
91 ac 3b af 53 7a 69 fc 7f 54 9d 97 9f 0e ff 9a
94 fd a4 16 9b d4 d1 d1 9a 69 c9 9e 33 c3 b5 54
90 d5 01 b3 9b 1e da e1 18 ff 67 93 a1 53 26 15
84 d3 a5 f3 9f 6e 68 2e 3d 17 c8 cd 12 61 fa 72
            ";

            RunOneCase("RSAES-OAEP Encryption Example 9.1", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example9_2()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample9(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
81 b9 06 60 50 15 a6 3a ab e4 2d df 11 e1 97 89
12 f5 40 4c 74 74 b2 6d ce 3e d4 82 bf 96 1e cc
81 8b f4 20 c5 46 59
            ";

            //# Seed:
            seed = @"
ec b1 b8 b2 5f a5 0c da b0 8e 56 04 28 67 f4 af
58 26 d1 6c
            ";

            //# Encryption:
            encryption = @"
93 ac 9f 06 71 ec 29 ac bb 44 4e ff c1 a5 74 13
51 d6 0f db 0e 39 3f bf 75 4a cf 0d e4 97 61 a1
48 41 df 77 72 e9 bc 82 77 39 66 a1 58 4c 4d 72
ba ea 00 11 8f 83 f3 5c ca 6e 53 7c bd 4d 81 1f
55 83 b2 97 83 d8 a6 d9 4c d3 1b e7 0d 6f 52 6c
10 ff 09 c6 fa 7c e0 69 79 5a 3f cd 05 11 fd 5f
cb 56 4b cc 80 ea 9c 78 f3 8b 80 01 25 39 d8 a4
dd f6 fe 81 e9 cd db 7f 50 db bb bc c7 e5 d8 60
97 cc f4 ec 49 18 9f b8 bf 31 8b e6 d5 a0 71 5d
51 6b 49 af 19 12 58 cd 32 dc 83 3c e6 eb 46 73
c0 3a 19 bb ac e8 8c c5 48 95 f6 36 cc 0c 1e c8
90 96 d1 1c e2 35 a2 65 ca 17 64 23 2a 68 9a e8
            ";

            RunOneCase("RSAES-OAEP Encryption Example 9.2", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example9_3()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample9(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
fd 32 64 29 df 9b 89 0e 09 b5 4b 18 b8 f3 4f 1e
24
            ";

            //# Seed:
            seed = @"
e8 9b b0 32 c6 ce 62 2c bd b5 3b c9 46 60 14 ea
77 f7 77 c0
            ";

            //# Encryption:
            encryption = @"
81 eb dd 95 05 4b 0c 82 2e f9 ad 76 93 f5 a8 7a
df b4 b4 c4 ce 70 df 2d f8 4e d4 9c 04 da 58 ba
5f c2 0a 19 e1 a6 e8 b7 a3 90 0b 22 79 6d c4 e8
69 ee 6b 42 79 2d 15 a8 ec eb 56 c0 9c 69 91 4e
81 3c ea 8f 69 31 e4 b8 ed 6f 42 1a f2 98 d5 95
c9 7f 47 89 c7 ca a6 12 c7 ef 36 09 84 c2 1b 93
ed c5 40 10 68 b5 af 4c 78 a8 77 1b 98 4d 53 b8
ea 8a df 2f 6a 7d 4a 0b a7 6c 75 e1 dd 9f 65 8f
20 de d4 a4 60 71 d4 6d 77 91 b5 68 03 d8 fe a7
f0 b0 f8 e4 1a e3 f0 93 83 a6 f9 58 5f e7 75 3e
aa ff d2 bf 94 56 31 08 be ec c2 07 bb b5 35 f5
fc c7 05 f0 dd e9 f7 08 c6 2f 49 a9 c9 03 71 d3
            ";

            RunOneCase("RSAES-OAEP Encryption Example 9.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example9_4()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample9(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
f1 45 9b 5f 0c 92 f0 1a 0f 72 3a 2e 56 62 48 4d
8f 8c 0a 20 fc 29 da d6 ac d4 3b b5 f3 ef fd f4
e1 b6 3e 07 fd fe 66 28 d0 d7 4c a1 9b f2 d6 9e
4a 0a bf 86 d2 93 92 5a 79 67 72 f8 08 8e
            ";

            //# Seed:
            seed = @"
60 6f 3b 99 c0 b9 cc d7 71 ea a2 9e a0 e4 c8 84
f3 18 9c cc
            ";

            //# Encryption:
            encryption = @"
bc c3 5f 94 cd e6 6c b1 13 66 25 d6 25 b9 44 32
a3 5b 22 f3 d2 fa 11 a6 13 ff 0f ca 5b d5 7f 87
b9 02 cc dc 1c d0 ae bc b0 71 5e e8 69 d1 d1 fe
39 5f 67 93 00 3f 5e ca 46 50 59 c8 86 60 d4 46
ff 5f 08 18 55 20 22 55 7e 38 c0 8a 67 ea d9 91
26 22 54 f1 06 82 97 5e c5 63 97 76 85 37 f4 97
7a f6 d5 f6 aa ce b7 fb 25 de c5 93 72 30 23 1f
d8 97 8a f4 91 19 a2 9f 29 e4 24 ab 82 72 b4 75
62 79 2d 5c 94 f7 74 b8 82 9d 0b 0d 9f 1a 8c 9e
dd f3 75 74 d5 fa 24 8e ef a9 c5 27 1f c5 ec 25
79 c8 1b dd 61 b4 10 fa 61 fe 36 e4 24 22 1c 11
3a dd b2 75 66 4c 80 1d 34 ca 8c 63 51 e4 a8 58
            ";

            RunOneCase("RSAES-OAEP Encryption Example 9.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example9_5()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample9(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
53 e6 e8 c7 29 d6 f9 c3 19 dd 31 7e 74 b0 db 8e
4c cc a2 5f 3c 83 05 74 6e 13 7a c6 3a 63 ef 37
39 e7 b5 95 ab b9 6e 8d 55 e5 4f 7b d4 1a b4 33
37 8f fb 91 1d
            ";

            //# Seed:
            seed = @"
fc bc 42 14 02 e9 ec ab c6 08 2a fa 40 ba 5f 26
52 2c 84 0e
            ";

            //# Encryption:
            encryption = @"
23 2a fb c9 27 fa 08 c2 f6 a2 7b 87 d4 a5 cb 09
c0 7d c2 6f ae 73 d7 3a 90 55 88 39 f4 fd 66 d2
81 b8 7e c7 34 bc e2 37 ba 16 66 98 ed 82 91 06
a7 de 69 42 cd 6c dc e7 8f ed 8d 2e 4d 81 42 8e
66 49 0d 03 62 64 ce f9 2a f9 41 d3 e3 50 55 fe
39 81 e1 4d 29 cb b9 a4 f6 74 73 06 3b ae c7 9a
11 79 f5 a1 7c 9c 18 32 f2 83 8f d7 d5 e5 9b b9
65 9d 56 dc e8 a0 19 ed ef 1b b3 ac cc 69 7c c6
cc 7a 77 8f 60 a0 64 c7 f6 f5 d5 29 c6 21 02 62
e0 03 de 58 3e 81 e3 16 7b 89 97 1f b8 c0 e1 5d
44 ff fe f8 9b 53 d8 d6 4d d7 97 d1 59 b5 6d 2b
08 ea 53 07 ea 12 c2 41 bd 58 d4 ee 27 8a 1f 2e
            ";

            RunOneCase("RSAES-OAEP Encryption Example 9.5", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example9_6()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample9(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
b6 b2 8e a2 19 8d 0c 10 08 bc 64
            ";

            //# Seed:
            seed = @"
23 aa de 0e 1e 08 bb 9b 9a 78 d2 30 2a 52 f9 c2
1b 2e 1b a2
            ";

            //# Encryption:
            encryption = @"
43 8c c7 dc 08 a6 8d a2 49 e4 25 05 f8 57 3b a6
0e 2c 27 73 d5 b2 90 f4 cf 9d ff 71 8e 84 20 81
c3 83 e6 70 24 a0 f2 95 94 ea 98 7b 9d 25 e4 b7
38 f2 85 97 0d 19 5a bb 3a 8c 80 54 e3 d7 9d 6b
9c 9a 83 27 ba 59 6f 12 59 e2 71 26 67 47 66 90
7d 8d 58 2f f3 a8 47 61 54 92 9a db 1e 6d 12 35
b2 cc b4 ec 8f 66 3b a9 cc 67 0a 92 be bd 85 3c
8d bf 69 c6 43 6d 01 6f 61 ad d8 36 e9 47 32 45
04 34 20 7f 9f d4 c4 3d ec 2a 12 a9 58 ef a0 1e
fe 26 69 89 9b 5e 60 4c 25 5c 55 fb 71 66 de 55
89 e3 69 59 7b b0 91 68 c0 6d d5 db 17 7e 06 a1
74 0e b2 d5 c8 2f ae ca 6d 92 fc ee 99 31 ba 9f
            ";

            RunOneCase("RSAES-OAEP Encryption Example 9.6", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example10_1()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample10(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
8b ba 6b f8 2a 6c 0f 86 d5 f1 75 6e 97 95 68 70
b0 89 53 b0 6b 4e b2 05 bc 16 94 ee
            ";

            //# Seed:
            seed = @"
47 e1 ab 71 19 fe e5 6c 95 ee 5e aa d8 6f 40 d0
aa 63 bd 33
            ";

            //# Encryption:
            encryption = @"
53 ea 5d c0 8c d2 60 fb 3b 85 85 67 28 7f a9 15
52 c3 0b 2f eb fb a2 13 f0 ae 87 70 2d 06 8d 19
ba b0 7f e5 74 52 3d fb 42 13 9d 68 c3 c5 af ee
e0 bf e4 cb 79 69 cb f3 82 b8 04 d6 e6 13 96 14
4e 2d 0e 60 74 1f 89 93 c3 01 4b 58 b9 b1 95 7a
8b ab cd 23 af 85 4f 4c 35 6f b1 66 2a a7 2b fc
c7 e5 86 55 9d c4 28 0d 16 0c 12 67 85 a7 23 eb
ee be ff 71 f1 15 94 44 0a ae f8 7d 10 79 3a 87
74 a2 39 d4 a0 4c 87 fe 14 67 b9 da f8 52 08 ec
6c 72 55 79 4a 96 cc 29 14 2f 9a 8b d4 18 e3 c1
fd 67 34 4b 0c d0 82 9d f3 b2 be c6 02 53 19 62
93 c6 b3 4d 3f 75 d3 2f 21 3d d4 5c 62 73 d5 05
ad f4 cc ed 10 57 cb 75 8f c2 6a ee fa 44 12 55
ed 4e 64 c1 99 ee 07 5e 7f 16 64 61 82 fd b4 64
73 9b 68 ab 5d af f0 e6 3e 95 52 01 68 24 f0 54
bf 4d 3c 8c 90 a9 7b b6 b6 55 32 84 eb 42 9f cc
            ";

            RunOneCase("RSAES-OAEP Encryption Example 10.1", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example10_2()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample10(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
e6 ad 18 1f 05 3b 58 a9 04 f2 45 75 10 37 3e 57
            ";

            //# Seed:
            seed = @"
6d 17 f5 b4 c1 ff ac 35 1d 19 5b f7 b0 9d 09 f0
9a 40 79 cf
            ";

            //# Encryption:
            encryption = @"
a2 b1 a4 30 a9 d6 57 e2 fa 1c 2b b5 ed 43 ff b2
5c 05 a3 08 fe 90 93 c0 10 31 79 5f 58 74 40 01
10 82 8a e5 8f b9 b5 81 ce 9d dd d3 e5 49 ae 04
a0 98 54 59 bd e6 c6 26 59 4e 7b 05 dc 42 78 b2
a1 46 5c 13 68 40 88 23 c8 5e 96 dc 66 c3 a3 09
83 c6 39 66 4f c4 56 9a 37 fe 21 e5 a1 95 b5 77
6e ed 2d f8 d8 d3 61 af 68 6e 75 02 29 bb d6 63
f1 61 86 8a 50 61 5e 0c 33 7b ec 0c a3 5f ec 0b
b1 9c 36 eb 2e 0b bc c0 58 2f a1 d9 3a ac db 06
10 63 f5 9f 2c e1 ee 43 60 5e 5d 89 ec a1 83 d2
ac df e9 f8 10 11 02 2a d3 b4 3a 3d d4 17 da c9
4b 4e 11 ea 81 b1 92 96 6e 96 6b 18 20 82 e7 19
64 60 7b 4f 80 02 f3 62 99 84 4a 11 f2 ae 0f ae
ac 2e ae 70 f8 f4 f9 80 88 ac dc d0 ac 55 6e 9f
cc c5 11 52 19 08 fa d2 6f 04 c6 42 01 45 03 05
77 87 58 b0 53 8b f8 b5 bb 14 4a 82 8e 62 97 95
            ";

            RunOneCase("RSAES-OAEP Encryption Example 10.2", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example10_3()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample10(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
51 0a 2c f6 0e 86 6f a2 34 05 53 c9 4e a3 9f bc
25 63 11 e8 3e 94 45 4b 41 24
            ";

            //# Seed:
            seed = @"
38 53 87 51 4d ec cc 7c 74 0d d8 cd f9 da ee 49
a1 cb fd 54
            ";

            //# Encryption:
            encryption = @"
98 86 c3 e6 76 4a 8b 9a 84 e8 41 48 eb d8 c3 b1
aa 80 50 38 1a 78 f6 68 71 4c 16 d9 cf d2 a6 ed
c5 69 79 c5 35 d9 de e3 b4 4b 85 c1 8b e8 92 89
92 37 17 11 47 22 16 d9 5d da 98 d2 ee 83 47 c9
b1 4d ff df f8 4a a4 8d 25 ac 06 f7 d7 e6 53 98
ac 96 7b 1c e9 09 25 f6 7d ce 04 9b 7f 81 2d b0
74 29 97 a7 4d 44 fe 81 db e0 e7 a3 fe af 2e 5c
40 af 88 8d 55 0d db be 3b c2 06 57 a2 95 43 f8
fc 29 13 b9 bd 1a 61 b2 ab 22 56 ec 40 9b bd 7d
c0 d1 77 17 ea 25 c4 3f 42 ed 27 df 87 38 bf 4a
fc 67 66 ff 7a ff 08 59 55 5e e2 83 92 0f 4c 8a
63 c4 a7 34 0c ba fd dc 33 9e cd b4 b0 51 50 02
f9 6c 93 2b 5b 79 16 7a f6 99 c0 ad 3f cc fd f0
f4 4e 85 a7 02 62 bf 2e 18 fe 34 b8 50 58 99 75
e8 67 ff 96 9d 48 ea bf 21 22 71 54 6c dc 05 a6
9e cb 52 6e 52 87 0c 83 6f 30 7b d7 98 78 0e de
            ";

            RunOneCase("RSAES-OAEP Encryption Example 10.3", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example10_4()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample10(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
bc dd 19 0d a3 b7 d3 00 df 9a 06 e2 2c aa e2 a7
5f 10 c9 1f f6 67 b7 c1 6b de 8b 53 06 4a 26 49
a9 40 45 c9
            ";

            //# Seed:
            seed = @"
5c ac a6 a0 f7 64 16 1a 96 84 f8 5d 92 b6 e0 ef
37 ca 8b 65
            ";

            //# Encryption:
            encryption = @"
63 18 e9 fb 5c 0d 05 e5 30 7e 16 83 43 6e 90 32
93 ac 46 42 35 8a aa 22 3d 71 63 01 3a ba 87 e2
df da 8e 60 c6 86 0e 29 a1 e9 26 86 16 3e a0 b9
17 5f 32 9c a3 b1 31 a1 ed d3 a7 77 59 a8 b9 7b
ad 6a 4f 8f 43 96 f2 8c f6 f3 9c a5 81 12 e4 81
60 d6 e2 03 da a5 85 6f 3a ca 5f fe d5 77 af 49
94 08 e3 df d2 33 e3 e6 04 db e3 4a 9c 4c 90 82
de 65 52 7c ac 63 31 d2 9d c8 0e 05 08 a0 fa 71
22 e7 f3 29 f6 cc a5 cf a3 4d 4d 1d a4 17 80 54
57 e0 08 be c5 49 e4 78 ff 9e 12 a7 63 c4 77 d1
5b bb 78 f5 b6 9b d5 78 30 fc 2c 4e d6 86 d7 9b
c7 2a 95 d8 5f 88 13 4c 6b 0a fe 56 a8 cc fb c8
55 82 8b b3 39 bd 17 90 9c f1 d7 0d e3 33 5a e0
70 39 09 3e 60 6d 65 53 65 de 65 50 b8 72 cd 6d
e1 d4 40 ee 03 1b 61 94 5f 62 9a d8 a3 53 b0 d4
09 39 e9 6a 3c 45 0d 2a 8d 5e ee 9f 67 80 93 c8
            ";

            RunOneCase("RSAES-OAEP Encryption Example 10.4", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example10_5()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample10(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
a7 dd 6c 7d c2 4b 46 f9 dd 5f 1e 91 ad a4 c3 b3
df 94 7e 87 72 32 a9
            ";

            //# Seed:
            seed = @"
95 bc a9 e3 85 98 94 b3 dd 86 9f a7 ec d5 bb c6
40 1b f3 e4
            ";

            //# Encryption:
            encryption = @"
75 29 08 72 cc fd 4a 45 05 66 0d 65 1f 56 da 6d
aa 09 ca 13 01 d8 90 63 2f 6a 99 2f 3d 56 5c ee
46 4a fd ed 40 ed 3b 5b e9 35 67 14 ea 5a a7 65
5f 4a 13 66 c2 f1 7c 72 8f 6f 2c 5a 5d 1f 8e 28
42 9b c4 e6 f8 f2 cf f8 da 8d c0 e0 a9 80 8e 45
fd 09 ea 2f a4 0c b2 b6 ce 6f ff f5 c0 e1 59 d1
1b 68 d9 0a 85 f7 b8 4e 10 3b 09 e6 82 66 64 80
c6 57 50 5c 09 29 25 94 68 a3 14 78 6d 74 ea b1
31 57 3c f2 34 bf 57 db 7d 9e 66 cc 67 48 19 2e
00 2d c0 de ea 93 05 85 f0 83 1f dc d9 bc 33 d5
1f 79 ed 2f fc 16 bc f4 d5 98 12 fc eb ca a3 f9
06 9b 0e 44 56 86 d6 44 c2 5c cf 63 b4 56 ee 5f
a6 ff e9 6f 19 cd f7 51 fe d9 ea f3 59 57 75 4d
bf 4b fe a5 21 6a a1 84 4d c5 07 cb 2d 08 0e 72
2e ba 15 03 08 c2 b5 ff 11 93 62 0f 17 66 ec f4
48 1b af b9 43 bd 29 28 77 f2 13 6c a4 94 ab a0
            ";

            RunOneCase("RSAES-OAEP Encryption Example 10.5", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        [Test]
        public static void TestRSAES_OAEP_Encryption_Example10_6()
        {
            string n, e, d, p, q, dp, dq, qinv, message, seed, encryption;
            SetupKeysExample10(out n, out e, out d, out p, out q, out dp, out dq, out qinv);

            //# Message to be encrypted:
            message = @"
ea f1 a7 3a 1b 0c 46 09 53 7d e6 9c d9 22 8b bc
fb 9a 8c a8 c6 c3 ef af 05 6f e4 a7 f4 63 4e d0
0b 7c 39 ec 69 22 d7 b8 ea 2c 04 eb ac
            ";

            //# Seed:
            seed = @"
9f 47 dd f4 2e 97 ee a8 56 a9 bd bc 71 4e b3 ac
22 f6 eb 32
            ";

            //# Encryption:
            encryption = @"
2d 20 7a 73 43 2a 8f b4 c0 30 51 b3 f7 3b 28 a6
17 64 09 8d fa 34 c4 7a 20 99 5f 81 15 aa 68 16
67 9b 55 7e 82 db ee 58 49 08 c6 e6 97 82 d7 de
b3 4d bd 65 af 06 3d 57 fc a7 6a 5f d0 69 49 2f
d6 06 8d 99 84 d2 09 35 05 65 a6 2e 5c 77 f2 30
38 c1 2c b1 0c 66 34 70 9b 54 7c 46 f6 b4 a7 09
bd 85 ca 12 2d 74 46 5e f9 77 62 c2 97 63 e0 6d
bc 7a 9e 73 8c 78 bf ca 01 02 dc 5e 79 d6 5b 97
3f 28 24 0c aa b2 e1 61 a7 8b 57 d2 62 45 7e d8
19 5d 53 e3 c7 ae 9d a0 21 88 3c 6d b7 c2 4a fd
d2 32 2e ac 97 2a d3 c3 54 c5 fc ef 1e 14 6c 3a
02 90 fb 67 ad f0 07 06 6e 00 42 8d 2c ec 18 ce
58 f9 32 86 98 de fe f4 b2 eb 5e c7 69 18 fd e1
c1 98 cb b3 8b 7a fc 67 62 6a 9a ef ec 43 22 bf
d9 0d 25 63 48 1c 9a 22 1f 78 c8 27 2c 82 d1 b6
2a b9 14 e1 c6 9f 6a f6 ef 30 ca 52 60 db 4a 46
            ";

            RunOneCase("RSAES-OAEP Encryption Example 10.6", n, e, d, p, q, dp, dq, qinv, message, seed, encryption);
        }

        private static void SetupKeysExample1(out string n, out string e, out string d, out string p, out string q, out string dp, out string dq, out string qinv)
        {
            //# RSA modulus n:
            n = @"
            a8 b3 b2 84 af 8e b5 0b 38 70 34 a8 60 f1 46 c4
            91 9f 31 87 63 cd 6c 55 98 c8 ae 48 11 a1 e0 ab
            c4 c7 e0 b0 82 d6 93 a5 e7 fc ed 67 5c f4 66 85
            12 77 2c 0c bc 64 a7 42 c6 c6 30 f5 33 c8 cc 72
            f6 2a e8 33 c4 0b f2 58 42 e9 84 bb 78 bd bf 97
            c0 10 7d 55 bd b6 62 f5 c4 e0 fa b9 84 5c b5 14
            8e f7 39 2d d3 aa ff 93 ae 1e 6b 66 7b b3 d4 24
            76 16 d4 f5 ba 10 d4 cf d2 26 de 88 d3 9f 16 fb";

            //# RSA public exponent e:
            e = @"01 00 01";

            //# RSA private exponent d:
            d = @"
            53 33 9c fd b7 9f c8 46 6a 65 5c 73 16 ac a8 5c
            55 fd 8f 6d d8 98 fd af 11 95 17 ef 4f 52 e8 fd
            8e 25 8d f9 3f ee 18 0f a0 e4 ab 29 69 3c d8 3b
            15 2a 55 3d 4a c4 d1 81 2b 8b 9f a5 af 0e 7f 55
            fe 73 04 df 41 57 09 26 f3 31 1f 15 c4 d6 5a 73
            2c 48 31 16 ee 3d 3d 2d 0a f3 54 9a d9 bf 7c bf
            b7 8a d8 84 f8 4d 5b eb 04 72 4d c7 36 9b 31 de
            f3 7d 0c f5 39 e9 cf cd d3 de 65 37 29 ea d5 d1";

            //# Prime p:
            p = @"
            d3 27 37 e7 26 7f fe 13 41 b2 d5 c0 d1 50 a8 1b
            58 6f b3 13 2b ed 2f 8d 52 62 86 4a 9c b9 f3 0a
            f3 8b e4 48 59 8d 41 3a 17 2e fb 80 2c 21 ac f1
            c1 1c 52 0c 2f 26 a4 71 dc ad 21 2e ac 7c a3 9d";

            //# Prime q:
            q = @"
            cc 88 53 d1 d5 4d a6 30 fa c0 04 f4 71 f2 81 c7
            b8 98 2d 82 24 a4 90 ed be b3 3d 3e 3d 5c c9 3c
            47 65 70 3d 1d d7 91 64 2f 1f 11 6a 0d d8 52 be
            24 19 b2 af 72 bf e9 a0 30 e8 60 b0 28 8b 5d 77";

            //# p's CRT exponent dP:
            dp = @"
            0e 12 bf 17 18 e9 ce f5 59 9b a1 c3 88 2f e8 04
            6a 90 87 4e ef ce 8f 2c cc 20 e4 f2 74 1f b0 a3
            3a 38 48 ae c9 c9 30 5f be cb d2 d7 68 19 96 7d
            46 71 ac c6 43 1e 40 37 96 8d b3 78 78 e6 95 c1";

            //# q's CRT exponent dQ:
            dq = @"
            95 29 7b 0f 95 a2 fa 67 d0 07 07 d6 09 df d4 fc
            05 c8 9d af c2 ef 6d 6e a5 5b ec 77 1e a3 33 73
            4d 92 51 e7 90 82 ec da 86 6e fe f1 3c 45 9e 1a
            63 13 86 b7 e3 54 c8 99 f5 f1 12 ca 85 d7 15 83";

            //# CRT coefficient qInv:
            qinv = @"
            4f 45 6c 50 24 93 bd c0 ed 2a b7 56 a3 a6 ed 4d
            67 35 2a 69 7d 42 16 e9 32 12 b1 27 a6 3d 54 11
            ce 6f a9 8d 5d be fd 73 26 3e 37 28 14 27 43 81
            81 66 ed 7d d6 36 87 dd 2a 8c a1 d2 f4 fb d8 e1";
        }

        private static void SetupKeysExample2(out string n, out string e, out string d, out string p, out string q, out string dp, out string dq, out string qinv)
        {
            //# RSA modulus n:
            n = @"
            01 94 7c 7f ce 90 42 5f 47 27 9e 70 85 1f 25 d5
            e6 23 16 fe 8a 1d f1 93 71 e3 e6 28 e2 60 54 3e
            49 01 ef 60 81 f6 8c 0b 81 41 19 0d 2a e8 da ba
            7d 12 50 ec 6d b6 36 e9 44 ec 37 22 87 7c 7c 1d
            0a 67 f1 4b 16 94 c5 f0 37 94 51 a4 3e 49 a3 2d
            de 83 67 0b 73 da 91 a1 c9 9b c2 3b 43 6a 60 05
            5c 61 0f 0b af 99 c1 a0 79 56 5b 95 a3 f1 52 66
            32 d1 d4 da 60 f2 0e da 25 e6 53 c4 f0 02 76 6f
            45
            ";

            //# RSA public exponent e:
            e = @"
            01 00 01
            ";

            //# RSA private exponent d:
            d = @"
            08 23 f2 0f ad b5 da 89 08 8a 9d 00 89 3e 21 fa
            4a 1b 11 fb c9 3c 64 a3 be 0b aa ea 97 fb 3b 93
            c3 ff 71 37 04 c1 9c 96 3c 1d 10 7a ae 99 05 47
            39 f7 9e 02 e1 86 de 86 f8 7a 6d de fe a6 d8 cc
            d1 d3 c8 1a 47 bf a7 25 5b e2 06 01 a4 a4 b2 f0
            8a 16 7b 5e 27 9d 71 5b 1b 45 5b dd 7e ab 24 59
            41 d9 76 8b 9a ce fb 3c cd a5 95 2d a3 ce e7 25
            25 b4 50 16 63 a8 ee 15 c9 e9 92 d9 24 62 fe 39
            ";

            //# Prime p:
            p = @"
            01 59 db de 04 a3 3e f0 6f b6 08 b8 0b 19 0f 4d
            3e 22 bc c1 3a c8 e4 a0 81 03 3a bf a4 16 ed b0
            b3 38 aa 08 b5 73 09 ea 5a 52 40 e7 dc 6e 54 37
            8c 69 41 4c 31 d9 7d db 1f 40 6d b3 76 9c c4 1a
            43
            ";

            //# Prime q:
            q = @"
            01 2b 65 2f 30 40 3b 38 b4 09 95 fd 6f f4 1a 1a
            cc 8a da 70 37 32 36 b7 20 2d 39 b2 ee 30 cf b4
            6d b0 95 11 f6 f3 07 cc 61 cc 21 60 6c 18 a7 5b
            8a 62 f8 22 df 03 1b a0 df 0d af d5 50 6f 56 8b
            d7
            ";

            //# p's CRT exponent dP:
            dp = @"
            43 6e f5 08 de 73 65 19 c2 da 4c 58 0d 98 c8 2c
            b7 45 2a 3f b5 ef ad c3 b9 c7 78 9a 1b c6 58 4f
            79 5a dd bb d3 24 39 c7 46 86 55 2e cb 6c 2c 30
            7a 4d 3a f7 f5 39 ee c1 57 24 8c 7b 31 f1 a2 55
            ";

            //# q's CRT exponent dQ:
            dq = @"
            01 2b 15 a8 9f 3d fb 2b 39 07 3e 73 f0 2b dd 0c
            1a 7b 37 9d d4 35 f0 5c dd e2 ef f9 e4 62 94 8b
            7c ec 62 ee 90 50 d5 e0 81 6e 07 85 a8 56 b4 91
            08 dc b7 5f 36 83 87 4d 1c a6 32 9a 19 01 30 66
            ff
            ";

            //# CRT coefficient qInv:
            qinv = @"
            02 70 db 17 d5 91 4b 01 8d 76 11 8b 24 38 9a 73
            50 ec 83 6b 00 63 a2 17 21 23 6f d8 ed b6 d8 9b
            51 e7 ee b8 7b 61 1b 71 32 cb 7e a7 35 6c 23 15
            1c 1e 77 51 50 7c 78 6d 9e e1 79 41 70 a8 c8 e8
            ";
        }

        private static void SetupKeysExample3(out string n, out string e, out string d, out string p, out string q, out string dp, out string dq, out string qinv)
        {
            //# ==================================
            //# Example 3: A 1026-bit RSA Key Pair
            //# ==================================

            //# ------------------------------
            //# Components of the RSA Key Pair
            //# ------------------------------

            //# RSA modulus n:
            n = @"
02 b5 8f ec 03 9a 86 07 00 a4 d7 b6 46 2f 93 e6
cd d4 91 16 1d dd 74 f4 e8 10 b4 0e 3c 16 52 00
6a 5c 27 7b 27 74 c1 13 05 a4 cb ab 5a 78 ef a5
7e 17 a8 6d f7 a3 fa 36 fc 4b 1d 22 49 f2 2e c7
c2 dd 6a 46 32 32 ac ce a9 06 d6 6e be 80 b5 70
4b 10 72 9d a6 f8 33 23 4a bb 5e fd d4 a2 92 cb
fa d3 3b 4d 33 fa 7a 14 b8 c3 97 b5 6e 3a cd 21
20 34 28 b7 7c df a3 3a 6d a7 06 b3 d8 b0 fc 43
e9
            ";

            //# RSA public exponent e:
            e = @"
01 00 01
            ";

            //# RSA private exponent d:
            d = @"
15 b4 8a 5b 56 83 a9 46 70 e2 3b 57 18 f8 14 fa
0e 13 f8 50 38 f5 07 11 18 2c ba 61 51 05 81 f3
d2 2c 7e 23 2e f9 37 e2 2e 55 1d 68 b8 6e 2f 8c
b1 aa d8 be 2e 48 8f 5d f7 ef d2 79 e3 f5 68 d4
ea f3 6f 80 cf 71 41 ac e6 0f cc 91 13 fb 6c 4a
84 1f d5 0b bc 7c 51 2f fc be ff 21 48 7a a8 11
eb 3c a8 c6 20 05 34 6a 86 de 86 bf a1 d8 a9 48
fd 3f 34 8c 22 ea ad f3 33 c3 ce 6c e1 32 08 fd
            ";

            //# Prime p:
            p = @"
01 bf 01 d2 16 d7 35 95 cf 02 70 c2 be b7 8d 40
a0 d8 44 7d 31 da 91 9a 98 3f 7e ea 78 1b 77 d8
5f e3 71 b3 e9 37 3e 7b 69 21 7d 31 50 a0 2d 89
58 de 7f ad 9d 55 51 60 95 8b 44 54 12 7e 0e 7e
af
            ";

            //# Prime q:
            q = @"
01 8d 33 99 65 81 66 db 38 29 81 6d 7b 29 54 16
75 9e 9c 91 98 7f 5b 2d 8a ec d6 3b 04 b4 8b d7
b2 fc f2 29 bb 7f 8a 6d c8 8b a1 3d d2 e3 9a d5
5b 6d 1a 06 16 07 08 f9 70 0b e8 0b 8f d3 74 4c
e7
            ";

            //# p's CRT exponent dP:
            dp = @"
06 c0 a2 49 d2 0a 6f 2e e7 5c 88 b4 94 d5 3f 6a
ae 99 aa 42 7c 88 c2 8b 16 3a 76 94 45 e5 f3 90
cf 40 c2 74 fd 6e a6 32 9a 5c e7 c7 ce 03 a2 15
83 96 ee 2a 78 45 78 6e 09 e2 88 5a 97 28 e4 e5
            ";

            //# q's CRT exponent dQ:
            dq = @"
d1 d2 7c 29 fe dd 92 d8 6c 34 8e dd 0c cb fa c1
4f 74 6e 05 1c e1 d1 81 1d f3 5d 61 f2 ee 1c 97
d4 bf 28 04 80 2f 64 27 18 7b a8 e9 0a 8a f4 42
43 b4 07 9b 03 44 5e 60 2e 29 fa 51 93 e6 4f e9
            ";

            //# CRT coefficient qInv:
            qinv = @"
8c b2 f7 56 bd 89 41 b1 d3 b7 70 e5 ad 31 ee 37
3b 28 ac da 69 ff 9b 6f 40 fe 57 8b 9f 1a fb 85
83 6f 96 27 d3 7a cf f7 3c 27 79 e6 34 bb 26 01
1c 2c 8f 7f 33 61 ae 2a 9e a6 5e d6 89 e3 63 9a
            ";
        }

        private static void SetupKeysExample4(out string n, out string e, out string d, out string p, out string q, out string dp, out string dq, out string qinv)
        {
            //# ==================================
            //# Example 4: A 1027-bit RSA Key Pair
            //# ==================================

            //# ------------------------------
            //# Components of the RSA Key Pair
            //# ------------------------------

            //# RSA modulus n:
            n = @"
05 12 40 b6 cc 00 04 fa 48 d0 13 46 71 c0 78 c7
c8 de c3 b3 e2 f2 5b c2 56 44 67 33 9d b3 88 53
d0 6b 85 ee a5 b2 de 35 3b ff 42 ac 2e 46 bc 97
fa e6 ac 96 18 da 95 37 a5 c8 f5 53 c1 e3 57 62
59 91 d6 10 8d cd 78 85 fb 3a 25 41 3f 53 ef ca
d9 48 cb 35 cd 9b 9a e9 c1 c6 76 26 d1 13 d5 7d
de 4c 5b ea 76 bb 5b b7 de 96 c0 0d 07 37 2e 96
85 a6 d7 5c f9 d2 39 fa 14 8d 70 93 1b 5f 3f b0
39
            ";

            //# RSA public exponent e:
            e = @"
01 00 01
            ";

            //# RSA private exponent d:
            d = @"
04 11 ff ca 3b 7c a5 e9 e9 be 7f e3 8a 85 10 5e
35 38 96 db 05 c5 79 6a ec d2 a7 25 16 1e b3 65
1c 86 29 a9 b8 62 b9 04 d7 b0 c7 b3 7f 8c b5 a1
c2 b5 40 01 01 8a 00 a1 eb 2c af e4 ee 4e 94 92
c3 48 bc 2b ed ab 4b 9e bb f0 64 e8 ef f3 22 b9
00 9f 8e ec 65 39 05 f4 0d f8 8a 3c dc 49 d4 56
7f 75 62 7d 41 ac a6 24 12 9b 46 a0 b7 c6 98 e5
e6 5f 2b 7b a1 02 c7 49 a1 01 35 b6 54 0d 04 01
            ";

            //# Prime p:
            p = @"
02 74 58 c1 9e c1 63 69 19 e7 36 c9 af 25 d6 09
a5 1b 8f 56 1d 19 c6 bf 69 43 dd 1e e1 ab 8a 4a
3f 23 21 00 bd 40 b8 8d ec c6 ba 23 55 48 b6 ef
79 2a 11 c9 de 82 3d 0a 79 22 c7 09 5b 6e ba 57
01
            ";

            //# Prime q:
            q = @"
02 10 ee 9b 33 ab 61 71 6e 27 d2 51 bd 46 5f 4b
35 a1 a2 32 e2 da 00 90 1c 29 4b f2 23 50 ce 49
0d 09 9f 64 2b 53 75 61 2d b6 3b a1 f2 03 86 49
2b f0 4d 34 b3 c2 2b ce b9 09 d1 34 41 b5 3b 51
39
            ";

            //# p's CRT exponent dP:
            dp = @"
39 fa 02 8b 82 6e 88 c1 12 1b 75 0a 8b 24 2f a9
a3 5c 5b 66 bd fd 1f a6 37 d3 cc 48 a8 4a 4f 45
7a 19 4e 77 27 e4 9f 7b cc 6e 5a 5a 41 26 57 fc
47 0c 73 22 eb c3 74 16 ef 45 8c 30 7a 8c 09 01
            ";

            //# q's CRT exponent dQ:
            dq = @"
01 5d 99 a8 41 95 94 39 79 fa 9e 1b e2 c3 c1 b6
9f 43 2f 46 fd 03 e4 7d 5b ef bb bf d6 b1 d1 37
1d 83 ef b3 30 a3 e0 20 94 2b 2f ed 11 5e 5d 02
be 24 fd 92 c9 01 9d 1c ec d6 dd 4c f1 e5 4c c8
99
            ";

            //# CRT coefficient qInv:
            qinv = @"
01 f0 b7 01 51 70 b3 f5 e4 22 23 ba 30 30 1c 41
a6 d8 7c bb 70 e3 0c b7 d3 c6 7d 25 47 3d b1 f6
cb f0 3e 3f 91 26 e3 e9 79 68 27 9a 86 5b 2c 2b
42 65 24 cf c5 2a 68 3d 31 ed 30 eb 98 4b e4 12
ba
            ";
        }

        private static void SetupKeysExample5(out string n, out string e, out string d, out string p, out string q, out string dp, out string dq, out string qinv)
        {
            //# ==================================
            //# Example 5: A 1028-bit RSA Key Pair
            //# ==================================

            //# ------------------------------
            //# Components of the RSA Key Pair
            //# ------------------------------

            //# RSA modulus n:
            n = @"
0a ad f3 f9 c1 25 e5 d8 91 f3 1a c4 48 e9 93 de
fe 58 0f 80 2b 45 f9 d7 f2 2b a5 02 1e 9c 47 57
6b 5a 1e 68 03 1b a9 db 4e 6d ab e4 d9 6a 1d 6f
3d 26 72 68 cf f4 08 00 5f 11 8e fc ad b9 98 88
d1 c2 34 46 71 66 b2 a2 b8 49 a0 5a 88 9c 06 0a
c0 da 0c 5f ae 8b 55 f3 09 ba 62 e7 03 74 2f a0
32 6f 2d 10 b0 11 02 14 89 ff 49 77 70 19 0d 89
5f d3 9f 52 29 3c 39 ef d7 3a 69 8b da b9 f1 0e
d9
            ";

            //# RSA public exponent e:
            e = @"
01 00 01
            ";

            //# RSA private exponent d:
            d = @"
02 56 eb 4c ba 70 67 f2 d2 be 54 0d cd ff 45 82
a3 6b 7d 31 d1 c9 09 9b b2 14 b7 98 48 46 6a 26
8f 80 f5 8a 49 ac 04 c0 e3 64 89 34 a0 20 6c 04
53 7c 19 b2 36 64 3a 60 82 73 21 44 df 75 fa 21
75 88 f7 94 68 2b e8 91 68 27 6d c7 26 c5 c0 cb
db 84 d3 1b bf 26 d0 a4 3a f4 95 71 7f 7d 52 8a
cf ee 34 15 61 f6 ff 3c ae 05 c5 78 f8 47 0d 96
82 f9 c0 d0 72 f9 f6 06 8b 56 d5 88 0f 68 2b e2
c5
            ";

            //# Prime p:
            p = @"
03 b0 d3 96 2f 6d 17 54 9c bf ca 11 29 43 48 dc
f0 e7 e3 9f 8c 2b c6 82 4f 21 64 b6 06 d6 87 86
0d ae 1e 63 23 93 cf ed f5 13 22 82 29 06 9e 2f
60 e4 ac d7 e6 33 a4 36 06 3f 82 38 5f 48 99 37
07
            ";

            //# Prime q:
            q = @"
02 e4 c3 2e 2f 51 72 69 b7 07 23 09 f0 0c 0e 31
36 5f 7c e2 8b 23 6b 82 91 2d f2 39 ab f3 95 72
cf 0e d6 04 b0 29 82 e5 35 64 c5 2d 6a 05 39 7d
e5 c0 52 a2 fd dc 14 1e f7 18 98 36 34 6a eb 33
1f
            ";

            //# p's CRT exponent dP:
            dp = @"
01 e8 4b 11 9d 25 16 1f a6 7b 00 25 6a 5b d9 b6
45 d2 b2 32 ec b0 5b 01 51 80 02 9a 88 62 2a dc
3f 09 b3 ae ac de 61 61 ab 7c de 22 c2 ad 26 e7
79 7d f5 4e 07 2c bd 3b 26 73 80 0b 3e 43 38 db
d5
            ";

            //# q's CRT exponent dQ:
            dq = @"
eb 90 aa 1a 40 13 5b 4c ea 07 19 7c ed c8 81 9b
e1 e7 cb ff 25 47 66 21 16 f4 65 a4 a9 f4 87 ab
12 f3 ba 4f ef 13 82 22 65 a6 52 97 d9 8b 7b de
d9 37 2e 3f fe 81 a3 8b 3e 96 00 fe d0 55 75 4f
            ";

            //# CRT coefficient qInv:
            qinv = @"
01 2f 7f 81 38 f9 40 40 62 eb 85 a4 29 24 52 0b
38 f5 bb 88 6a 01 96 f4 8b b8 dc ea 60 fd 92 cc
02 7f 18 e7 81 58 a3 4a 5c 5d 5f 86 0a 0f 6c 04
07 1a 7d 01 31 2c 06 50 62 f1 eb 48 b7 9d 1c 83
cb
            ";
        }

        private static void SetupKeysExample6(out string n, out string e, out string d, out string p, out string q, out string dp, out string dq, out string qinv)
        {
            //# ==================================
            //# Example 6: A 1029-bit RSA Key Pair
            //# ==================================

            //# ------------------------------
            //# Components of the RSA Key Pair
            //# ------------------------------

            //# RSA modulus n:
            n = @"
12 b1 7f 6d ad 2e cd 19 ff 46 dc 13 f7 86 0f 09
e0 e0 cf b6 77 b3 8a 52 59 23 05 ce af 02 2c 16
6d b9 0d 04 ac 29 e3 3f 7d d1 2d 9f af 66 e0 81
6b b6 3e ad 26 7c c7 d4 6c 17 c3 7b e2 14 bc a2
a2 2d 72 3a 64 e4 44 07 43 6b 6f c9 65 72 9a ef
c2 55 4f 37 6c d5 dc ea 68 29 37 80 a6 2b f3 9d
00 29 48 5a 16 0b bb 9e 5d c0 97 2d 21 a5 04 f5
2e 5e e0 28 aa 41 63 32 f5 10 b2 e9 cf f5 f7 22
af
            ";

            //# RSA public exponent e:
            e = @"
01 00 01
            ";

            //# RSA private exponent d:
            d = @"
02 95 ec a3 56 06 18 36 95 59 ce cd 30 3a a9 cf
da fc 1d 9f 06 95 9d f7 5f fe f9 29 aa 89 69 61
bc d1 90 dc 69 97 ed a7 f5 96 3e 72 4d 07 b4 dc
11 f3 06 5e 5a e9 7d 96 83 51 12 28 0b 90 84 bb
14 f2 a2 1e bd 4e 88 9d 41 b9 c4 13 2e c1 95 6f
ca b8 bb 2f ed 05 75 88 49 36 52 2c 5f f7 d3 32
61 90 48 24 e7 ca de e4 e0 bb 37 2d 24 57 cf 78
e2 bd 12 86 22 8f f8 3f 10 73 1c e6 3c 90 cf f3
f9
            ";

            //# Prime p:
            p = @"
04 a6 ce 8b 73 58 df a6 9b dc f7 42 61 70 05 af
b5 38 5f 5f 3a 58 a2 4e f7 4a 22 a8 c0 5c b7 cc
38 eb d4 cc 9d 9a 9d 78 9a 62 cd 0f 60 f0 cb 94
1d 34 23 c9 69 2e fa 4f e3 ad ff 29 0c 47 49 a3
8b
            ";

            //# Prime q:
            q = @"
04 04 c9 a8 03 37 1f ed b4 c5 be 39 f3 c0 0b 00
9e 5e 08 a6 3b e1 e4 00 35 cd ac a5 01 1c c7 01
cf 7e eb cb 99 f0 ff e1 7c fd 0a 4b f7 be fd 2d
d5 36 ac 94 6d b7 97 fd bc 4a be 8f 29 34 9b 91
ed
            ";

            //# p's CRT exponent dP:
            dp = @"
03 96 1c 8f 76 0a a2 bd 51 54 c7 aa fd 77 22 5b
3b ac d0 13 9a e7 b5 94 8e a3 31 1f cc d8 6f b9
5c 75 af a7 67 28 4b 9b 2d e5 59 57 2f 15 d8 d0
44 c7 eb 83 a1 be 5f ad f2 cc 37 7c 0d 84 75 29
4b
            ";

            //# q's CRT exponent dQ:
            dq = @"
02 21 97 e0 66 74 21 96 aa bc 03 fa 2f ee b4 e7
0b 15 cb 78 7d 61 7a cd 31 bb 75 c7 bc 23 4a d7
06 f7 c4 8d 21 82 d1 f0 ff 9c 22 8d cf 41 96 7b
6c 0b a6 d2 c0 ad 11 0a 1b 85 78 31 ec 24 5e 2c
b1
            ";

            //# CRT coefficient qInv:
            qinv = @"
04 01 c4 c0 c5 3d 45 db db 5e 9d 96 d0 fe cf 42
75 df 09 74 bc 4a 07 36 b4 a7 4c 32 69 05 3e fb
68 6a ce 24 06 e2 2c 9e 05 8d db 4a e5 40 62 7a
e2 fd b0 82 61 e8 e7 e4 bc bc 99 4d aa fa 30 5c
45
            ";
        }

        private static void SetupKeysExample7(out string n, out string e, out string d, out string p, out string q, out string dp, out string dq, out string qinv)
        {
            //# ==================================
            //# Example 7: A 1030-bit RSA Key Pair
            //# ==================================

            //# ------------------------------
            //# Components of the RSA Key Pair
            //# ------------------------------

            //# RSA modulus n:
            n = @"
31 11 79 f0 bc fc 9b 9d 3c a3 15 d0 0e f3 0d 7b
dd 3a 2c fa e9 91 1b fe dc b9 48 b3 a4 78 2d 07
32 b6 ab 44 aa 4b f0 37 41 a6 44 dc 01 be c3 e6
9b 01 a0 33 e6 75 d8 ac d7 c4 92 5c 6b 1a ec 31
19 05 1d fd 89 76 2d 21 5d 45 47 5f fc b5 9f 90
81 48 62 3f 37 17 71 56 f6 ae 86 dd 7a 7c 5f 43
dc 1e 1f 90 82 54 05 8a 28 4a 5f 06 c0 02 17 93
a8 7f 1a c5 fe ff 7d ca ee 69 c5 e5 1a 37 89 e3
73
            ";

            //# RSA public exponent e:
            e = @"
01 00 01
            ";

            //# RSA private exponent d:
            d = @"
07 0c fc ff 2f eb 82 76 e2 74 32 c4 5d fe e4 8f
49 b7 91 7d 65 30 e1 f0 ca 34 60 f3 2e 02 76 17
44 87 c5 6e 22 a4 5d 25 00 d7 77 54 95 21 9d 7d
16 5a 9c f3 bd 92 c3 2a f9 a9 8d 8d c9 cc 29 68
00 ad c9 4a 0a 54 fb 40 f3 42 91 bf 84 ee 8e a1
2b 6f 10 93 59 c6 d3 54 2a 50 f9 c7 67 f5 cf ff
05 a6 81 c2 e6 56 fb 77 ca aa db 4b e9 46 8d 8a
bc d4 df 98 f5 8e 86 d2 05 3f a1 34 9f 74 8e 21
b1
            ";

            //# Prime p:
            p = @"
07 49 26 2c 11 1c d4 70 ec 25 66 e6 b3 73 2f c0
93 29 46 9a a1 90 71 d3 b9 c0 19 06 51 4c 6f 1d
26 ba a1 4b ea b0 97 1c 8b 7e 61 1a 4f 79 00 9d
6f ea 77 69 28 ca 25 28 5b 0d e3 64 3d 1a 3f 8c
71
            ";

            //# Prime q:
            q = @"
06 bc 1e 50 e9 6c 02 bf 63 6e 9e ea 8b 89 9b be
bf 76 51 de 77 dd 47 4c 3e 9b c2 3b ad 81 82 b6
19 04 c7 d9 7d fb eb fb 1e 00 10 88 78 b6 e6 7e
41 53 91 d6 79 42 c2 b2 bf 9b 44 35 f8 8b 0c b0
23
            ";

            //# p's CRT exponent dP:
            dp = @"
03 bc 7e a7 f0 aa b1 43 ab c6 ce 8b 97 11 86 36
a3 01 72 e4 cf e0 2c 8f a0 dd a3 b7 ba af 90 f8
09 29 82 98 55 25 f4 88 bd fc b4 bd 72 6e 22 63
9a c6 4a 30 92 ab 7f fc bf 1d 53 34 cf a5 0b 5b
f1
            ";

            //# q's CRT exponent dQ:
            dq = @"
02 62 a6 aa 29 c2 a3 c6 7d c5 34 6c 06 38 1a fd
98 7a a3 cc 93 cf bf ec f5 4f dd 9f 9d 78 7d 7f
59 a5 23 d3 98 97 9d a1 37 a2 f6 38 1f e9 48 01
f7 c9 4d a2 15 18 dc 34 cb 40 87 0c 46 97 99 4a
d9
            ";

            //# CRT coefficient qInv:
            qinv = @"
64 9d 4c 17 b6 ee 17 21 e7 72 d0 38 9a 55 9c 3d
3c df 95 50 d4 57 c4 6b 03 7b 74 64 1b 1d 52 16
6a f8 a2 13 c8 39 62 06 cd fb a4 42 2f 18 d6 f6
1d bc b5 d2 14 c9 71 bf 48 2a eb 97 6a 73 70 c2
            ";
        }

        private static void SetupKeysExample8(out string n, out string e, out string d, out string p, out string q, out string dp, out string dq, out string qinv)
        {
            //# ==================================
            //# Example 8: A 1031-bit RSA Key Pair
            //# ==================================

            //# ------------------------------
            //# Components of the RSA Key Pair
            //# ------------------------------

            //# RSA modulus n:
            n = @"
5b df 0e 30 d3 21 dd a5 14 7f 88 24 08 fa 69 19
54 80 df 8f 80 d3 f6 e8 bf 58 18 50 4f 36 42 7c
a9 b1 f5 54 0b 9c 65 a8 f6 97 4c f8 44 7a 24 4d
92 80 20 1b b4 9f cb be 63 78 d1 94 4c d2 27 e2
30 f9 6e 3d 10 f8 19 dc ef 27 6c 64 a0 0b 2a 4b
67 01 e7 d0 1d e5 fa bd e3 b1 e9 a0 df 82 f4 63
13 59 cd 22 66 96 47 fb b1 71 72 46 13 4e d7 b4
97 cf ff bd c4 2b 59 c7 3a 96 ed 90 16 62 12 df
f7
            ";

            //# RSA public exponent e:
            e = @"
01 00 01
            ";

            //# RSA private exponent d:
            d = @"
0f 7d 1e 9e 5a aa 25 fd 13 e4 a0 66 3a e1 44 e0
d1 5f 5c d1 8b cd b0 9d f2 cc 7e 64 e3 c5 e9 15
ad 62 64 53 04 16 1d 09 8c 71 5b b7 ab 8b d0 1d
07 ea f3 fe d7 c7 ed 08 af 2a 8a 62 ef 44 ab 16
b3 20 e1 4a f7 2a 48 f9 6a fe 26 2a 0a e4 cf 65
e6 35 e9 10 79 0c d4 ee 5c ea 76 8a 4b 26 39 f7
e6 f6 77 b3 f0 bb 6b e3 2b 75 74 7d 89 09 03 6f
02 64 f5 8d 40 1c db a1 31 71 61 57 a7 5e cf 63
31
            ";

            //# Prime p:
            p = @"
0a 02 ef 84 48 d9 fa d8 bb d0 d0 04 c8 c2 aa 97
51 ef 97 21 c1 b0 d0 32 36 a5 4b 0d f9 47 cb ae
d5 a2 55 ee 9e 8e 20 d4 91 ea 17 23 fe 09 47 04
a9 76 2e 88 af d1 6e bb 59 94 41 2c a9 66 dc 4f
9f
            ";

            //# Prime q:
            q = @"
09 2d 36 2e 7e d3 a0 bf d9 e9 fd 0e 6c 03 01 b6
df 29 15 9c f5 0c c8 3b 9b 0c f4 d6 ee a7 1a 61
e0 02 b4 6e 0a e9 f2 de 62 d2 5b 5d 74 52 d4 98
b8 1c 9a c6 fc 58 59 3d 4c 3f b4 f5 d7 2d fb b0
a9
            ";

            //# p's CRT exponent dP:
            dp = @"
07 c7 14 10 af 10 39 62 db 36 74 04 e3 7a e8 50
ba a4 e9 c2 9d d9 21 45 81 52 94 a6 7c 7d 1c 6d
ed 26 3a a0 30 a9 b6 33 ae 50 30 3e 14 03 5d 1a
f0 14 12 3e ba 68 78 20 30 8d 8e bc 85 b6 95 7d
7d
            ";

            //# q's CRT exponent dQ:
            dq = @"
ae 2c 75 38 0c 02 c0 16 ad 05 89 1b 33 01 de 88
1f 28 ae 11 71 18 2b 6b 2c 83 be a7 c5 15 ec a9
ca 29 8c 7b 1c ab 58 17 a5 97 06 8f c8 50 60 de
4d a8 a0 16 37 8a ae 43 c7 f9 67 bc c3 79 04 b9
            ";

            //# CRT coefficient qInv:
            qinv = @"
05 98 d1 05 9e 3a da 4f 63 20 75 2c 09 d8 05 ff
7d 1f 1a e0 d0 17 ae ee e9 ce fa 0d 7d d7 ff 77
5e 44 b5 78 32 2f 64 05 d6 21 1d a1 95 19 66 6a
a8 7f dc 4c d8 c8 8f 6b 6e 3d 67 e9 61 dc bb a3
d0
            ";
        }

        private static void SetupKeysExample9(out string n, out string e, out string d, out string p, out string q, out string dp, out string dq, out string qinv)
        {
            //# ==================================
            //# Example 9: A 1536-bit RSA Key Pair
            //# ==================================

            //# ------------------------------
            //# Components of the RSA Key Pair
            //# ------------------------------

            //# RSA modulus n:
            n = @"
cf 2c d4 1e 34 ca 3a 72 8e a5 cb 8a ff 64 c3 6d
27 bd ef 53 64 e3 36 fd 68 d3 12 3c 5a 19 6a 8c
28 70 13 e8 53 d5 15 6d 58 d1 51 95 45 20 fb 4f
6d 7b 17 ab b6 81 77 65 90 9c 57 61 19 65 9d 90
2b 19 06 ed 8a 2b 10 c1 55 c2 4d 12 45 28 da b9
ee ae 37 9b ea c6 6e 4a 41 17 86 dc b8 fd 00 62
eb c0 30 de 12 19 a0 4c 2a 8c 1b 7d d3 13 1e 4d
6b 6c ae e2 e3 1a 5e d4 1a c1 50 9b 2e f1 ee 2a
b1 83 64 be 56 8c a9 41 c2 5e cc 84 ff 9d 64 3b
5e c1 aa ae 10 2a 20 d7 3f 47 9b 78 0f d6 da 91
07 52 12 d9 ea c0 3a 06 74 d8 99 eb a2 e4 31 f4
c4 4b 61 5b 6b a2 23 2b d4 b3 3b ae d7 3d 62 5d
            ";

            //# RSA public exponent e:
            e = @"
01 00 01
            ";

            //# RSA private exponent d:
            d = @"
19 8c 14 1e 23 71 5a 92 bc cf 6a 11 9a 5b c1 13
89 46 8d 28 11 f5 48 d7 27 e1 7b 4a b0 eb 98 6d
6f 21 1e fb 53 b7 1f 7c cb ea 87 ee 69 c7 5e e6
15 00 8c 53 32 de b5 2b f3 90 ab df bf e3 7d 72
05 36 81 59 b2 63 8c 1d e3 26 e2 1d 22 25 1f 0f
b5 84 8b 3b f1 50 05 d2 a7 43 30 f0 af e9 16 ee
62 cc c1 34 4d 1d 83 a7 09 e6 06 76 27 38 40 f7
f3 77 42 4a 5e 0a 4d a7 5f 01 b3 1f f7 68 19 cf
9c bf dd 21 52 43 c3 91 7c 03 ef 38 19 93 12 e5
67 b3 bf 7a ed 3a b4 57 f3 71 ef 8a 14 23 f4 5b
68 c6 e2 82 ec 11 1b ba 28 33 b9 87 fd 69 fa d8
3b c1 b8 c6 13 c5 e1 ea 16 c1 1e d1 25 ea 7e c1
            ";

            //# Prime p:
            p = @"
fc 8d 6c 04 be c4 eb 9a 81 92 ca 79 00 cb e5 36
e2 e8 b5 19 de cf 33 b2 45 97 98 c6 90 9d f4 f1
76 db 7d 23 19 0f c7 2b 88 65 a7 18 af 89 5f 1b
cd 91 45 29 80 27 42 3b 60 5e 70 a4 7c f5 83 90
a8 c3 e8 8f c8 c4 8e 8b 32 e3 da 21 0d fb e3 e8
81 ea 56 74 b6 a3 48 c2 1e 93 f9 e5 5e a6 5e fd
            ";

            //# Prime q:
            q = @"
d2 00 d4 5e 78 8a ac ea 60 6a 40 1d 04 60 f8 7d
d5 c1 02 7e 12 dc 1a 0d 75 86 e8 93 9d 9c f7 89
b4 0f 51 ac 04 42 96 1d e7 d2 1c c2 1e 05 c8 31
55 c1 f2 aa 91 93 38 7c fd f9 56 cb 48 d1 53 ba
27 04 06 f9 bb ba 53 7d 49 87 d9 e2 f9 94 2d 7a
14 cb ff fe a7 4f ec dd a9 28 d2 3e 25 9f 5e e1
            ";

            //# p's CRT exponent dP:
            dp = @"
db 16 80 2f 79 a2 f0 d4 5f 35 8d 69 fd 33 e4 4b
81 fa e8 28 62 2e 93 a5 42 53 e9 97 d0 1b 07 43
75 9d a0 e8 12 b4 aa 4e 6c 8b ea b2 32 8d 54 31
95 5a 41 8a 67 ff 26 a8 c5 c8 07 a5 da 35 4e 05
ef 31 cc 8c f7 58 f4 63 73 29 50 b0 3e 26 57 26
fb 94 e3 9d 6a 57 2a 26 24 4a b0 8d b7 57 52 ad
            ";

            //# q's CRT exponent dQ:
            dq = @"
a0 a3 17 cf e7 df 14 23 f8 7a 6d ee 84 51 f4 e2
b4 a6 7e 54 97 f2 9b 4f 1e 4e 83 0b 9f ad d9 40
11 67 02 6f 55 96 e5 a3 9c 97 81 7e 0f 5f 16 e2
7e 19 ec 99 02 e0 1d 7e a6 fb 9a a3 c7 60 af ee
1e 38 1b 69 de 6a c9 c0 75 85 a0 6a d9 c4 ba 00
bf 75 c8 ad 2f a8 98 a4 79 e8 0a e2 94 fe d2 a1
            ";

            //# CRT coefficient qInv:
            qinv = @"
0b 21 f3 35 c3 53 34 2e b4 4c 3a a2 44 45 78 0c
2d 65 5b 94 01 74 ca e3 8c 7c 8a 4e 64 93 c0 ba
9f d3 03 74 82 67 b0 83 b9 a7 a6 cb 61 e4 2d b3
62 b8 c9 89 6d b7 06 4e 02 ad 5a e6 15 87 da 15
b4 64 9c 90 59 49 09 fe b3 7d bc b6 54 be b7 26
8e c8 01 e5 a8 b4 aa 39 11 be bd 88 54 2f 05 be
            ";
        }

        private static void SetupKeysExample10(out string n, out string e, out string d, out string p, out string q, out string dp, out string dq, out string qinv)
        {
            //# ===================================
            //# Example 10: A 2048-bit RSA Key Pair
            //# ===================================

            //# ------------------------------
            //# Components of the RSA Key Pair
            //# ------------------------------

            //# RSA modulus n:
            n = @"
ae 45 ed 56 01 ce c6 b8 cc 05 f8 03 93 5c 67 4d
db e0 d7 5c 4c 09 fd 79 51 fc 6b 0c ae c3 13 a8
df 39 97 0c 51 8b ff ba 5e d6 8f 3f 0d 7f 22 a4
02 9d 41 3f 1a e0 7e 4e be 9e 41 77 ce 23 e7 f5
40 4b 56 9e 4e e1 bd cf 3c 1f b0 3e f1 13 80 2d
4f 85 5e b9 b5 13 4b 5a 7c 80 85 ad ca e6 fa 2f
a1 41 7e c3 76 3b e1 71 b0 c6 2b 76 0e de 23 c1
2a d9 2b 98 08 84 c6 41 f5 a8 fa c2 6b da d4 a0
33 81 a2 2f e1 b7 54 88 50 94 c8 25 06 d4 01 9a
53 5a 28 6a fe b2 71 bb 9b a5 92 de 18 dc f6 00
c2 ae ea e5 6e 02 f7 cf 79 fc 14 cf 3b dc 7c d8
4f eb bb f9 50 ca 90 30 4b 22 19 a7 aa 06 3a ef
a2 c3 c1 98 0e 56 0c d6 4a fe 77 95 85 b6 10 76
57 b9 57 85 7e fd e6 01 09 88 ab 7d e4 17 fc 88
d8 f3 84 c4 e6 e7 2c 3f 94 3e 0c 31 c0 c4 a5 cc
36 f8 79 d8 a3 ac 9d 7d 59 86 0e aa da 6b 83 bb
            ";

            //# RSA public exponent e:
            e = @"
01 00 01
            ";

            //# RSA private exponent d:
            d = @"
05 6b 04 21 6f e5 f3 54 ac 77 25 0a 4b 6b 0c 85
25 a8 5c 59 b0 bd 80 c5 64 50 a2 2d 5f 43 8e 59
6a 33 3a a8 75 e2 91 dd 43 f4 8c b8 8b 9d 5f c0
d4 99 f9 fc d1 c3 97 f9 af c0 70 cd 9e 39 8c 8d
19 e6 1d b7 c7 41 0a 6b 26 75 df bf 5d 34 5b 80
4d 20 1a dd 50 2d 5c e2 df cb 09 1c e9 99 7b be
be 57 30 6f 38 3e 4d 58 81 03 f0 36 f7 e8 5d 19
34 d1 52 a3 23 e4 a8 db 45 1d 6f 4a 5b 1b 0f 10
2c c1 50 e0 2f ee e2 b8 8d ea 4a d4 c1 ba cc b2
4d 84 07 2d 14 e1 d2 4a 67 71 f7 40 8e e3 05 64
fb 86 d4 39 3a 34 bc f0 b7 88 50 1d 19 33 03 f1
3a 22 84 b0 01 f0 f6 49 ea f7 93 28 d4 ac 5c 43
0a b4 41 49 20 a9 46 0e d1 b7 bc 40 ec 65 3e 87
6d 09 ab c5 09 ae 45 b5 25 19 01 16 a0 c2 61 01
84 82 98 50 9c 1c 3b f3 a4 83 e7 27 40 54 e1 5e
97 07 50 36 e9 89 f6 09 32 80 7b 52 57 75 1e 79
            ";

            //# Prime p:
            p = @"
ec f5 ae cd 1e 55 15 ff fa cb d7 5a 28 16 c6 eb
f4 90 18 cd fb 46 38 e1 85 d6 6a 73 96 b6 f8 09
0f 80 18 c7 fd 95 cc 34 b8 57 dc 17 f0 cc 65 16
bb 13 46 ab 4d 58 2c ad ad 7b 41 03 35 23 87 b7
03 38 d0 84 04 7c 9d 95 39 b6 49 62 04 b3 dd 6e
a4 42 49 92 07 be c0 1f 96 42 87 ff 63 36 c3 98
46 58 33 68 46 f5 6e 46 86 18 81 c1 02 33 d2 17
6b f1 5a 5e 96 dd c7 80 bc 86 8a a7 7d 3c e7 69
            ";

            //# Prime q:
            q = @"
bc 46 c4 64 fc 6a c4 ca 78 3b 0e b0 8a 3c 84 1b
77 2f 7e 9b 2f 28 ba bd 58 8a e8 85 e1 a0 c6 1e
48 58 a0 fb 25 ac 29 99 90 f3 5b e8 51 64 c2 59
ba 11 75 cd d7 19 27 07 13 51 84 99 2b 6c 29 b7
46 dd 0d 2c ab e1 42 83 5f 7d 14 8c c1 61 52 4b
4a 09 94 6d 48 b8 28 47 3f 1c e7 6b 6c b6 88 6c
34 5c 03 e0 5f 41 d5 1b 5c 3a 90 a3 f2 40 73 c7
d7 4a 4f e2 5d 9c f2 1c 75 96 0f 3f c3 86 31 83
            ";

            //# p's CRT exponent dP:
            dp = @"
c7 35 64 57 1d 00 fb 15 d0 8a 3d e9 95 7a 50 91
5d 71 26 e9 44 2d ac f4 2b c8 2e 86 2e 56 73 ff
6a 00 8e d4 d2 e3 74 61 7d f8 9f 17 a1 60 b4 3b
7f da 9c b6 b6 b7 42 18 60 98 15 f7 d4 5c a2 63
c1 59 aa 32 d2 72 d1 27 fa f4 bc 8c a2 d7 73 78
e8 ae b1 9b 0a d7 da 3c b3 de 0a e7 31 49 80 f6
2b 6d 4b 0a 87 5d 1d f0 3c 1b ae 39 cc d8 33 ef
6c d7 e2 d9 52 8b f0 84 d1 f9 69 e7 94 e9 f6 c1
            ";

            //# q's CRT exponent dQ:
            dq = @"
26 58 b3 7f 6d f9 c1 03 0b e1 db 68 11 7f a9 d8
7e 39 ea 2b 69 3b 7e 6d 3a 2f 70 94 74 13 ee c6
14 2e 18 fb 8d fc b6 ac 54 5d 7c 86 a0 ad 48 f8
45 71 70 f0 ef b2 6b c4 81 26 c5 3e fd 1d 16 92
01 98 dc 2a 11 07 dc 28 2d b6 a8 0c d3 06 23 60
ba 3f a1 3f 70 e4 31 2f f1 a6 cd 6b 8f c4 cd 9c
5c 3d b1 7c 6d 6a 57 21 2f 73 ae 29 f6 19 32 7b
ad 59 b1 53 85 85 85 ba 4e 28 b6 0a 62 a4 5e 49
            ";

            //# CRT coefficient qInv:
            qinv = @"
6f 38 52 6b 39 25 08 55 34 ef 3e 41 5a 83 6e de
8b 86 15 8a 2c 7c bf ec cb 0b d8 34 30 4f ec 68
3b a8 d4 f4 79 c4 33 d4 34 16 e6 32 69 62 3c ea
10 07 76 d8 5a ff 40 1d 3f ff 61 0e e6 54 11 ce
3b 13 63 d6 3a 97 09 ee de 42 64 7c ea 56 14 93
d5 45 70 a8 79 c1 86 82 cd 97 71 0b 96 20 5e c3
11 17 d7 3b 5f 36 22 3f ad d6 e8 ba 90 dd 7c 0e
e6 1d 44 e1 63 25 1e 20 c7 f6 6e b3 05 11 7c b8
            ";
        }

        private static void RunOneCase(string testcase, string n, string e, string d, string p, string q, string dp, string dq, string qinv, string message, string seed, string encryption)
        {
            TypeMap.Register.Singleton<IRandomGenerator>(() => new InjectedBytesRandomGenerator(seed.FromHex()));

            IAsymmetricKeyPair keyPair = New<IAsymmetricFactory>().CreateKeyPair(PositiveFromHex(n), PositiveFromHex(e), PositiveFromHex(d), PositiveFromHex(p), PositiveFromHex(q), PositiveFromHex(dp), PositiveFromHex(dq), PositiveFromHex(qinv));

            byte[] cipher = keyPair.PublicKey.Transform(message.FromHex());
            Assert.That(cipher, Is.EquivalentTo(encryption.FromHex()), testcase);

            byte[] deciphered = keyPair.PrivateKey.Transform(cipher);
            Assert.That(deciphered, Is.EquivalentTo(message.FromHex()));
        }
    }
}