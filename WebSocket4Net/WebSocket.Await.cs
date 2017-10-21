using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocket4Net
{
    public partial class WebSocket
    {
        private TaskCompletionSource<bool> m_OpenTaskSrc;
        private TaskCompletionSource<bool> m_CloseTaskSrc;
         
        public async Task<bool> OpenAsync()
        {
            var openTaskSrc = m_OpenTaskSrc;

            if (openTaskSrc != null)
                return await openTaskSrc.Task;

            openTaskSrc = m_OpenTaskSrc = new TaskCompletionSource<bool>();
            Open();
            return await openTaskSrc.Task;
        }

        public async Task<bool> CloseAsync()
        {
            var closeTaskSrc = m_CloseTaskSrc;

            if (closeTaskSrc != null)
                return await closeTaskSrc.Task;

            closeTaskSrc = m_CloseTaskSrc = new TaskCompletionSource<bool>();
            Close();
            return await closeTaskSrc.Task;
        }

        private void FinishOpenTask()
        {
            m_OpenTaskSrc?.SetResult(this.StateCode == WebSocketStateConst.Open);
            m_OpenTaskSrc = null;
        }

        private void FinishCloseTask()
        {
            m_CloseTaskSrc?.SetResult(this.StateCode == WebSocketStateConst.Closed);
            m_CloseTaskSrc = null;
        }

        partial void OnInternalOpened()
        {
            FinishOpenTask();
        }

        partial void OnInternalClosed()
        {
            FinishOpenTask();
            FinishCloseTask();
        }

        partial void OnInternalError()
        {
            FinishOpenTask();
            FinishCloseTask();
        }
    }
}
