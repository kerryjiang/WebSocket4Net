using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.Common
{
    public abstract class CommandInfo<TCommandData> : ICommandInfo<TCommandData>
    {
        public CommandInfo(string key, TCommandData data)
        {
            Key = key;
            Data = data;
        }

        #region ICommandInfo<TCommandData> Members

        public TCommandData Data { get; private set; }

        #endregion

        #region ICommandInfo Members

        public string Key { get; private set; }

        #endregion
    }
}
