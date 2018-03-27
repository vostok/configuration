using System.IO;
using Newtonsoft.Json;

namespace Vostok.Configuration.Sources
{
    public enum SerializeOption
    {
        Short,
        Readable,
    }

    public class JsonSerializer
    {
        public static void Serialize(object obj, string filePath, SerializeOption serializeOption = SerializeOption.Short)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer
            {
                NullValueHandling = NullValueHandling.Include,
                Formatting = serializeOption == SerializeOption.Short ? Formatting.None : Formatting.Indented,
            };
            using (var sw = new StreamWriter(filePath))
                using (JsonWriter writer = new JsonTextWriter(sw))
                    serializer.Serialize(writer, obj);
        }
    }
}