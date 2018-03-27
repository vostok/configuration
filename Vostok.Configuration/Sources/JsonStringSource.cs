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
        
        public RawSettings Get() => 
            string.IsNullOrWhiteSpace(json) ? null : ParseJson(JObject.Parse(json));

        public IObservable<RawSettings> Observe() => 
            Observable.Empty<RawSettings>();

        private RawSettings ParseJson(JObject jObject)
        {
            Dictionary<string, RawSettings> dict = null;
            if (jObject.Count > 0)
                dict = new Dictionary<string, RawSettings>();

            foreach (var token in jObject)
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

        private RawSettings ParseJson(JArray jArray)
        {
            List<RawSettings> list = null;
            if (jArray.Count > 0)
                list = new List<RawSettings>(jArray.Count);

            foreach (var item in jArray)
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