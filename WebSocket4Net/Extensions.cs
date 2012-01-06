using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket4Net
{
    public static partial class Extensions
    {
        private readonly static char[] m_CrCf;

        static Extensions()
        {
            m_CrCf = "\r\n".ToArray();
        }

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
    }
}
