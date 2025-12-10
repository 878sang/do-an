using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;

namespace Do_an_1.Models
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        // Extension method để lấy string từ session (tương thích với các phiên bản ASP.NET Core)
        public static string GetSessionString(this ISession session, string key)
        {
            var value = session.Get(key);
            return value == null ? null : Encoding.UTF8.GetString(value);
        }
    }
}
