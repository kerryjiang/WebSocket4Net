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
         
        public async Task<bool> OpenAsync()
        {
            if (m_OpenTaskSrc != null)
                return await m_OpenTaskSrc.Task;

            m_OpenTaskSrc = new TaskCompletionSource<bool>();
            this.Opened += OnOpenCompleted;
            
            Open();
            return await m_OpenTaskSrc.Task;
        }

        private void OnOpenCompleted(object sender, EventArgs e)
        {
            this.Opened -= OnOpenCompleted;
            m_OpenTaskSrc?.SetResult(this.StateCode == WebSocketStateConst.Open);
            m_OpenTaskSrc = null;
        }
    }
}
