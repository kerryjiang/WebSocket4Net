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

        #endregion ICommandInfo<TCommandData> Members

        #region ICommandInfo Members

        public string Key { get; private set; }

        #endregion ICommandInfo Members
    }
}