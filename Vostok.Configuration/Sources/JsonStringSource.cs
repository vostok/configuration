using System;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json.Linq;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Json converter to <see cref="IRawSettings"/> tree from string
    /// </summary>
    public class JsonStringSource : IConfigurationSource
    {
        private readonly IRawSettings currentSettings;
        private readonly BehaviorSubject<IRawSettings> observers;

        /// <summary>
        /// <para>Creates a <see cref="JsonFileSource"/> instance using given string in <paramref name="json"/> parameter</para>
        /// <para>Parsing is here.</para>
        /// </summary>
        /// <param name="json">Json data in string</param>
        /// <exception cref="Exception">Json has wrong format</exception>
        public JsonStringSource(string json)
        {
            observers = new BehaviorSubject<IRawSettings>(currentSettings);
            currentSettings = string.IsNullOrWhiteSpace(json) ? null : ParseJson(JObject.Parse(json), "root");
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously parsed <see cref="IRawSettings"/> tree.
        /// </summary>
        public IRawSettings Get() => currentSettings;

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to <see cref="IRawSettings"/> tree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        public IObservable<IRawSettings> Observe() =>
            Observable.Create<IRawSettings>(
                observer =>
                    observers.Select(settings => currentSettings).Subscribe(observer));

        public void Dispose()
        {
            observers.Dispose();
        }

        private IRawSettings ParseJson(JObject jObject, string tokenKey)
        {
            if (jObject.Count <= 0)
                return new RawSettings((OrderedDictionary)null, tokenKey);

            var dict = new OrderedDictionary();
            foreach (var token in jObject)
                switch (token.Value.Type)
                {
                    case JTokenType.Null:
                        dict.Add(token.Key, new RawSettings(null, token.Key));
                        break;
                    case JTokenType.Object:
                        dict.Add(token.Key, ParseJson((JObject)token.Value, token.Key));
                        break;
                    case JTokenType.Array:
                        dict.Add(token.Key, ParseJson((JArray)token.Value, token.Key));
                        break;
                    default:
                        dict.Add(token.Key, new RawSettings(token.Value.ToString(), token.Key));
                        break;
                }

            return new RawSettings(dict, tokenKey);
        }

        private RawSettings ParseJson(JArray jArray, string tokenKey)
        {
            if (jArray.Count <= 0)
                return new RawSettings((OrderedDictionary)null, tokenKey);

            var dict = new OrderedDictionary();
            var i = 0;
            foreach (var item in jArray)
            {
                object obj;
                switch (item.Type)
                {
                    case JTokenType.Null:
                        obj = new RawSettings(null, i.ToString());
                        break;
                    case JTokenType.Object:
                        obj = ParseJson((JObject)item, i.ToString());
                        break;
                    case JTokenType.Array:
                        obj = ParseJson((JArray)item, i.ToString());
                        break;
                    default:
                        obj = new RawSettings(item.ToString(), i.ToString());
                        break;
                }

                dict.Add(i++.ToString(), obj);
            }

            return new RawSettings(dict, tokenKey);
        }
    }
}