using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SuperSocket.ClientEngine;

namespace WebSocket4Net.Protocol
{
    /// <summary>
    /// http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-00
    /// </summary>
    class DraftHybi00Processor : ProtocolProcessorBase
    {
        public DraftHybi00Processor()
            : base(WebSocketVersion.DraftHybi00, new CloseStatusCodeHybi10())
        {

        }

        private static List<char> m_CharLib = new List<char>();
        private static List<char> m_DigLib = new List<char>();
        private static Random m_Random = new Random();

        public const byte StartByte = 0x00;
        public const byte EndByte = 0xFF;

        public static byte[] CloseHandshake = new byte[] { 0xFF, 0x00 };

        private byte[] m_ExpectedChallenge;

        static DraftHybi00Processor()
        {
            for (int i = 33; i <= 126; i++)
            {
                char currentChar = (char)i;

                if (char.IsLetter(currentChar))
                    m_CharLib.Add(currentChar);
                else if (char.IsDigit(currentChar))
                    m_DigLib.Add(currentChar);
            }
        }

        public override ReaderBase CreateHandshakeReader(WebSocket websocket)
        {
            return new DraftHybi00HandshakeReader(websocket);
        }

        private const string m_Error_ChallengeLengthNotMatch = "challenge length doesn't match";
        private const string m_Error_ChallengeNotMatch = "challenge doesn't match";
        private const string m_Error_InvalidHandshake = "invalid handshake";

        public override bool VerifyHandshake(WebSocket websocket, WebSocketCommandInfo handshakeInfo, out string description)
        {
            var challenge = handshakeInfo.Data;

            if (challenge.Length != challenge.Length)
            {
                description = m_Error_ChallengeLengthNotMatch;
                return false;
            }

            for (var i = 0; i < m_ExpectedChallenge.Length; i++)
            {
                if (challenge[i] != m_ExpectedChallenge[i])
                {
                    description = m_Error_ChallengeNotMatch;
                    return false;
                }
            }

            if (!handshakeInfo.Text.ParseMimeHeader(websocket.Items))
            {
                description = m_Error_InvalidHandshake;
                return false;
            }

            description = string.Empty;
            return true;
        }

        public override void SendMessage(WebSocket websocket, string message)
        {
            var maxByteCount = Encoding.UTF8.GetMaxByteCount(message.Length) + 2;
            var sendBuffer = new byte[maxByteCount];
            sendBuffer[0] = StartByte;
            int bytesCount = Encoding.UTF8.GetBytes(message, 0, message.Length, sendBuffer, 1);
            sendBuffer[1 + bytesCount] = EndByte;

            websocket.Client.Send(sendBuffer, 0, bytesCount + 2);
        }

        public override void SendData(WebSocket websocket, byte[] data, int offset, int length)
        {
            throw new NotSupportedException();
        }

        public override void SendData(WebSocket websocket, IList<ArraySegment<byte>> segments)
        {
            throw new NotSupportedException();
        }

        public override void SendCloseHandshake(WebSocket websocket, int statusCode, string closeReason)
        {
            websocket.Client.Send(CloseHandshake, 0, CloseHandshake.Length);
        }

        public override void SendPing(WebSocket websocket, string ping)
        {
            throw new NotSupportedException();
        }

        public override void SendPong(WebSocket websocket, string pong)
        {
            throw new NotSupportedException();
        }

