﻿using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Mono;
using Axantum.AxCrypt.Mono.Portable;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Api.Test
{
    [TestFixture]
    public class TestAxCryptApiClient
    {
        [SetUp]
        public void Setup()
        {
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new BouncyCastleAsymmetricFactory());
            TypeMap.Register.Singleton<IRandomGenerator>(() => new RandomGenerator());
            TypeMap.Register.New<IStringSerializer>(() => new StringSerializer(new BouncyCastleAsymmetricFactory().GetConverters()));
            TypeMap.Register.Singleton<IEmailParser>(() => new EmailParser());
            TypeMap.Register.New<Sha256>(() => PortableFactory.SHA256Managed());
            TypeMap.Register.New<RandomNumberGenerator>(() => PortableFactory.RandomNumberGenerator());
            RuntimeEnvironment.RegisterTypeFactories();
        }

        [TearDown]
        public void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public void TestSimpleSummary()
        {
            LogOnIdentity identity = new LogOnIdentity(new UserAsymmetricKeys(EmailAddress.Parse("svante@axcrypt.net"), 512), new Passphrase("a"));

            UserSummary value = new UserSummary(identity.UserEmail.Address, "Free", new string[] { identity.UserKeys.KeyPair.PublicKey.Thumbprint.ToString() });
            string content = Resolve.Serializer.Serialize(value);

            WebCallerResponse response = new WebCallerResponse(HttpStatusCode.OK, content);

            Mock<IWebCaller> mockWebCaller = new Mock<IWebCaller>();
            mockWebCaller.Setup<WebCallerResponse>(wc => wc.Send(It.Is<LogOnIdentity>((i) => i.UserEmail == identity.UserEmail), It.Is<WebCallerRequest>((r) => r.Url == new Uri("http://localhost/api/summary")))).Returns(() => new WebCallerResponse(HttpStatusCode.OK, content));
            TypeMap.Register.New<IWebCaller>(() => mockWebCaller.Object);

            AxCryptApiClient client = new AxCryptApiClient(identity, new Uri("http://localhost"));
            UserSummary userSummary = client.User();

            Assert.That(userSummary.UserName, Is.EqualTo(identity.UserEmail.Address));
            Assert.That(userSummary.PublicKeyThumbprints.Count(), Is.EqualTo(1));
            Assert.That(userSummary.PublicKeyThumbprints.First(), Is.EqualTo(identity.UserKeys.KeyPair.PublicKey.Thumbprint.ToString()));
        }
    }
}