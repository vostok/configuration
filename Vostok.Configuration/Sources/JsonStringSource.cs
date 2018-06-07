﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Newtonsoft.Json.Linq;
using Vostok.Configuration.SettingsTree;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Json converter to <see cref="ISettingsNode"/> tree from string
    /// </summary>
    public class JsonStringSource : IConfigurationSource
    {
        private readonly string json;
        private readonly TaskSource taskSource;
        private ISettingsNode currentSettings;

        private bool neverParsed;

        /// <summary>
        /// <para>Creates a <see cref="JsonFileSource"/> instance using given string in <paramref name="json"/> parameter</para>
        /// <para>Parsing is here.</para>
        /// </summary>
        /// <param name="json">Json data in string</param>
        /// <exception cref="Exception">Json has wrong format</exception>
        public JsonStringSource(string json)
        {
            this.json = json;
            neverParsed = true;
            taskSource = new TaskSource();
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously parsed <see cref="ISettingsNode"/> tree.
        /// </summary>
        public ISettingsNode Get() => taskSource.Get(Observe());

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to <see cref="ISettingsNode"/> tree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        public IObservable<ISettingsNode> Observe()
        {
            if (neverParsed)
            {
                neverParsed = false;
                currentSettings = string.IsNullOrWhiteSpace(json) ? null : ParseJson(JObject.Parse(json), "root");
            }

            return Observable.Create<ISettingsNode>(
                observer =>
                {
                    observer.OnNext(currentSettings);
                    return Disposable.Empty;
                });
        }

        public void Dispose()
        {
        }

        private ISettingsNode ParseJson(JObject jObject, string tokenKey)
        {
            if (jObject.Count <= 0)
                return new ObjectNode(tokenKey);

            var dict = new SortedDictionary<string, ISettingsNode>(new ChildrenKeysComparer());
            foreach (var token in jObject)
                switch (token.Value.Type)
                {
                    case JTokenType.Null:
                        dict.Add(token.Key, new ValueNode(null, token.Key));
                        break;
                    case JTokenType.Object:
                        dict.Add(token.Key, ParseJson((JObject)token.Value, token.Key));
                        break;
                    case JTokenType.Array:
                        dict.Add(token.Key, ParseJson((JArray)token.Value, token.Key));
                        break;
                    default:
                        dict.Add(token.Key, new ValueNode(token.Value.ToString(), token.Key));
                        break;
                }

            return new ObjectNode(dict, tokenKey);
        }

        private ISettingsNode ParseJson(JArray jArray, string tokenKey)
        {
            if (jArray.Count <= 0)
                return new ArrayNode(tokenKey);

            var list = new List<ISettingsNode>(jArray.Count);
            var i = 0;
            foreach (var item in jArray)
            {
                ISettingsNode obj;
                switch (item.Type)
                {
                    case JTokenType.Null:
                        obj = new ValueNode(null);
                        break;
                    case JTokenType.Object:
                        obj = ParseJson((JObject)item, i.ToString());
                        break;
                    case JTokenType.Array:
                        obj = ParseJson((JArray)item, i.ToString());
                        break;
                    default:
                        obj = new ValueNode(item.ToString());
                        break;
                }

                list.Add(obj);
            }

            return new ArrayNode(list, tokenKey);
        }
    }
}