using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace WebSocket4Net.Protocol
{
    class HandshakeReader : ReaderBase
    {
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

            var result = readBuffer.SearchMark(offset, length, m_HeadSeachState);

            if (result < 0)
            {
                AddArraySegment(readBuffer, offset, length);
                return null;
            }

            BufferSegments.AddSegment(readBuffer, offset, result - offset, false);

            string handshake = BufferSegments.Decode(Encoding.UTF8);

            left = length - (result - offset + HeaderTerminator.Length);

            BufferSegments.ClearSegements();

            m_HeadSeachState.Matched = 0;

            return new WebSocketCommandInfo
                {
                    Key = OpCode.Handshake.ToString(),
                    Text = handshake
                };
        }
    }
}
