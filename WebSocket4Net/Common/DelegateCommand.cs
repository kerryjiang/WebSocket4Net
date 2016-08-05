using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.ClientEngine;

namespace WebSocket4Net.Common
{
    public delegate void CommandDelegate<TClientSession, TCommandInfo>(TClientSession session, TCommandInfo commandInfo);

    class DelegateCommand<TClientSession, TCommandInfo> : ICommand<TClientSession, TCommandInfo>
        where TClientSession : IClientSession
        where TCommandInfo : ICommandInfo
    {
        private CommandDelegate<TClientSession, TCommandInfo> m_Execution;

        public DelegateCommand(string name, CommandDelegate<TClientSession, TCommandInfo> execution)
        {
            Name = name;
            m_Execution = execution;
        }

        public void ExecuteCommand(TClientSession session, TCommandInfo commandInfo)
        {
            m_Execution(session, commandInfo);
        }

        public string Name { get; private set; }
    }
}
