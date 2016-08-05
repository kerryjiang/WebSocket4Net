using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.Common
{
    public class StringCommandInfo : CommandInfo<string>
    {
        public StringCommandInfo(string key, string data, string[] parameters)
            : base(key, data)
        {
            Parameters = parameters;
        }

        public string[] Parameters { get; private set; }

        public string GetFirstParam()
        {
            if (Parameters.Length > 0)
                return Parameters[0];

            return string.Empty;
        }

        public string this[int index]
        {
            get { return Parameters[index]; }
        }
    }
}
