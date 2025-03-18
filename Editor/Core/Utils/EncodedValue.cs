using System;

namespace Wireframe
{
    internal readonly struct EncodedValue<T> : IEquatable<EncodedValue<T>>
    {
        private readonly byte[] m_value;

        public EncodedValue(T value)
        {
            m_value = Encode(value);
        }

        public EncodedValue(byte[] bytes)
        {
            m_value = bytes;
        }

        public static byte[] Encode(T value)
        {
            char[] m = new char[] { 'B', 'i', 'l', 'd', 'U', 'p', 'l', 'o', 'd', 'r' };
            char[] s = value.ToString().ToCharArray();
            char[] c = new char[m.Length + s.Length];
            for (int i = 0; i < m.Length; i++)
            {
                c[i] = (char)(m[i] + 10);
            }

            for (int i = 0; i < s.Length; i++)
            {
                c[i + 10] = (char)(s[i] + 10);
            }

            return System.Text.Encoding.UTF8.GetBytes(c);
        }

        public string Encode64()
        {
            return Convert.ToBase64String(m_value);
        }

        public static string Encode64(T value)
        {
            var base64String = Convert.ToBase64String(Encode(value));
            return base64String;
        }

        public static EncodedValue<T> Decode64(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return new EncodedValue<T>(Array.Empty<byte>());
            }

            return new EncodedValue<T>(Convert.FromBase64String(s));
        }

        private static T Decode(byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                return default;
            }

            char[] chars = System.Text.Encoding.UTF8.GetChars(bytes);
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)(chars[i] - 10);
            }

            string v = new string(chars, 10, chars.Length - 10);
            T newValue = (T)Convert.ChangeType(v, typeof(T));
            return newValue;
        }

        public static implicit operator T(EncodedValue<T> encodedValue)
        {
            return Decode(encodedValue.m_value);
        }

        public static implicit operator EncodedValue<T>(T value)
        {
            return new EncodedValue<T>(value);
        }

        public static EncodedValue<T> Encoded(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return new EncodedValue<T>(Array.Empty<byte>());
            }

            return new EncodedValue<T>(Convert.FromBase64String(s));
        }

        public bool Equals(EncodedValue<T> other)
        {
            return Equals(m_value, other.m_value);
        }

        public override bool Equals(object obj)
        {
            return obj is EncodedValue<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (m_value != null ? m_value.GetHashCode() : 0);
        }
    }
}