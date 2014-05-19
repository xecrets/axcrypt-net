﻿using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2AES256RegressionCompleteFiles
    {
        private static readonly string _rootPath = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestSimpleSmallFile()
        {
            TestOneFile("short-txt-AES256.axx", "PâsswördètMëd§½ Lôñg|´¨", "2de4823aa40ed2a6d040e7ba67bf60e3b1ae5c1f1bc2391ba8435ec7d1597f49");
        }

        [Test]
        public static void TestLargerUncompressibleFile()
        {
            TestOneFile("snow-jpg-AES256.axx", "PâsswördètMëd§½ Lôñg|´¨", "b541684642894f9385b15ddd62f980e20a730fc036bcb1bbb4bad75b1f4889b4");
        }

        [Test]
        public static void TestLargerCompressibleTextFile()
        {
            TestOneFile("Frankenstein-txt-AES256.axx", "PâsswördètMëd§½ Lôñg|´¨", "3493994a1a7d891e1a6fb4e3f60c58cbfb3e6f71f12f4c3ffe51c0c9498eb520");
        }

        private static void TestOneFile(string resourceName, string password, string sha256HashValue)
        {
            string source = Path.Combine(_rootPath, "source.axx");
            string destination = Path.Combine(_rootPath, "destination.file");
            Stream stream = Assembly.GetAssembly(typeof(TestV2AES256RegressionCompleteFiles)).GetManifestResourceStream("Axantum.AxCrypt.Core.Test.resources." + resourceName);
            FakeRuntimeFileInfo.AddFile(source, FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.TestDate2Utc, FakeRuntimeFileInfo.TestDate3Utc, stream);

            Passphrase passphrase = new Passphrase(password);

            bool ok = new AxCryptFile().Decrypt(Factory.New<IRuntimeFileInfo>(source), Factory.New<IRuntimeFileInfo>(destination), passphrase, AxCryptOptions.SetFileTimes, new ProgressContext());
            Assert.That(ok, Is.True, "The Decrypt() method should return true for ok.");

            byte[] hash;
            using (Stream plainStream = Factory.New<IRuntimeFileInfo>(destination).OpenRead())
            {
                HashAlgorithm hashAlgorithm = SHA256.Create();
                using (Stream cryptoStream = new CryptoStream(plainStream, hashAlgorithm, CryptoStreamMode.Read))
                {
                    cryptoStream.CopyTo(Stream.Null);
                }
                hash = hashAlgorithm.Hash;
            }

            Assert.That(hash.IsEquivalentTo(sha256HashValue.FromHex()), "Wrong SHA-256.");
        }
    }
}