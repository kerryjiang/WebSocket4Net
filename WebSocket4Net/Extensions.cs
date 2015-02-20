using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

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
            return
                type.IsValueType ||
                type.IsPrimitive ||
                m_SimpleTypes.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }
    }
}
