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
            #if NETCORE
                m_ExecutorAction.DynamicInvoke(new object[] { param });
            #else                
                m_ExecutorAction.Method.Invoke(m_ExecutorAction.Target, new object[] { param });
            #endif
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
            #if NETCORE
                m_ExecutorAction.DynamicInvoke(new object[] { token, param });
            #else                
                m_ExecutorAction.Method.Invoke(m_ExecutorAction.Target, new object[] { token, param });
            #endif
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
            #if NETCORE
                m_ExecutorAction.DynamicInvoke(new object[] { websocket, param });
            #else                
                m_ExecutorAction.Method.Invoke(m_ExecutorAction.Target, new object[] { websocket, param });
            #endif
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
            #if NETCORE
                m_ExecutorAction.DynamicInvoke(new object[] { websocket, token, param });
            #else                
                m_ExecutorAction.Method.Invoke(m_ExecutorAction.Target, new object[] { websocket, token, param });
            #endif
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
            #if NETCORE
                m_ExecutorAction.DynamicInvoke(new object[] { websocket, param, m_State });
            #else                
                m_ExecutorAction.Method.Invoke(m_ExecutorAction.Target, new object[] { websocket, param, m_State });
            #endif
        }
    }
}
