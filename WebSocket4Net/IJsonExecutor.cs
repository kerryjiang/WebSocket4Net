using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket4Net
{
    interface IJsonExecutor
    {
        Type Type { get; }
        void Execute(object param);
    }
}
