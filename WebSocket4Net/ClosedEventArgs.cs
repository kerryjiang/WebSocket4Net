using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket4Net
{
    /// <summary>
    /// The event args for closed event
    /// </summary>
    public class ClosedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the code of close reason.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public short Code { get; private set; }

        /// <summary>
        /// Gets the description of the close reason.
        /// </summary>
        /// <value>
        /// The reason.
        /// </value>
        public string Reason { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClosedEventArgs"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="reason">The reason.</param>
        public ClosedEventArgs(short code, string reason)
        {
            Code = code;
            Reason = reason;
        }
    }
}
