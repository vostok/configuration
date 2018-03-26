using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Newtonsoft.Json.Linq;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Json converter to RawSettings tree from string
    /// </summary>
    public class JsonStringSource : IConfigurationSource
    {
        private readonly string json;

        /// <summary>
        /// Creating json converter
        /// </summary>
        /// <param name="json">Json file data in string</param>
        public JsonStringSource(string json)
        {
            this.json = json;
        }

        public RawSettings Get()
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            var obj = JObject.Parse(json);
            return ParseJson(obj);
        }

        public IObservable<RawSettings> Observe() => 
            Observable.Empty<RawSettings>();

        private RawSettings ParseJson(JObject obj)
        {
            Dictionary<string, RawSettings> dict = null;
            if (obj.Count > 0)
                dict = new Dictionary<string, RawSettings>();

            foreach (var token in obj)
                switch (token.Value.Type)
                {
                    case JTokenType.Null:
                        dict.Add(token.Key, new RawSettings(null));
                        break;
                    case JTokenType.Object:
                        dict.Add(token.Key, ParseJson((JObject)token.Value));
                        break;
                    case JTokenType.Array:
                        dict.Add(token.Key, ParseJson((JArray)token.Value));
                        break;
                    default:
                        dict.Add(token.Key, new RawSettings(token.Value.ToString()));
                        break;
                }
            return new RawSettings(dict);
        }

        private RawSettings ParseJson(JArray arr)
        {
            List<RawSettings> list = null;
            if (arr.Count > 0)
                list = new List<RawSettings>(arr.Count);

            foreach (var item in arr)
                switch (item.Type)
                {
                    case JTokenType.Null:
                        list.Add(new RawSettings(null));
                        break;
                    case JTokenType.Object:
                        list.Add(ParseJson((JObject)item));
                        break;
                    case JTokenType.Array:
                        list.Add(ParseJson((JArray)item));
                        break;
                    default:
                        list.Add(new RawSettings(item.ToString()));
                        break;
                }

            return new RawSettings(list);
        }

        public void Dispose() { }
    }
}