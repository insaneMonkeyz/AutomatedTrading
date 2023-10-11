using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Tools.Serialization
{
    public static class JsonHelper
    {
        public static T? SafeExtractObject<T>(this JObject? obj)
        {
            if (obj is not null && obj.TryGetValue(nameof(T), out JToken token))
            {
                try
                {
                    return token.ToObject<T>();
                }
                catch (Exception)
                {
                    return default;
                }
            }

            return default;
        }
    }
}
