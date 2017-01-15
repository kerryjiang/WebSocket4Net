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

	    // prevMatched is needed because handshake may have come
	    // in a number of segments, and the HeaderTerminator that
	    // we're looking for might lie across previous segments
	    // and `readBuffer`.
	    //
	    // More precisely, prevMatched > 0 if and only if the last
	    // byte(s) at end of the previous segment **started like**
	    // an incomplete HeaderTerminator that **may or may not**
	    // be continued and completed in current `readBuffer`.  --
	    // fidergo-stephane-gourichon
            var prevMatched = m_HeadSeachState.Matched;

	    // At this point in code **we don't know yet** if the
	    // `prevMatched` bytes that match the `HeaderTerminator`
	    // at end of previous segment are part of a **full match**
	    // (in this case the value of `prevMatched` is useful) or
	    // just a **partial** (in that cas the value of
	    // `prevMatched` is irrelevant).
	    
            var result = readBuffer.SearchMark(offset, length, m_HeadSeachState);

            if (result < 0)
            {
		// We've not found the HeaderTerminator yet.  We'll be
		// called again when more data arrives.  --
		// fidergo-stephane-gourichon
                AddArraySegment(readBuffer, offset, length);
                return null;
            }

	    // We've found the HeaderTerminator.  All might be in
	    // readBuffer, or handshake might be cut across the last
	    // segment, or just the HeaderTerminator might be in
	    // readBuffer or even cut across.  We must handle all
	    // those cases.  -- fidergo-stephane-gourichon
	    
            int findLen = result - offset;
            string handshake = string.Empty;

            if (this.BufferSegments.Count > 0)
            {
                if (findLen > 0)
                {
		    // In this code path we know that the handshake
		    // was cut across at least previous segments and
		    // `readBuffer`.  In other words,
		    // `readBuffer[offset]` starts with at least one
		    // byte that belongs to the handshake proper
		    // (excluding `HeaderTerminator`).  So, we add
		    // those bytes and extract the handshake.

                    this.AddArraySegment(readBuffer, offset, findLen);
                    handshake = this.BufferSegments.Decode(Encoding.UTF8);

		    // Now, we need to correct `prevMatched`.  Indeed,
		    // in this code path, any byte(s) matched at end
		    // of previous segment were not part of an actual
		    // HeaderTerminator cut across.  If
		    // `prevMatched`>0, such byte(s) was / were a
		    // partial match that `readBuffer` content
		    // disproved.
		    //
		    // So, basically, there were actually zero bytes
		    // of the actual `HeaderTerminator` match in
		    // previous segment.  We reflect that by setting
		    // prevMatched = 0. -- fidergo-stephane-gourichon

                    prevMatched = 0;

		    // If we did not set prevMatched to zero, `left`
		    // would be too big and our read pointer would not
		    // advance enough, causing desynchronization in WS
		    // protocol decoding, failure to recognize further
		    // messages, server closing connection for lack of
		    // reply to ping. -- fidergo-stephane-gourichon
                }
                else
                {
		    // The handshake was actually fully inside the
		    // previous segment.  That segment possibly ended
		    // with `prevMatch` bytes of the
		    // `HeaderTerminator` that we have to shave
		    // off. -- fidergo-stephane-gourichon

                    handshake = this.BufferSegments.Decode(Encoding.UTF8, 0, this.BufferSegments.Count - prevMatched);
                }
            }
            else
            {
		// In this code path, there was no previous segment.
		// Everything is in `readBuffer`.
                handshake = Encoding.UTF8.GetString(readBuffer, offset, findLen);
		// I'm nearly sure prevMatched is always zero already,
		// if reset between invocations (see below
		// `m_HeadSeachState.Matched = 0`).  I'm definitely
		// sure it should be zero here.  An assert would be
		// good.  As a fallback set it. --
		// fidergo-stephane-gourichon
		prevMatched = 0; 
            }

	    // We must tell caller how many bytes are left, with a
	    // formula that works in all cases.
	    // It works if prevMatched reflects actual match, not
	    // partial match, as set to zero above. --
	    // fidergo-stephane-gourichon

            left = length - findLen - (HeaderTerminator.Length - prevMatched);

	    // Rationale: left bytes are all bytes minus bytes
	    // consumed.  We consume the part of the handshake that's
	    // in `readBuffer`, which is `findLen` bytes.  We also
	    // consume the part of the `HeaderTerminator` that is in
	    // `readBuffer`, which is `(HeaderTerminator.Length -
	    // prevMatched)` bytes.

            BufferSegments.ClearSegements();
	    
	    // In case the object is reused, reset search state.  I'm
	    // nearly sure this is always zero already. An assert
	    // would be good.  As a fallback set it. --
	    // fidergo-stephane-gourichon

            m_HeadSeachState.Matched = 0;
	    
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
