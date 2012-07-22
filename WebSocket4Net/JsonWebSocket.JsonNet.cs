using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

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
            return JsonConvert.SerializeObject(target);
        }

        /// <summary>
        /// Deserializes the json string to object.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <param name="type">The type of the target object.</param>
        /// <returns></returns>
        protected virtual object DeserializeObject(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json,  type);
        }
    }
}
