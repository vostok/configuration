using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Vostok.Configuration.Sources
{
    public enum SerializeOption
    {
        Short,
        Readable,
    }

    public static class JsonSerializer
    {
        public static string Serialize(object obj, SerializeOption serializeOption = SerializeOption.Short)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer
            {
                NullValueHandling = NullValueHandling.Include,
                Formatting = serializeOption == SerializeOption.Short ? Formatting.None : Formatting.Indented,
            };
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, obj);
                return sb.ToString();
            }
        }
    }
}