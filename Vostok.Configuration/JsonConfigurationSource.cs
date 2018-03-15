using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Vostok.Commons.ThreadManagment;

namespace Vostok.Configuration
{
    public class JsonConfigurationSource: IConfigurationSource
    {
        private readonly string filePath;
        private readonly FileSystemWatcher watcher;
        private readonly List<IObserver<RawSettings>> observers;
        private readonly object sync;
        private RawSettings current;

        public JsonConfigurationSource(string filePath)
        {
            this.filePath = filePath;
            var path = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(path))
                path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            watcher = new FileSystemWatcher(path, Path.GetFileName(filePath));
            observers = new List<IObserver<RawSettings>>();
            sync = new object();
            
            ThreadRunner.Run(WatchFile, null);
        }

        public RawSettings Get()
        {
            var obj = JObject.Parse(File.ReadAllText(filePath));
            return JsonParser(obj);
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

        private void WatchFile()
        {
            const int reloadPeriod = 10000;

            while (true)
            {
                watcher.WaitForChanged(WatcherChangeTypes.All, reloadPeriod);

                if (File.Exists(filePath))
                {
                    using (var reader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                        current = JsonParser(JObject.Parse(reader.ReadToEnd()));

                    lock (sync)
                    {
                        foreach (var observer in observers)
                            observer.OnNext(current);
                    }
                }
            }
        }

        public IObservable<RawSettings> Observe()
        {
            return Observable.Create<RawSettings>(observer =>
            {
                lock (sync)
                {
                    observers.Add(observer);

                    if (current != null)
                        observer.OnNext(current);
                }

                return Disposable.Create(() =>
                {
                    lock (sync)
                    {
                        observers.Remove(observer);
                    }
                });
            });
        }
    }
}