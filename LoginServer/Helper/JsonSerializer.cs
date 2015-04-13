using System;
using Newtonsoft.Json;

namespace LoginServer.Helper
{
    public static class JsonSerializer
    {
        public static string toJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T fromJson<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}