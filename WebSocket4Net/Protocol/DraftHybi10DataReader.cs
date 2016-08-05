using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.Common;
using WebSocket4Net.Protocol.FramePartReader;

namespace WebSocket4Net.Protocol
{
    class DraftHybi10DataReader : IClientCommandReader<WebSocketCommandInfo>
    {
        public DraftHybi10DataReader()
        {
            m_Frame = new WebSocketDataFrame(new ArraySegmentList());
            m_PartReader = DataFramePartReader.NewReader;
        }

        private List<WebSocketDataFrame> m_PreviousFrames;
        private WebSocketDataFrame m_Frame;
        private IDataFramePartReader m_PartReader;
        private int m_LastPartLength = 0;

        public int LeftBufferSize
        {
            get { return m_Frame.InnerData.Count; }
        }

        public IClientCommandReader<WebSocketCommandInfo> NextCommandReader
        {
            get { return this; }
        }

        protected void AddArraySegment(ArraySegmentList segments, byte[] buffer, int offset, int length, bool isReusableBuffer)
        {
            segments.AddSegment(buffer, offset, length, isReusableBuffer);
        }

        public WebSocketCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left)
        {
            this.AddArraySegment(m_Frame.InnerData, readBuffer, offset, length, true);

            IDataFramePartReader nextPartReader;

            int thisLength = m_PartReader.Process(m_LastPartLength, m_Frame, out nextPartReader);

            if (thisLength < 0)
            {
                left = 0;
                return null;
            }
            else
            {
                left = thisLength;

                if (left > 0)
                    m_Frame.InnerData.TrimEnd(left);

                //Means this part reader is the last one
                if (nextPartReader == null)
                {
                    WebSocketCommandInfo commandInfo;

                    // Control frames MAY be injected in the middle of
                    // a fragmented message.Control frames themselves MUST NOT be
                    // fragmented.
                    if (m_Frame.IsControlFrame)
                    {
                        commandInfo = new WebSocketCommandInfo(m_Frame);
                        m_Frame.Clear();
                    }
                    else if (m_Frame.FIN)
                    {
                        if (m_PreviousFrames != null && m_PreviousFrames.Count > 0)
                        {
                            m_PreviousFrames.Add(m_Frame);
                            m_Frame = new WebSocketDataFrame(new ArraySegmentList());
                            commandInfo = new WebSocketCommandInfo(m_PreviousFrames);
                            m_PreviousFrames = null;
                        }
                        else
                        {
                            commandInfo = new WebSocketCommandInfo(m_Frame);
                            m_Frame.Clear();
                        }
                    }
                    else
                    {
                        if (m_PreviousFrames == null)
                            m_PreviousFrames = new List<WebSocketDataFrame>();

                        m_PreviousFrames.Add(m_Frame);
                        m_Frame = new WebSocketDataFrame(new ArraySegmentList());

                        commandInfo = null;
                    }

                    //BufferSegments.ClearSegements();
                    m_LastPartLength = 0;
                    m_PartReader = DataFramePartReader.NewReader;

                    return commandInfo;
                }
                else
                {
                    m_LastPartLength = m_Frame.InnerData.Count - thisLength;
                    m_PartReader = nextPartReader;

                    return null;
                }
            }
        }
    }
}
