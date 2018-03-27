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
        // CR(krait): Nope, it should serialize to a string, not to a file. We usually log current settings. And some services have a special handler that returns the settings too.
        public static void Serialize(object obj, string filePath, SerializeOption serializeOption = SerializeOption.Short)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = serializeOption == SerializeOption.Short ? Formatting.None : Formatting.Indented,
            };
            using (var sw = new StreamWriter(filePath))
                using (JsonWriter writer = new JsonTextWriter(sw))
                    serializer.Serialize(writer, obj);
        }
    }
}