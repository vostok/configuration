using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json.Linq;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Json converter to <see cref="RawSettings"/> tree from string
    /// </summary>
    public class JsonStringSource : IConfigurationSource
    {
        private readonly RawSettings currentSettings;
        private readonly BehaviorSubject<RawSettings> observers;

        /// <summary>
        /// <para>Creates a <see cref="JsonFileSource"/> instance using given string in <paramref name="json"/> parameter</para>
        /// <para>Parsing is here.</para>
        /// </summary>
        /// <param name="json">Json data in string</param>
        /// <exception cref="Exception">Json has wrong format</exception>
        public JsonStringSource(string json)
        {
            observers = new BehaviorSubject<RawSettings>(currentSettings);
            currentSettings = string.IsNullOrWhiteSpace(json) ? null : ParseJson(JObject.Parse(json));
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously parsed <see cref="RawSettings"/> tree.
        /// </summary>
        public RawSettings Get() => currentSettings;

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to <see cref="RawSettings"/> tree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        public IObservable<RawSettings> Observe() =>
            Observable.Create<RawSettings>(observer =>
                observers.Select(settings => currentSettings).Subscribe(observer));

        public void Dispose()
        {
            observers.Dispose();
        }

        private RawSettings ParseJson(JObject jObject)
        {
            Dictionary<string, RawSettings> dict = null;
            if (jObject.Count > 0)
                dict = new Dictionary<string, RawSettings>();

            foreach (var token in jObject)
            {
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
            }

            return new RawSettings(dict);
        }

        private RawSettings ParseJson(JArray jArray)
        {
            List<RawSettings> list = null;
            if (jArray.Count > 0)
                list = new List<RawSettings>(jArray.Count);

            foreach (var item in jArray)
            {
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
            }

            return new RawSettings(list);
        }
    }
}