using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net
{
    abstract class JsonExecutorBase<T> : IJsonExecutor
    {
        public Type Type
        {
            get
            {
                return typeof(T);
            }
        }

        public abstract void Execute(JsonWebSocket websocket, string token, object param);
    }

    class JsonExecutor<T> : JsonExecutorBase<T>
    {
        private Action<T> m_ExecutorAction;

        public JsonExecutor(Action<T> action)
        {
            m_ExecutorAction = action;
        }

        public override void Execute(JsonWebSocket websocket, string token, object param)
        {
            m_ExecutorAction.Method.Invoke(m_ExecutorAction.Target, new object[] { param });
        }
    }

    class JsonExecutorWithToken<T> : JsonExecutorBase<T>
    {
        private Action<string, T> m_ExecutorAction;

        public JsonExecutorWithToken(Action<string, T> action)
        {
            m_ExecutorAction = action;
        }

        public override void Execute(JsonWebSocket websocket, string token, object param)
        {
            m_ExecutorAction.Method.Invoke(m_ExecutorAction.Target, new object[] { token, param });
        }
    }

    class JsonExecutorWithSender<T> : JsonExecutorBase<T>
    {
        private Action<JsonWebSocket, T> m_ExecutorAction;

        public JsonExecutorWithSender(Action<JsonWebSocket, T> action)
        {
            m_ExecutorAction = action;
        }

        public override void Execute(JsonWebSocket websocket, string token, object param)
        {
            m_ExecutorAction.Method.Invoke(m_ExecutorAction.Target, new object[] { websocket, param });
        }
    }

    class JsonExecutorFull<T> : JsonExecutorBase<T>
    {
        private Action<JsonWebSocket, string, T> m_ExecutorAction;

        public JsonExecutorFull(Action<JsonWebSocket, string, T> action)
        {
            m_ExecutorAction = action;
        }

        public override void Execute(JsonWebSocket websocket, string token, object param)
        {
            m_ExecutorAction.Method.Invoke(m_ExecutorAction.Target, new object[] { websocket, token, param });
        }
    }

    class JsonExecutorWithSenderAndState<T> : JsonExecutorBase<T>
    {
        private Action<JsonWebSocket, T, object> m_ExecutorAction;

        private object m_State;

        public JsonExecutorWithSenderAndState(Action<JsonWebSocket, T, object> action, object state)
        {
            m_ExecutorAction = action;
            m_State = state;
        }

        public override void Execute(JsonWebSocket websocket, string token, object param)
        {
            m_ExecutorAction.Method.Invoke(m_ExecutorAction.Target, new object[] { websocket, param, m_State });
        }
    }
}
