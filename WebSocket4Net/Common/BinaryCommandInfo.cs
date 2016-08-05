using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.Common
{
    public class BinaryCommandInfo : CommandInfo<byte[]>
    {
        public BinaryCommandInfo(string key, byte[] data)
            : base(key, data)
        {

        }
    }
}
