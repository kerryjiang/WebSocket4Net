using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.Common
{
    public interface IClientCommandReader<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        TCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left);

        IClientCommandReader<TCommandInfo> NextCommandReader { get; }
    }
}
