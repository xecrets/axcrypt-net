﻿using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Algorithm;
using AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Crypto.Asymmetric
{
    public class PublicKeyThumbprint : IEquatable<PublicKeyThumbprint>
    {
        private byte[] _thumbprint;

        public PublicKeyThumbprint(byte[] modulus, byte[] exponent)
        {
            if (modulus == null)
            {
                throw new ArgumentNullException("modulus");
            }
            if (exponent == null)
            {
                throw new ArgumentNullException("exponent");
            }

            Sha256 sha256 = New<Sha256>();

            byte[] bytes = modulus.Append(exponent);
            byte[] hash = sha256.ComputeHash(bytes);
            _thumbprint = hash.Reduce(16);
        }

        public PublicKeyThumbprint(byte[] thumbprint)
        {
            if (thumbprint == null)
            {
                throw new ArgumentNullException("thumbprint");
            }
            if (thumbprint.Length != 16)
            {
                throw new ArgumentException("The length must be 128 bits.", "thumbprint");
            }

            _thumbprint = (byte[])thumbprint.Clone();
        }

        public PublicKeyThumbprint(string value)
            : this(Convert.FromBase64String(value))
        {
        }

        public byte[] ToByteArray()
        {
            return (byte[])_thumbprint.Clone();
        }

        public override string ToString()
        {
            return Convert.ToBase64String(_thumbprint);
        }

        public bool Equals(PublicKeyThumbprint other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return _thumbprint.IsEquivalentTo(other._thumbprint);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(PublicKeyThumbprint) != obj.GetType())
            {
                return false;
            }
            PublicKeyThumbprint other = (PublicKeyThumbprint)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            int hashCode = _thumbprint.Aggregate(0, (s, v) => s + v);
            return hashCode;
        }

        public static bool operator ==(PublicKeyThumbprint left, PublicKeyThumbprint right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }
            if ((object)left == null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(PublicKeyThumbprint left, PublicKeyThumbprint right)
        {
            return !(left == right);
        }
    }
}