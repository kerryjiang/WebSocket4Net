using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;

namespace WebSocket4Net
{
    public partial class JsonWebSocket
    {
        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="target">The target object is being serialized.</param>
        /// <returns></returns>
        protected virtual string SerializeObject(object target)
        {
            var serializer = new DataContractJsonSerializer(target.GetType());

            string result;
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, target);
                result = Encoding.UTF8.GetString(ms.ToArray());
            };

            return result;
        }

        /// <summary>
        /// Deserializes the json string to object.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <param name="type">The type of the target object.</param>
        /// <returns></returns>
        protected virtual object DeserializeObject(string json, Type type)
        {
            var serializer = new DataContractJsonSerializer(type);

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return serializer.ReadObject(ms);
            };
        }
    }
}
