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

        private bool m_WithToken;

        private Action<T> m_ExecutorAction;

        private Action<string, T> m_ExecutorActionWithToken;

        public JsonExecutor(Action<T> action)
        {
            m_ExecutorAction = action;
        }

        public JsonExecutor(Action<string, T> action)
        {
            m_ExecutorActionWithToken = action;
            m_WithToken = true;
        }

        public void Execute(string token, object param)
        {
            if (m_WithToken)
                m_ExecutorActionWithToken.Method.Invoke(m_ExecutorActionWithToken.Target, new object[] { token, param });
            else
                m_ExecutorAction.Method.Invoke(m_ExecutorAction.Target, new object[] { param });
        }
    }
}
