using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket4Net
{
    class JsonExecutor<T> : IJsonExecutor
    {
        public Type Type
        {
            get
            {
                return typeof(T);
            }
        }

        private Action<T> m_ExecutorAction;

        public JsonExecutor(Action<T> action)
        {
            m_ExecutorAction = action;
        }

        public void Execute(object param)
        {
            m_ExecutorAction.Method.Invoke(m_ExecutorAction.Target, new object[] { param });
        }
    }
}
