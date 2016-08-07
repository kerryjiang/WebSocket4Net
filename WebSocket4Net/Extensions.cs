using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using SuperSocket.ClientEngine;

#if !NETFX_CORE
using System.Security.Cryptography;
#else
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
#endif


namespace WebSocket4Net
{
    public static partial class Extensions
    {
        private readonly static char[] m_CrCf = new char[] { '\r', '\n' };

        public static void AppendFormatWithCrCf(this StringBuilder builder, string format, object arg)
        {
            builder.AppendFormat(format, arg);
            builder.Append(m_CrCf);
        }

        public static void AppendFormatWithCrCf(this StringBuilder builder, string format, params object[] args)
        {
            builder.AppendFormat(format, args);
            builder.Append(m_CrCf);
        }

        public static void AppendWithCrCf(this StringBuilder builder, string content)
        {
            builder.Append(content);
            builder.Append(m_CrCf);
        }

        public static void AppendWithCrCf(this StringBuilder builder)
        {
            builder.Append(m_CrCf);
        }

        private const string m_Tab = "\t";
        private const char m_Colon = ':';
        private const string m_Space = " ";
        private const string m_ValueSeparator = ", ";

        public static bool ParseMimeHeader(this string source, IDictionary<string, object> valueContainer, out string verbLine)
        {
            verbLine = string.Empty;

            var items = valueContainer;

            string line;
            string prevKey = string.Empty;

            var reader = new StringReader(source);

            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (string.IsNullOrEmpty(verbLine))
                {
                    verbLine = line;
                    continue;
                }

                object currentValue;

                if (line.StartsWith(m_Tab) && !string.IsNullOrEmpty(prevKey))
                {
                    if (!items.TryGetValue(prevKey, out currentValue))
                        return false;

                    items[prevKey] = currentValue + line.Trim();
                    continue;
                }

                int pos = line.IndexOf(m_Colon);

                if (pos < 0)
                    continue;

                string key = line.Substring(0, pos);

                if (!string.IsNullOrEmpty(key))
                    key = key.Trim();

                string value = line.Substring(pos + 1);
                if (!string.IsNullOrEmpty(value) && value.StartsWith(m_Space) && value.Length > 1)
                    value = value.Substring(1);

                if (string.IsNullOrEmpty(key))
                    continue;

                if (!items.TryGetValue(key, out currentValue))
                {
                    items.Add(key, value);
                }
                else
                {
                    items[key] = currentValue + m_ValueSeparator + value;
                }

                prevKey = key;
            }

            return true;
        }

        public static TValue GetValue<TValue>(this IDictionary<string, object> valueContainer, string name)
        {
            var defaultValue = default(TValue);
            return GetValue(valueContainer, name, defaultValue);
        }

        public static TValue GetValue<TValue>(this IDictionary<string, object> valueContainer, string name, TValue defaultValue)
        {
            object value;

            if (!valueContainer.TryGetValue(name, out value))
                return defaultValue;

            return (TValue)value;
        }

        private static Type[] m_SimpleTypes = new Type[] {
                typeof(String),
                typeof(Decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
            };


        internal static bool IsSimpleType(this Type type)
        {
#if NETFX_CORE || NETCORE
            var typeInfo = type.GetTypeInfo();

            return
                typeInfo.IsValueType ||
                typeInfo.IsPrimitive ||
                m_SimpleTypes.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;

#else
            return
                type.IsValueType ||
                type.IsPrimitive ||
                m_SimpleTypes.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
#endif
        }

        public static string GetOrigin(this Uri uri)
        {
#if NETFX_CORE || NETCORE
            return uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
#else
            return uri.GetLeftPart(UriPartial.Authority);
#endif
        }

        public static byte[] ComputeMD5Hash(this byte[] source)
        {
#if NETFX_CORE
            var algProv = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var hash = algProv.CreateHash();
            hash.Append(CryptographicBuffer.CreateFromByteArray(source));
            byte[] result;
            CryptographicBuffer.CopyToByteArray(hash.GetValueAndReset(), out result);
            return result;
#else
            return MD5.Create().ComputeHash(source);
#endif

        }

        public static string CalculateChallenge(this string source)
        {
#if NETFX_CORE
            var algProv = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            var hash = algProv.CreateHash();
            hash.Append(CryptographicBuffer.ConvertStringToBinary(source, BinaryStringEncoding.Utf8));
            return CryptographicBuffer.EncodeToBase64String(hash.GetValueAndReset());

#elif !SILVERLIGHT
            return Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(source)));
#else
            return Convert.ToBase64String(SHA1.Create().ComputeHash(ASCIIEncoding.Instance.GetBytes(source)));
#endif

        }
    }
}
