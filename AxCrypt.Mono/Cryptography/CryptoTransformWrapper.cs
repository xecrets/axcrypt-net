﻿using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Algorithm;
using AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Mono.Cryptography
{
    public class CryptoTransformWrapper : ICryptoTransform
    {
        private System.Security.Cryptography.ICryptoTransform _cryptoTransform;

        public CryptoTransformWrapper(System.Security.Cryptography.ICryptoTransform cryptoTransform)
        {
            _cryptoTransform = cryptoTransform;
        }

        public bool CanReuseTransform
        {
            get { return _cryptoTransform.CanReuseTransform; }
        }

        public bool CanTransformMultipleBlocks
        {
            get { return _cryptoTransform.CanTransformMultipleBlocks; }
        }

        public int InputBlockSize
        {
            get { return _cryptoTransform.InputBlockSize; }
        }

        public int OutputBlockSize
        {
            get { return _cryptoTransform.OutputBlockSize; }
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return _cryptoTransform.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            try
            {
                return _cryptoTransform.TransformFinalBlock(inputBuffer, inputOffset, inputCount);
            }
            catch (System.Security.Cryptography.CryptographicException ce)
            {
                throw new CryptoException("Error in cryptographic transformation.", ErrorStatus.CryptographicError, ce);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cryptoTransform.Dispose();
            }
        }
    }
}