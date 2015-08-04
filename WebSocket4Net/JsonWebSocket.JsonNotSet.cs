using System;
using System.IO;
using System.Text;

namespace WebSocket4Net
{
    public partial class JsonWebSocket
    {
        private Func<object, string> m_JsonSerialzier;

        private Func<string, Type, object> m_JsonDeserialzier;

        /// <summary>
        /// Configure json deserializer
        /// </summary>
        /// <param name="serialzier">the json serialzing method</param>
        /// <param name="deserializer">the json deserializing method</param>
        public void ConfigJsonSerialzier(Func<object, string> serialzier, Func<string, Type, object> deserializer)
        {
            m_JsonSerialzier = serialzier;
            m_JsonDeserialzier = deserializer;
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="target">The target object is being serialized.</param>
        /// <returns></returns>
        protected virtual string SerializeObject(object target)
        {
            var serializer = m_JsonSerialzier;

            if (serializer == null)
                throw new Exception("Json serialzier is not configured yet.");

            return serializer(target);
        }

        /// <summary>
        /// Deserializes the json string to object.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <param name="type">The type of the target object.</param>
        /// <returns></returns>
        protected virtual object DeserializeObject(string json, Type type)
        {
            var deserializer = m_JsonDeserialzier;

            if (deserializer == null)
                throw new Exception("Json serialzier is not configured yet.");

            return deserializer(json, type);
        }
    }
}