        public override void SendHandshake(WebSocket websocket)
        {
            string secKey1 = Encoding.UTF8.GetString(GenerateSecKey());

            string secKey2 = Encoding.UTF8.GetString(GenerateSecKey());

            byte[] secKey3 = GenerateSecKey(8);

            m_ExpectedChallenge = GetResponseSecurityKey(secKey1, secKey2, secKey3);

            var handshakeBuilder = new StringBuilder();

#if SILVERLIGHT
            handshakeBuilder.AppendFormatWithCrCf("GET {0} HTTP/1.1", websocket.TargetUri.GetPathAndQuery());
#else
            handshakeBuilder.AppendFormatWithCrCf("GET {0} HTTP/1.1", websocket.TargetUri.PathAndQuery);
#endif

            handshakeBuilder.AppendWithCrCf("Upgrade: WebSocket");
            handshakeBuilder.AppendWithCrCf("Connection: Upgrade");
            handshakeBuilder.Append("Sec-WebSocket-Key1: ");
            handshakeBuilder.AppendWithCrCf(secKey1);
            handshakeBuilder.Append("Sec-WebSocket-Key2: ");
            handshakeBuilder.AppendWithCrCf(secKey2);
            handshakeBuilder.Append("Host: ");
            handshakeBuilder.AppendWithCrCf(websocket.TargetUri.Host);
            handshakeBuilder.Append("Origin: ");
            handshakeBuilder.AppendWithCrCf(string.IsNullOrEmpty(websocket.Origin) ? websocket.TargetUri.Host : websocket.Origin);

            if (!string.IsNullOrEmpty(websocket.SubProtocol))
            {
                handshakeBuilder.Append("Sec-WebSocket-Protocol: ");
                handshakeBuilder.AppendWithCrCf(websocket.SubProtocol);
            }

            var cookies = websocket.Cookies;

            if (cookies != null && cookies.Count > 0)
            {
                string[] cookiePairs = new string[cookies.Count];
                for (int i = 0; i < cookies.Count; i++)
                {
                    var item = cookies[i];
                    cookiePairs[i] = item.Key + "=" + Uri.EscapeUriString(item.Value);
                }
                handshakeBuilder.Append("Cookie: ");
                handshakeBuilder.AppendWithCrCf(string.Join(";", cookiePairs));
            }

            if (websocket.CustomHeaderItems != null)
            {
                for (var i = 0; i < websocket.CustomHeaderItems.Count; i++)
                {
                    var item = websocket.CustomHeaderItems[i];

                    handshakeBuilder.AppendFormatWithCrCf(HeaderItemFormat, item.Key, item.Value);
                }
            }

            handshakeBuilder.AppendWithCrCf();
            handshakeBuilder.Append(Encoding.UTF8.GetString(secKey3, 0, secKey3.Length));

            byte[] handshakeBuffer = Encoding.UTF8.GetBytes(handshakeBuilder.ToString());

            websocket.Client.Send(handshakeBuffer, 0, handshakeBuffer.Length);
        }

        private byte[] GetResponseSecurityKey(string secKey1, string secKey2, byte[] secKey3)
        {
            //Remove all symbols that are not numbers
            string k1 = Regex.Replace(secKey1, "[^0-9]", String.Empty);
            string k2 = Regex.Replace(secKey2, "[^0-9]", String.Empty);

            //Convert received string to 64 bit integer.
            Int64 intK1 = Int64.Parse(k1);
            Int64 intK2 = Int64.Parse(k2);

            //Dividing on number of spaces
            int k1Spaces = secKey1.Count(c => c == ' ');
            int k2Spaces = secKey2.Count(c => c == ' ');
            int k1FinalNum = (int)(intK1 / k1Spaces);
            int k2FinalNum = (int)(intK2 / k2Spaces);

            //Getting byte parts
            byte[] b1 = BitConverter.GetBytes(k1FinalNum);
            Array.Reverse(b1);
            byte[] b2 = BitConverter.GetBytes(k2FinalNum);
            Array.Reverse(b2);
            byte[] b3 = secKey3;

            //Concatenating everything into 1 byte array for hashing.
            byte[] bChallenge = new byte[b1.Length + b2.Length + b3.Length];
            Array.Copy(b1, 0, bChallenge, 0, b1.Length);
            Array.Copy(b2, 0, bChallenge, b1.Length, b2.Length);
            Array.Copy(b3, 0, bChallenge, b1.Length + b2.Length, b3.Length);

            //Hash and return
            byte[] hash = MD5.Create().ComputeHash(bChallenge);
            return hash;
        }

        private byte[] GenerateSecKey()
        {
            int totalLen = m_Random.Next(10, 20);
            return GenerateSecKey(totalLen);
        }

        private byte[] GenerateSecKey(int totalLen)
        {
            int spaceLen = m_Random.Next(1, totalLen / 2 + 1);
            int charLen = m_Random.Next(3, totalLen - 1 - spaceLen);
            int digLen = totalLen - spaceLen - charLen;

            byte[] source = new byte[totalLen];

            var pos = 0;

            for (int i = 0; i < spaceLen; i++)
                source[pos++]  = (byte)' ';

            for (int i = 0; i < charLen; i++)
            {
                source[pos++] = (byte)m_CharLib[m_Random.Next(0, m_CharLib.Count - 1)];
            }

            for (int i = 0; i < digLen; i++)
            {
                source[pos++] = (byte)m_DigLib[m_Random.Next(0, m_DigLib.Count - 1)];
            }

            return source.RandomOrder();
        }

        public override bool SupportBinary
        {
            get { return false; }
        }

        public override bool SupportPingPong
        {
            get { return false; }
        }
    }
}
