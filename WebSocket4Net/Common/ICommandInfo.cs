using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.Common
{
    public interface ICommandInfo
    {
        string Key { get; }
    }

    public interface ICommandInfo<TCommandData> : ICommandInfo
    {
        TCommandData Data { get; }
    }
}
