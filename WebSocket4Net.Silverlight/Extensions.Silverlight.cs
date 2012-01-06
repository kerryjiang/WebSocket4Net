using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.Text;

namespace WebSocket4Net
{
    public static partial class Extensions
    {
        public static string GetString(this Encoding encoding, byte[] buffer)
        {
            return encoding.GetString(buffer, 0, buffer.Length);
        }

        public static string GetPathAndQuery(this Uri uri)
        {
            int pos = uri.OriginalString.IndexOf(uri.Host);
            return uri.OriginalString.Substring(pos + uri.Host.Length);
        }
    }

    public static class MD5
    {
        public static HashAlgorithm Create()
        {
            return new MD5Managed();
        }
    }


    public static class SHA1
    {
        public static HashAlgorithm Create()
        {
            return new SHA1Managed();
        }
    }
}
