using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.ClientEngine;

namespace WebSocket4Net.Protocol
{
    class HandshakeReader : ReaderBase
    {
        private const string m_BadRequestPrefix = "HTTP/1.1 400 ";

        protected static readonly string BadRequestCode = OpCode.BadRequest.ToString();

        static HandshakeReader()
        {

        }

        public HandshakeReader(WebSocket websocket)
            : base(websocket)
        {
            m_HeadSeachState = new SearchMarkState<byte>(HeaderTerminator);
        }

        protected static readonly byte[] HeaderTerminator = Encoding.UTF8.GetBytes("\r\n\r\n");

        private SearchMarkState<byte> m_HeadSeachState;

        protected static WebSocketCommandInfo DefaultHandshakeCommandInfo { get; private set; }

        public override WebSocketCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left)
        {
            left = 0;

            var prevMatched = m_HeadSeachState.Matched;

            var result = readBuffer.SearchMark(offset, length, m_HeadSeachState);

            if (result < 0)
            {
                AddArraySegment(readBuffer, offset, length);
                return null;
            }

            int findLen = result - offset;
            string handshake = string.Empty;

            if (this.BufferSegments.Count > 0)
            {
                if (findLen > 0)
                {
                    this.AddArraySegment(readBuffer, offset, findLen);
                    handshake = this.BufferSegments.Decode(Encoding.UTF8);
                }
                else
                {
                    handshake = this.BufferSegments.Decode(Encoding.UTF8, 0, this.BufferSegments.Count - prevMatched);
                }
            }
            else
            {
                handshake = Encoding.UTF8.GetString(readBuffer, offset, findLen);
            }

            left = length - findLen - (HeaderTerminator.Length - prevMatched);

            BufferSegments.ClearSegements();

            if (!handshake.StartsWith(m_BadRequestPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return new WebSocketCommandInfo
                    {
                        Key = OpCode.Handshake.ToString(),
                        Text = handshake
                    };
            }
            else
            {
                return new WebSocketCommandInfo
                {
                    Key = OpCode.BadRequest.ToString(),
                    Text = handshake
                };
            }
        }
    }
}
