﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AxCrypt.Abstractions;
using AxCrypt.Core;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.IO;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Sdk
{
    public class AxSdkStreamEncryptor
    {
        private List<UserPublicKey> _publicKeys = new List<UserPublicKey>();

        private Passphrase _passphrase = Passphrase.Empty;

        private Guid _cryptoId;

        private AxCryptOptions _options;

        public AxSdkStreamEncryptor(AxSdkConfiguration configuration)
        {
            _cryptoId = configuration.CryptoId;
            _options = configuration.Copmress ? AxCryptOptions.EncryptWithCompression : AxCryptOptions.EncryptWithoutCompression;
            _options |= AxCryptOptions.SetFileTimes;
        }

        public AxSdkStreamEncryptor SetPassphrase(string passphrase)
        {
            _passphrase = new Passphrase(passphrase);

            return this;
        }

        public AxSdkStreamEncryptor AddPublicKey(string email, string pem)
        {
            UserPublicKey publicKey = new UserPublicKey(EmailAddress.Parse(email), New<IAsymmetricFactory>().CreatePublicKey(pem));
            _publicKeys.Add(publicKey);

            return this;
        }

        public async Task EncryptAsync(Stream clearIn, Stream encryptedOut, string fileName)
        {
            EncryptionParameters parameters = new EncryptionParameters(_cryptoId, _passphrase);
            await parameters.AddAsync(_publicKeys);

            EncryptedProperties properties = new EncryptedProperties(fileName);

            AxCryptFile.Encrypt(clearIn, encryptedOut, properties, parameters, _options, new ProgressContext());
        }

        public void Encrypt(Stream clearIn, string fileName, Action<Stream> processAction)
        {
            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                using (PipelineStream pipeline = new PipelineStream(tokenSource.Token))
                {
                    Task encryption = Task.Run(async () =>
                    {
                        await EncryptAsync(clearIn, pipeline, fileName);
                        pipeline.Complete();
                    }).ContinueWith((t) => { if (t.IsFaulted) tokenSource.Cancel(); }, tokenSource.Token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

                    Task process = Task.Run(() =>
                    {
                        processAction(pipeline);
                    }).ContinueWith((t) => { if (t.IsFaulted) tokenSource.Cancel(); }, tokenSource.Token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

                    try
                    {
                        Task.WaitAll(encryption, process);
                    }
                    catch (AggregateException ae)
                    {
                        IEnumerable<Exception> exceptions = ae.InnerExceptions.Where(ex1 => ex1.GetType() != typeof(OperationCanceledException));
                        if (!exceptions.Any())
                        {
                            return;
                        }

                        IEnumerable<Exception> axCryptExceptions = exceptions.Where(ex2 => ex2 is AxCryptException);
                        if (axCryptExceptions.Any())
                        {
                            ExceptionDispatchInfo.Capture(axCryptExceptions.First()).Throw();
                        }

                        throw exceptions.First();
                    }
                }
            }
        }
    }
}