using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.Common
{
    public interface ICommand
    {
        string Name { get; }
    }

    public interface ICommand<TSession, TCommandInfo> : ICommand
        where TCommandInfo : ICommandInfo
    {
        void ExecuteCommand(TSession session, TCommandInfo commandInfo);
    }
}
