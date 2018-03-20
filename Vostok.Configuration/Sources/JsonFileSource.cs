using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using Newtonsoft.Json.Linq;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Json converter to RawSettings tree
    /// </summary>
    public class JsonFileSource : IConfigurationSource
    {
        private readonly string filePath;
        private readonly SettingsFileWatcher fileWatcher;

        /// <summary>
        /// Creating json converter
        /// </summary>
        /// <param name="filePath">File name with settings</param>
        /// <param name="observePeriod">Observe period in ms (min 100)</param>
        public JsonFileSource(string filePath, int observePeriod = 10000)
        {
            this.filePath = filePath;
            fileWatcher = new SettingsFileWatcher(filePath, this, observePeriod);
        }

        public RawSettings Get()
        {
            if (!File.Exists(filePath)) return null;
            var obj = JObject.Parse(File.ReadAllText(filePath));
            return JsonParser(obj);
        }

        public IObservable<RawSettings> Observe()
        {
            return Observable.Create<RawSettings>(observer =>
            {
                fileWatcher.AddObserver(observer);
                return fileWatcher.GetDisposable(observer);
            });
        }

        private RawSettings JsonParser(JObject obj)
        {
            var res = new RawSettings();
            if (obj.Count > 0)
                res.CreateDictionary();

            foreach (var token in obj)
                switch (token.Value.Type)
                {
                    case JTokenType.Null:
                        res.ChildrenByKey.Add(token.Key, new RawSettings(null));
                        break;
                    case JTokenType.Object:
                        res.ChildrenByKey.Add(token.Key, JsonParser((JObject)token.Value));
                        break;
                    case JTokenType.Array:
                        res.ChildrenByKey.Add(token.Key, JsonParser((JArray)token.Value));
                        break;
                    default:
                        res.ChildrenByKey.Add(token.Key, new RawSettings(token.Value.ToString()));
                        break;
                }
            return res;
        }

        private RawSettings JsonParser(JArray arr)
        {
            var res = new RawSettings();
            if (arr.Count > 0)
                res.CreateList();

            var list = (List<RawSettings>) res.Children;
            foreach (var item in arr)
                switch (item.Type)
                {
                    case JTokenType.Null:
                        list.Add(new RawSettings(null));
                        break;
                    case JTokenType.Object:
                        list.Add(JsonParser((JObject)item));
                        break;
                    case JTokenType.Array:
                        list.Add(JsonParser((JArray)item));
                        break;
                    default:
                        list.Add(new RawSettings(item.ToString()));
                        break;
                }

            return res;
        }
    }
}