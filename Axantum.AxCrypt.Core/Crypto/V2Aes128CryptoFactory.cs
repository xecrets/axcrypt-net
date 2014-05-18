﻿using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class V2Aes128CryptoFactory : ICryptoFactory
    {
        private static readonly Guid _id = CryptoFactory.Aes128Id;

        public IDerivedKey CreatePassphrase(Passphrase passphrase)
        {
            return new V2Passphrase(passphrase, 128, Id);
        }

        public IDerivedKey CreatePassphrase(Passphrase passphrase, Salt salt, int derivationIterations)
        {
            return new V2Passphrase(passphrase, salt, derivationIterations, 128, Id);
        }

        public ICrypto CreateCrypto(SymmetricKey key, SymmetricIV iv, long keyStreamOffset)
        {
            return new V2AesCrypto(this, key, iv, keyStreamOffset);
        }

        public int Priority
        {
            get { return 200000; }
        }

        public Guid Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return "AES-128"; }
        }
    }
}