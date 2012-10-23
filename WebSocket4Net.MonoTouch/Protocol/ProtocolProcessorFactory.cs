using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace WebSocket4Net.Protocol
{
    class ProtocolProcessorFactory
    {
        private IProtocolProcessor[] m_OrderedProcessors;

        public ProtocolProcessorFactory(params IProtocolProcessor[] processors)
        {
            m_OrderedProcessors = processors.OrderByDescending(p => (int)p.Version).ToArray();
        }

        public IProtocolProcessor GetProcessorByVersion(WebSocketVersion version)
        {
            return m_OrderedProcessors.FirstOrDefault(p => p.Version == version);
        }

        public IProtocolProcessor GetPreferedProcessorFromAvialable(int[] versions)
        {
            foreach(var v in versions.OrderByDescending(i => i))
            {
                foreach (var n in m_OrderedProcessors)
                {
                    int versionValue = (int)n.Version;

                    if (versionValue < v)
                        break;

                    if (versionValue > v)
                        continue;

                    return n;
                }
            }

            return null;
        }
    }
}
