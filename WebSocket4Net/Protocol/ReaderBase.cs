using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.Common;

namespace WebSocket4Net.Protocol
{
    public abstract class ReaderBase : IClientCommandReader<WebSocketCommandInfo>
    {
        protected WebSocket WebSocket { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderBase"/> class.
        /// </summary>
        /// <param name="websocket">The websocket.</param>
        public ReaderBase(WebSocket websocket)
        {
            WebSocket = websocket;
            m_BufferSegments = new ArraySegmentList();
        }

        private readonly ArraySegmentList m_BufferSegments;

        /// <summary>
        /// Gets the buffer segments which can help you parse your commands conviniently.
        /// </summary>
        protected ArraySegmentList BufferSegments
        {
            get { return m_BufferSegments; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderBase"/> class.
        /// </summary>
        /// <param name="previousCommandReader">The previous command reader.</param>
        public ReaderBase(ReaderBase previousCommandReader)
        {
            m_BufferSegments = previousCommandReader.BufferSegments;
        }

        public abstract WebSocketCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left);

        /// <summary>
        /// Gets or sets the next command reader.
        /// </summary>
        /// <value>
        /// The next command reader.
        /// </value>
        public IClientCommandReader<WebSocketCommandInfo> NextCommandReader { get; internal set; }

        /// <summary>
        /// Adds the array segment into BufferSegment.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        protected void AddArraySegment(byte[] buffer, int offset, int length)
        {
            BufferSegments.AddSegment(buffer, offset, length, true);
        }

        /// <summary>
        /// Clears the buffer segments.
        /// </summary>
        protected void ClearBufferSegments()
        {
            BufferSegments.ClearSegements();
        }
    }
}
