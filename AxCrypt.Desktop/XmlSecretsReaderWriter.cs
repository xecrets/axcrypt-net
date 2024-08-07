#if DEBUG
#define CODE_ANALYSIS
#endif

#region License

/*
 *  AxCrypt.Xecrets.Core - Xecrets Core and Reference Implementation
 *
 *  Copyright (C) 2008 Svante Seleborg
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  If you'd like to license this program under any other terms than the
 *  above, please contact the author and copyright holder.
 *
 *  Contact: mailto:svante@axantum.com
 */

#endregion License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.Secrets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Desktop
{
    public class XmlSecretsReaderWriter : ISecretsReader, ISecretsWriter, ISecretsDBConverter
    {
        #region Private classes

        private class InternalSecret : Secret
        {
            public InternalSecret(Secret secret)
                : base(secret)
            {
            }

            private DateTime _lastUpdateUtc;

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "The getter is currently unused but should still be there.")]
            public DateTime LastUpdateUtc
            {
                get { return _lastUpdateUtc; }
                set { _lastUpdateUtc = value; }
            }
        }

        private class InternalEncryptionKey : EncryptionKey
        {
            public InternalEncryptionKey(EncryptionKey key)
                : base(key)
            {
            }

            public new string DecryptPassphrase()
            {
                return base.DecryptPassphrase();
            }
        }

        private static class SecretXMLElementNames
        {
            public const string Secrets = "Secrets";

            public const string Secret = "Secret";

            public const string Password = "Password";

            public const string Card = "Card";

            public const string Note = "Note";

            public const string Title = "Title";

            public const string PasswordUrl = "Url";

            public const string Description = "Description";

            public const string PasswordUsername = "Username";

            public const string TheSecret = "TheSecret";

            public const string CardNumber = "Number";

            public const string NameOnCard = "NameOnCard";

            public const string SecurityCode = "SecurityCode";

            public const string ExpirationDate = "ExpirationDate";

            public const string CreationDate = "CreationDate";

            public const string LastUpdateUtc = "LastUpdateUtc";

            public const string DeletedUtc = "DeletedUtc";
        }

        #endregion Private classes

        #region Properties etc

        private const string KEYNAME = "XecretsKey";

        private static int _defaultRfc2898iterations = CalculateDefaultRfc2989Iterations();

        private DateTime _lastUpdateUtc = DateTime.MinValue;

        public DateTime LastUpdateUtc
        {
            get { return _lastUpdateUtc; }
            set { _lastUpdateUtc = value; }
        }

        /// <summary>
        /// The current representation of persisted data.
        /// </summary>
        private byte[] _data;

        private XmlDocument _xecretsDocument;

        /// <summary>
        /// The sessions node of the Xecrets document. The parent is the document. This is where
        /// we keep the working copy of the actual data, either when it's new or after parsing
        /// the provided data byte stream.
        /// </summary>
        private XmlDocument XecretsDocument
        {
            get
            {
                if (_xecretsDocument == null)
                {
                    if (_data != null)
                    {
                        _xecretsDocument = GetDocumentFromData(_data);
                    }
                    else
                    {
                        _xecretsDocument = CreateNewDocument();
                    }
                }
                return _xecretsDocument;
            }
        }

        private string _uniqueName;

        /// <summary>
        /// Gets or sets a unique name for this instance, typically a user name.
        /// </summary>
        /// <value>The unique name</value>
        /// <remarks>
        /// This may be used as the caching key to enable caching.
        /// </remarks>
        public string UniqueName
        {
            get { return _uniqueName; }
            protected set { _uniqueName = value; }
        }

        #endregion Properties etc

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSecretsReaderWriter"/> class.
        /// </summary>
        /// <param name="uniquename">
        /// A globally, persistent, unique name for the secrets this instance works with. Typically a user name.
        /// </param>
        public XmlSecretsReaderWriter(string uniqueName)
        {
            UniqueName = uniqueName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSecretsReaderWriter"/> class.
        /// </summary>
        /// <param name="uniquename">
        /// A globally, persistent, unique name for the secrets this instance works with. Typically a user name.
        /// </param>
        /// <param name="data">The data to initally read from. Do NOT modify after call!</param>
        public XmlSecretsReaderWriter(string uniqueName, byte[] data)
            : this(uniqueName)
        {
            _data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSecretsReaderWriter"/> class.
        /// </summary>
        /// <param name="uniquename">
        /// A globally, persistent, unique name for the secrets this instance works with. Typically a user name.
        /// </param>
        /// <param name="stream">The stream to intially read from, or null for a non-existing 'file'</param>
        public XmlSecretsReaderWriter(string uniqueName, Stream stream)
            : this(uniqueName)
        {
            if (stream == null)
            {
                return;
            }
            byte[] data = new byte[stream.Length - stream.Position];
            int bytesRead = stream.Read(data, 0, data.Length);
            Trace.Assert(bytesRead == data.Length, "bytesRead == data.Length");

            _data = data;
        }

        #endregion Constructors

        #region ISecretsReader Members

        public FormatConfidence DetermineFormatConfidence(EncryptionKeyCollection keyCollection)
        {
            if (_data == null)
            {
                throw new InvalidOperationException("No data read to check format for");
            }

            // Here try to extract from the data using the keys. If this fails (exception) it's not our format.
            // Even if we get zero secrets, it might not be our format, it could be a later format.
            try
            {
                // Since it's a lazy load, the property reference here must be inside the try
                if (XecretsDocument == null)
                {
                    return FormatConfidence.DefinitelyNot;
                }

                // Get what we can using the keyCollection
                IList<Secret> secrets = FindSecrets(keyCollection);
                if (secrets.Count > 0)
                {
                    // If we got any secrest, we're definitely on the right track
                    return FormatConfidence.Definitely;
                }
                // If we just got zero, we're probably the right one.
                return FormatConfidence.Probably;
            }
            catch (FormatException)
            {
                // Something went wrong...
                return FormatConfidence.DefinitelyNot;
            }
        }

        public SecretCollection FindSecrets(IEnumerable<EncryptionKey> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            // Get a copy of the collection so we can remove keys as we use them.
            EncryptionKeyCollection unusedKeys = new EncryptionKeyCollection("");
            unusedKeys.AddRange(keys);

            // Accumulate decrypted secrets here.
            SecretCollection secrets = new SecretCollection();

            XmlNodeList nodeList = GetXecretsSession(XecretsDocument).SelectNodes("XecretsSession");
            foreach (XmlNode node in nodeList)
            {
                SecretCollection decryptedSessionSecrets = AttemptDecryptXecretsSessionElement(unusedKeys, (XmlElement)node);
                secrets.AddRange(decryptedSessionSecrets);
            }

            secrets.OriginalCount = secrets.Count;
            return secrets;
        }

        public bool HasMoreSecrets(IEnumerable<EncryptionKey> keys)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #endregion ISecretsReader Members

        #region ISecretsWriter Members

        public System.IO.Stream OpenDataStream()
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public void SaveSecrets(IEnumerable<Secret> secrets, Stream stream)
        {
            XmlDocument document = CreateDocumentFromSecrets(secrets);

            // Explicitly set the way the XML is written.
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.OmitXmlDeclaration = false;
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = "  ";
            xmlWriterSettings.CloseOutput = false;

            using (XmlWriter writer = XmlWriter.Create(stream, xmlWriterSettings))
            {
                document.Save(writer);
            }
        }

        #endregion ISecretsWriter Members

        #region ISecretsDBConverter Members

        /*
         *  Convert DB XML Secret strings to Secrets and Secrets Collection to XML Secret strings
         */

        public System.Text.StringBuilder SecretsXMLDoc(IEnumerable<Secret> secrets)
        {
            XmlDocument document = CreateDocumentFromSecrets(secrets);

            // Explicitly set the way the XML is written.
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.OmitXmlDeclaration = false;
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = "  ";
            xmlWriterSettings.CloseOutput = false;
            System.Text.StringBuilder xmlDocumentStr = new StringBuilder();

            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter xmlTextWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
                {
                    document.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    xmlDocumentStr.Append(stringWriter.GetStringBuilder().ToString());
                }

                stringWriter.Flush();
            }

            return xmlDocumentStr;
        }

        public SecretCollection SecretsFromXMLDocList(IEnumerable<Api.Model.Secret.SecretApiModel> secretsList, IEnumerable<EncryptionKey> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            // Accumulate decrypted secrets here.
            SecretCollection secrets = new SecretCollection();

            foreach (Api.Model.Secret.SecretApiModel secret in secretsList)
            {
                // Get a copy of the collection so we can remove keys as we use them.
                EncryptionKeyCollection unusedKeys = new EncryptionKeyCollection("");
                unusedKeys.AddRange(keys);

                XmlDocument SecretXMLDocument = GetDocumentFromString(secret.SecretBody);

                XmlNodeList nodeList = GetXecretsSession(SecretXMLDocument).SelectNodes("XecretsSession");
                foreach (XmlNode node in nodeList)
                {
                    SecretCollection decryptedSessionSecrets = AttemptDecryptXecretsSessionElement(unusedKeys, (XmlElement)node);
                    if (decryptedSessionSecrets.Count > 0)
                    {
                        decryptedSessionSecrets[0].DBId = secret.Id;
                        decryptedSessionSecrets[0].CreatedUtc = secret.CreatedUtc;
                        decryptedSessionSecrets[0].UpdatedUtc = secret.UpdatedUtc;
                        decryptedSessionSecrets[0].DeletedUtc = secret.DeletedUtc ?? DateTime.MinValue;
                    }
                    secrets.AddRange(decryptedSessionSecrets);
                }
            }
            secrets.OriginalCount = secrets.Count;
            return secrets;
        }

        private static XmlDocument GetDocumentFromString(string secretXmlStr)
        {
            // Setup the reader settings
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.CloseInput = false;
            xmlReaderSettings.ConformanceLevel = ConformanceLevel.Document;
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.ValidationType = ValidationType.None;

            // Actually load the document from the string
            StringReader stringReader = new StringReader(secretXmlStr);
            XmlReader reader = XmlReader.Create(stringReader, xmlReaderSettings);

            XmlDocument document = CreateNewDocument();
            try
            {
                document.Load(reader);
            }
            catch (XmlException)
            {
                // We turn this into a FormatException, since that is the 'controlled' exception that may
                // happen when we try a data stream for conformance.
                throw new FormatException($"{nameof(GetDocumentFromString)} - Error loading XML - not the right format");
            }

            return document;
        }

        public SecretCollection SecretsFromXMLDoc(Api.Model.Secret.SecretsApiModel secretModel, IEnumerable<EncryptionKey> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            if(secretModel.SecretBody == null) 
            { 
                return new SecretCollection();
            }

            // Accumulate decrypted secrets here.
            SecretCollection secrets = new SecretCollection();

            // Get a copy of the collection so we can remove keys as we use them.
            EncryptionKeyCollection unusedKeys = new EncryptionKeyCollection("");
            unusedKeys.AddRange(keys);

            XmlDocument SecretXMLDocument = GetDocumentFromString(secretModel.SecretBody);

            XmlNodeList nodeList = GetXecretsSession(SecretXMLDocument).SelectNodes("XecretsSession");
            foreach (XmlNode node in nodeList)
            {
                SecretCollection decryptedSessionSecrets = AttemptDecryptXecretsSessionElement(unusedKeys, (XmlElement)node);
                if (decryptedSessionSecrets.Count > 0)
                {
                    decryptedSessionSecrets[0].DBId = secretModel.Id;
                    decryptedSessionSecrets[0].CreatedUtc = secretModel.CreatedUtc;
                    decryptedSessionSecrets[0].UpdatedUtc = secretModel.UpdatedUtc;
                    decryptedSessionSecrets[0].DeletedUtc = secretModel.DeletedUtc ?? DateTime.MinValue;
                }
                secrets.AddRange(decryptedSessionSecrets);
            }

            secrets.OriginalCount = secrets.Count;
            return secrets;
        }

        #endregion ISecretsDBConverter Members

        #region Private Helpers

        private SecretCollection AttemptDecryptXecretsSessionElement(EncryptionKeyCollection keyCollection, XmlElement xecretsSessionElement)
        {
            foreach (EncryptionKey key in keyCollection)
            {
                DateTime lastUpdateUtc = DateTime.Parse(xecretsSessionElement.Attributes["LastUpdateUtc"].Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                if (lastUpdateUtc > LastUpdateUtc)
                {
                    LastUpdateUtc = lastUpdateUtc;
                }

                XmlDocument decryptedXml = DecryptEncryptedData(new InternalEncryptionKey(key), xecretsSessionElement);
                if (decryptedXml == null)
                {
                    continue;
                }

                SecretCollection decryptedSecrets = GetSessionSecrets(decryptedXml.SelectNodes("Secrets/Secret"), key);

                // Set the last updated for each and every secret - we don't actually store this per secret, only per
                // session, but that's not a fact we expose externally.
                foreach (Secret secret in decryptedSecrets)
                {
                    InternalSecret internalSecret = secret as InternalSecret;
                    internalSecret.LastUpdateUtc = lastUpdateUtc;
                }

                keyCollection.Remove(key);
                return decryptedSecrets;
            }
            return new SecretCollection();
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EncryptedData")]
        private static XmlDocument DecryptEncryptedData(InternalEncryptionKey internalKey, XmlElement sessionElement)
        {
            XmlElement secretsElement = sessionElement.FirstChild as XmlElement;
            if (secretsElement == null || secretsElement.Name != "EncryptedData")
            {
                throw new FormatException($"There must be an element named {nameof(EncryptedData)} here.");
            }

            SymmetricAlgorithm masterKey = GetAndDeriveMasterKey(internalKey.DecryptPassphrase(), sessionElement);

            // Decrypt the node, creating a named session key to use the embedded session key.
            EncryptedXml encryptedXml = new EncryptedXml();
            encryptedXml.AddKeyNameMapping(KEYNAME, masterKey);

            EncryptedData encryptedData = new EncryptedData();
            encryptedData.LoadXml(secretsElement);

            SymmetricAlgorithm decryptionKey;
            try
            {
                decryptionKey = encryptedXml.GetDecryptionKey(encryptedData, null);
            }
            catch (CryptographicException)
            {
                // This ok - we're not guaranteed that we actually provided the correct key.
                return null;
            }

            // It seems mono doesn't throw, but returns a null key instead.
            if (decryptionKey == null)
            {
                return null;
            }

            byte[] plainTextXml = encryptedXml.DecryptData(encryptedData, decryptionKey);
            string plain = encryptedXml.Encoding.GetString(plainTextXml);

            XmlDocument decryptedSecretsDocument = new XmlDocument();
            decryptedSecretsDocument.LoadXml(plain);

            return decryptedSecretsDocument;
        }

        private static XmlDocument GetDocumentFromData(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                return GetDocumentFromStream(stream);
            }
        }

        private static XmlDocument GetDocumentFromStream(Stream stream)
        {
            // Setup the reader settings
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.CloseInput = false;
            xmlReaderSettings.ConformanceLevel = ConformanceLevel.Document;
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.ValidationType = ValidationType.None;

            // Actually load the document from the stream
            XmlReader reader = XmlReader.Create(stream, xmlReaderSettings);

            XmlDocument document = CreateNewDocument();
            try
            {
                document.Load(reader);
            }
            catch (XmlException)
            {
                // We turn this into a FormatException, since that is the 'controlled' exception that may
                // happen when we try a data stream for conformance.
                throw new FormatException("Error loading XML - not the right format");
            }

            return document;
        }

        private static XmlDocument CreateNewDocument()
        {
            // We construct an empty XML document, with just a sessions element.
            XmlDocument document = new XmlDocument();
            XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, null);
            document.AppendChild(declaration);

            XmlElement xecretsSessions = document.CreateElement("XecretsSessions");

            XmlElement axantumXecretsElement = document.CreateElement("AxantumXecrets");
            axantumXecretsElement.AppendChild(xecretsSessions);

            document.AppendChild(axantumXecretsElement);
            return document;
        }

        private static XmlElement GetXecretsSession(XmlDocument document)
        {
            return document.SelectSingleNode("AxantumXecrets/XecretsSessions") as XmlElement;
        }

        private XmlDocument CreateDocumentFromSecrets(IEnumerable<Secret> secrets)
        {
            XmlDocument document = CreateNewDocument();

            if (secrets == null)
            {
                return document;
            }

            // Check if there are any elements in the collection, and also get hold of the first element in the collection
            IEnumerator<Secret> enumerator = secrets.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                if (New<ILogging>().IsWarningEnabled)
                {
                    New<ILogging>().LogWarning($"{nameof(CreateDocumentFromSecrets)} No elements in collection");
                }
                return document;
            }

            // This is where we sort the secret collection into a session per key.... Right now we assume the same
            // key for all....

            InternalEncryptionKey key = new InternalEncryptionKey(enumerator.Current.EncryptionKey);

            // Get some salt for the key-derivation function
            byte[] salt = New<IRandomGenerator>().Generate(16);

            SymmetricAlgorithm masterKey = DeriveMasterKey(key.DecryptPassphrase(), salt, _defaultRfc2898iterations);

            LastUpdateUtc = New<INow>().Utc;

            // Start building a session node, recording the used salt and the number of iterations
            XmlElement xecretsSession = document.CreateElement("XecretsSession");
            xecretsSession.Attributes.Append(document.CreateAttribute("KeyDerivation")).Value = "Rfc2898";
            xecretsSession.Attributes.Append(document.CreateAttribute("Salt")).Value = System.Convert.ToBase64String(salt);
            xecretsSession.Attributes.Append(document.CreateAttribute("Iterations")).Value = _defaultRfc2898iterations.ToString(CultureInfo.InvariantCulture);
            xecretsSession.Attributes.Append(document.CreateAttribute("LastUpdateUtc")).Value = LastUpdateUtc.ToString(CultureInfo.InvariantCulture);

            // Get the secrets collection in the form of an appropriate XML Element, suitable for encryption.
            XmlElement secretsElement = CreateSecretsElement(document, secrets);
            xecretsSession.AppendChild(secretsElement);

            // Encrypt the node, creating a named session key and embedding it in the output.
            EncryptedXml encryptedXml = new EncryptedXml();
            encryptedXml.AddKeyNameMapping(KEYNAME, masterKey);
            EncryptedData encryptedData = encryptedXml.Encrypt(secretsElement, KEYNAME);

            // Replace the encrypted element with the encrypted same
            EncryptedXml.ReplaceElement(secretsElement, encryptedData, false);

            // Finally, actually append this to the set of sessions.
            document.SelectSingleNode("AxantumXecrets/XecretsSessions").AppendChild(xecretsSession);

            return document;
        }

        /// <summary>
        /// Get an XmlNode representing the collection of secrets.
        /// </summary>
        /// <param name="secrets"></param>
        /// <returns></returns>
        private static XmlElement CreateSecretsElement(XmlDocument document, IEnumerable<Secret> secrets)
        {
            XmlElement secretsElement = document.CreateElement(SecretXMLElementNames.Secrets);
            foreach (Secret secret in secrets)
            {
                XmlElement secretElement = document.CreateElement(SecretXMLElementNames.Secret);

                secretElement.Attributes.Append(document.CreateAttribute("Id")).Value = secret.Id.ToString("N");
                secretElement.Attributes.Append(document.CreateAttribute("Type")).Value = ((int)secret.Type).ToString();

                if (secret.Type == AxCrypt.Api.Model.Secret.SecretType.Legacy || secret.Type == AxCrypt.Api.Model.Secret.SecretType.Password)
                {
                    XmlElement passwordElement = document.CreateElement(SecretXMLElementNames.Password);
                    passwordElement.AppendChild(document.CreateElement(SecretXMLElementNames.Title)).InnerText = secret.Password.Title;
                    passwordElement.AppendChild(document.CreateElement(SecretXMLElementNames.PasswordUrl)).InnerText = secret.Password.Url;
                    passwordElement.AppendChild(document.CreateElement(SecretXMLElementNames.Description)).InnerText = secret.Password.Description;
                    passwordElement.AppendChild(document.CreateElement(SecretXMLElementNames.PasswordUsername)).InnerText = secret.Password.Username;
                    passwordElement.AppendChild(document.CreateElement(SecretXMLElementNames.TheSecret)).InnerText = secret.Password.TheSecret;

                    secretElement.AppendChild(passwordElement);
                }

                if (secret.Type == AxCrypt.Api.Model.Secret.SecretType.Card)
                {
                    XmlElement cardElement = document.CreateElement(SecretXMLElementNames.Card);
                    cardElement.AppendChild(document.CreateElement(SecretXMLElementNames.CardNumber)).InnerText = secret.Card.Number;
                    cardElement.AppendChild(document.CreateElement(SecretXMLElementNames.Description)).InnerText = secret.Card.Description;
                    cardElement.AppendChild(document.CreateElement(SecretXMLElementNames.NameOnCard)).InnerText = secret.Card.NameOnCard;
                    cardElement.AppendChild(document.CreateElement(SecretXMLElementNames.SecurityCode)).InnerText = secret.Card.SecurityCode;
                    cardElement.AppendChild(document.CreateElement(SecretXMLElementNames.ExpirationDate)).InnerText = secret.Card.ExpirationDate;

                    secretElement.AppendChild(cardElement);
                }

                if (secret.Type == AxCrypt.Api.Model.Secret.SecretType.Note)
                {
                    XmlElement noteElement = document.CreateElement(SecretXMLElementNames.Note);
                    noteElement.AppendChild(document.CreateElement(SecretXMLElementNames.Description)).InnerText = secret.Note.Description;
                    noteElement.AppendChild(document.CreateElement(SecretXMLElementNames.Note)).InnerText = secret.Note.Note;

                    secretElement.AppendChild(noteElement);
                }

                secretsElement.AppendChild(secretElement);
            }
            return secretsElement;
        }

        private static SecretCollection GetSessionSecrets(XmlNodeList nodeList, EncryptionKey key)
        {
            SecretCollection secrets = new SecretCollection();

            foreach (XmlNode node in nodeList)
            {
                XmlElement secretElement = (XmlElement)node;

                Guid id = new Guid(secretElement.Attributes["Id"].Value);
                int secretTypeVal = int.Parse(secretElement.Attributes["Type"]?.Value ?? "1"); // Legacy = "1" is by default
                AxCrypt.Api.Model.Secret.SecretType secretType = (AxCrypt.Api.Model.Secret.SecretType)secretTypeVal;
                string title = String.Empty;
                string passwordUrl = String.Empty;
                string description = String.Empty;
                string passwordUsername = String.Empty;
                string theSecret = String.Empty;

                string cardNumber = String.Empty;
                string nameOnCard = String.Empty;
                string securityCode = String.Empty;
                string expirationDate = String.Empty;

                string note = String.Empty;

                DateTime creationDate = DateTime.MinValue;
                DateTime lastUpdateUtc = DateTime.MinValue;
                DateTime deletedUtc = DateTime.MinValue;

                secretElement = CreateSecretElement(secretElement, secretType);

                foreach (XmlNode textElement in secretElement.ChildNodes)
                {
                    switch (textElement.Name)
                    {
                        case SecretXMLElementNames.Title:
                            title = WebUtility.HtmlDecode(textElement.InnerText);
                            break;

                        case SecretXMLElementNames.PasswordUrl:
                            passwordUrl = WebUtility.HtmlDecode(textElement.InnerText);
                            break;

                        case SecretXMLElementNames.Description:
                            description = WebUtility.HtmlDecode(textElement.InnerText);
                            break;

                        case SecretXMLElementNames.PasswordUsername:
                            passwordUsername = WebUtility.HtmlDecode(textElement.InnerText);
                            break;

                        case SecretXMLElementNames.TheSecret:
                            theSecret = WebUtility.HtmlDecode(textElement.InnerText);
                            break;

                        case SecretXMLElementNames.CardNumber:
                            cardNumber = WebUtility.HtmlDecode(textElement.InnerText);
                            break;

                        case SecretXMLElementNames.NameOnCard:
                            nameOnCard = WebUtility.HtmlDecode(textElement.InnerText);
                            break;

                        case SecretXMLElementNames.SecurityCode:
                            securityCode = WebUtility.HtmlDecode(textElement.InnerText);
                            break;

                        case SecretXMLElementNames.ExpirationDate:
                            expirationDate = WebUtility.HtmlDecode(textElement.InnerText);
                            break;

                        case SecretXMLElementNames.Note:
                            note = WebUtility.HtmlDecode(textElement.InnerText);
                            break;

                        case SecretXMLElementNames.CreationDate:
                            creationDate = DateTime.Parse(WebUtility.HtmlDecode(textElement.InnerText), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                            break;

                        case SecretXMLElementNames.LastUpdateUtc:
                            lastUpdateUtc = DateTime.Parse(WebUtility.HtmlDecode(textElement.InnerText), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                            break;

                        case SecretXMLElementNames.DeletedUtc:
                            deletedUtc = DateTime.Parse(WebUtility.HtmlDecode(textElement.InnerText), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                            break;

                        default:
                            if (New<ILogging>().IsWarningEnabled)
                            {
                                New<ILogging>().LogWarning($"Unrecognized element: {textElement.Name}");
                            }
                            break;
                    }
                }

                if (secretType == Api.Model.Secret.SecretType.Legacy || secretType == Api.Model.Secret.SecretType.Password)
                {
                    SecretPassword secretPassword = new SecretPassword(title, passwordUrl, description, passwordUsername, theSecret);
                    secrets.Add(new InternalSecret(new Secret(id, secretPassword, key, creationDate, lastUpdateUtc, deletedUtc)));
                    continue;
                }

                if (secretType == Api.Model.Secret.SecretType.Card)
                {
                    SecretCard secretCard = new SecretCard(cardNumber, description, nameOnCard, securityCode, expirationDate);
                    secrets.Add(new InternalSecret(new Secret(id, secretCard, key, creationDate, lastUpdateUtc, deletedUtc)));
                    continue;
                }

                if (secretType == Api.Model.Secret.SecretType.Note)
                {
                    SecretNote secretNote = new SecretNote(description, note);
                    secrets.Add(new InternalSecret(new Secret(id, secretNote, key, creationDate, lastUpdateUtc, deletedUtc)));
                    continue;
                }
            }

            return secrets;
        }

        private static XmlElement CreateSecretElement(XmlElement secretElement, Api.Model.Secret.SecretType secretType)
        {
            switch (secretType)
            {
                case Api.Model.Secret.SecretType.Legacy:
                    return secretElement;

                case Api.Model.Secret.SecretType.Password:
                    return (XmlElement)secretElement.SelectSingleNode(SecretXMLElementNames.Password);

                case Api.Model.Secret.SecretType.Card:
                    return (XmlElement)secretElement.SelectSingleNode(SecretXMLElementNames.Card);

                case Api.Model.Secret.SecretType.Note:
                    return (XmlElement)secretElement.SelectSingleNode(SecretXMLElementNames.Note);
            }

            return secretElement;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "KeyDerviation")]
        private static SymmetricAlgorithm GetAndDeriveMasterKey(string key, XmlElement xecretsSessionElement)
        {
            string keyDerivation = xecretsSessionElement.Attributes["KeyDerivation"].Value;
            byte[] salt = Convert.FromBase64String(xecretsSessionElement.Attributes["Salt"].Value);
            int iterations = int.Parse(xecretsSessionElement.Attributes["Iterations"].Value, CultureInfo.InvariantCulture);

            Debug.Assert(string.Compare(keyDerivation, "Rfc2898", StringComparison.InvariantCulture) == 0, "The KeyDerviation attribute must be 'Rfc2898'");
            if (String.Compare(keyDerivation, "Rfc2898", StringComparison.InvariantCulture) != 0)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "The KeyDerviation attribute {0} must be 'Rfc2898'", keyDerivation));
            }

            if (iterations < 1000 || iterations > 1000000)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Too small or too large value for key derivation iterations {0}", iterations));
            }

            if (salt.Length != 16)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Invalid length {0} for the salt, it must be 16.", salt.Length));
            }

            return DeriveMasterKey(key, salt, iterations);
        }

        private static SymmetricAlgorithm DeriveMasterKey(string key, byte[] salt, int iterations)
        {
            // Get a key derivation function using the string key, the salt, and the configured # of iterations.
            Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(key, salt, iterations);

            // Build a master key for these secrets, setting the IV and the from the provided key
            RijndaelManaged masterKey = new RijndaelManaged();
            masterKey.KeySize = 256;
            masterKey.Key = deriveBytes.GetBytes(masterKey.Key.Length);
            masterKey.IV = deriveBytes.GetBytes(masterKey.IV.Length);
            return masterKey;
        }

        /// <summary>
        /// Calculate the # of RFC 2898 iterations dynamically to make the app keep up with faster CPU:s
        /// </summary>
        /// <returns></returns>
        private static int CalculateDefaultRfc2989Iterations()
        {
            // This is set to approximate about 1 second of work on a Pentium D 3GHz, i.e. typical CPU at the time of writing.
            const int TestIterations = 10000;

            DateTime now = New<INow>().Utc;
            Rfc2898DeriveBytes derivation = new Rfc2898DeriveBytes("test", new byte[16], TestIterations);
            // This is just to get some work done.
            derivation.GetBytes(48);
            TimeSpan time = New<INow>().Utc - now;

            // Calculate # ms/iteration, and then assume we want to work for 1000 ms to get # of iterations for 1 second.
            int iterations = (int)(1000 / (time.TotalMilliseconds / TestIterations));

            // We actually want 1/10th of a second work
            iterations = iterations / 10;

            // The minimum recommended # of iterations is 1000, which at the time of writing approximates slightly more
            // than 1/10th of a second.
            if (iterations < 1000)
            {
                iterations = 1000;
            }
            return iterations;
        }

        #endregion Private Helpers
    }
}