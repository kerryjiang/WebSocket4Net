using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.ClientEngine;

namespace WebSocket4Net.Protocol
{
    class DraftHybi00HandshakeReader : HandshakeReader
    {
        //-1 indicate response header has not been received
        private int m_ReceivedChallengeLength = -1;
        private int m_ExpectedChallengeLength = 16;

        private WebSocketCommandInfo m_HandshakeCommand = null;
        private byte[] m_Challenges = new byte[16];

        

        public DraftHybi00HandshakeReader(WebSocket websocket)
            : base(websocket)
        {

        }

        void SetDataReader()
        {
            NextCommandReader = new DraftHybi00DataReader(this);
        }

        public override WebSocketCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left)
        {
            //haven't receive handshake header
            if (m_ReceivedChallengeLength < 0)
            {
                var commandInfo = base.GetCommandInfo(readBuffer, offset, length, out left);

                if (commandInfo == null)
                    return null;

                //Bad request
                if (BadRequestCode.Equals(commandInfo.Key))
                    return commandInfo;

                m_ReceivedChallengeLength = 0;
                m_HandshakeCommand = commandInfo;

                var challengeOffset = offset + length - left;

                if (left < m_ExpectedChallengeLength)
                {
                    if (left > 0)
                    {
                        Buffer.BlockCopy(readBuffer, challengeOffset, m_Challenges, 0, left);
                        m_ReceivedChallengeLength = left;
                        left = 0;
                    }

                    return null;
                }
                else if (left == m_ExpectedChallengeLength)
                {
                    Buffer.BlockCopy(readBuffer, challengeOffset, m_Challenges, 0, left);
                    SetDataReader();
                    m_HandshakeCommand.Data = m_Challenges;
                    left = 0;
                    return m_HandshakeCommand;
                }
                else
                {
                    Buffer.BlockCopy(readBuffer, challengeOffset, m_Challenges, 0, m_ExpectedChallengeLength);
                    left -= m_ExpectedChallengeLength;
                    SetDataReader();
                    m_HandshakeCommand.Data = m_Challenges;
                    return m_HandshakeCommand;
                }
            }
            else
            {
                int receivedTotal = m_ReceivedChallengeLength + length;
                
                if (receivedTotal < m_ExpectedChallengeLength)
                {
                    Buffer.BlockCopy(readBuffer, offset, m_Challenges, m_ReceivedChallengeLength, length);
                    left = 0;
                    m_ReceivedChallengeLength = receivedTotal;
                    return null;
                }
                else if (receivedTotal == m_ExpectedChallengeLength)
                {
                    Buffer.BlockCopy(readBuffer, offset, m_Challenges, m_ReceivedChallengeLength, length);
                    left = 0;
                    SetDataReader();
                    m_HandshakeCommand.Data = m_Challenges;
                    return m_HandshakeCommand;
                }
                else
                {
                    var parsedLen = m_ExpectedChallengeLength - m_ReceivedChallengeLength;
                    Buffer.BlockCopy(readBuffer, offset, m_Challenges, m_ReceivedChallengeLength, parsedLen);
                    left = length - parsedLen;
                    SetDataReader();
                    m_HandshakeCommand.Data = m_Challenges;
                    return m_HandshakeCommand;
                }
            }
        }
    }
}
