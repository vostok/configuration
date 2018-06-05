using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Vostok.Configuration.Extensions;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Ini converter to <see cref="IRawSettings"/> tree from string
    /// </summary>
    public class IniStringSource : IConfigurationSource
    {
        private readonly string ini;
        private readonly TaskSource taskSource;
        private IRawSettings currentSettings;
        private bool neverParsed;

        /// <summary>
        /// <para>Creates a <see cref="JsonFileSource"/> instance using given string in <paramref name="ini"/> parameter</para>
        /// <para>Parsing is here.</para>
        /// </summary>
        /// <param name="ini">ini data in string</param>
        /// <exception cref="Exception">Ini has wrong format</exception>
        public IniStringSource(string ini)
        {
            this.ini = ini;
            taskSource = new TaskSource();
            neverParsed = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously parsed <see cref="IRawSettings"/> tree.
        /// </summary>
        public IRawSettings Get() => taskSource.Get(Observe());

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to <see cref="RawSettings"/> tree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        public IObservable<IRawSettings> Observe()
        {
            if (neverParsed)
            {
                neverParsed = false;
                currentSettings = string.IsNullOrWhiteSpace(ini) ? null : ParseIni(ini, "root");
            }

            return Observable.Create<IRawSettings>(
                observer =>
                {
                    observer.OnNext(currentSettings);
                    return Disposable.Empty;
                });
        }

        public void Dispose()
        {
        }

        private static IRawSettings ParseIni(string text, string name)
        {
            var res = new RawSettingsEditable(name);
            var section = res;
            var currentLine = -1;

            var lines = text
                .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l));
            foreach (var line in lines)
            {
                currentLine++;
                if (line.StartsWith("#") || line.StartsWith(";"))
                    continue;
                if (line.StartsWith("[") && line.EndsWith("]") && line.Length > 2 && !line.Contains(" "))
                    section = ParseSection(line.Substring(1, line.Length - 2), res, currentLine);
                else
                {
                    var pair = line.Split(new[] {'='}, 2).Select(s => s.Trim()).ToArray();
                    if (pair.Length == 2 && pair[0].Length > 0 && !pair[0].Contains(" "))
                        ParsePair(pair[0], pair[1], section, currentLine);
                    else
                        throw new FormatException($"{nameof(IniStringSource)}: wrong ini file ({currentLine}): line \"{line}\"");
                }
            }

            if (res.Children.Count == 0 && res.Value == null)
                return null;
            return (RawSettings)res;
        }

        private static RawSettingsEditable ParseSection(string section, RawSettingsEditable settings, int currentLine)
        {
            section = section.Replace(" ", "");

            if (settings.Children[section] != null)
                throw new FormatException($"{nameof(IniStringSource)}: wrong ini file ({currentLine}): section \"{section}\" already exists");
            var res = new RawSettingsEditable(section);
            settings.Children.Add(section, res);
            return res;
        }

        private static void ParsePair(string key, string value, RawSettingsEditable settings, int currentLine)
        {
            var keys = key.Replace(" ", "").Split('.');
            var isObj = false;
            var obj = settings;
            for (var i = 0; i < keys.Length; i++)
            {
                if (i == keys.Length - 1)
                {
                    if (obj.Children[keys[i]] != null)
                    {
                        var child = (RawSettingsEditable)obj.Children[keys[i]];
                        if (child.Value != null)
                            throw new FormatException($"{nameof(IniStringSource)}: wrong ini file ({currentLine}): key \"{keys[i]}\" with value \"{child.Value}\" already exists");
                        else
                            child.Value = value;
                    }
                    else
                        obj.Children.Add(keys[i], new RawSettingsEditable(value, keys[i]));
                }
                else if (obj.Children[keys[i]] == null)
                {
                    var newObj = new RawSettingsEditable(keys[i]);
                    obj.Children.Add(keys[i], newObj);
                    obj = newObj;
                }
                else
                    obj = (RawSettingsEditable)obj.Children[keys[i]];

                isObj = !isObj;
            }
        }

        private class RawSettingsEditable
        {
            public RawSettingsEditable(string name = "")
            {
                Children = new OrderedDictionary();
                Name = name;
            }

            public RawSettingsEditable(string value, string name = "")
            {
                Children = new OrderedDictionary();
                Value = value;
                Name = name;
            }

            public string Value { get; set; }
            public IOrderedDictionary Children { get; }

            private string Name { get; }

            public static explicit operator RawSettings(RawSettingsEditable settings)
            {
                IOrderedDictionary dict;
                if (settings.Children.Count == 0)
                    dict = null;
                else
                    dict = settings.Children.Cast<DictionaryEntry>()
                        .ToOrderedDictionary(p => p.Key, p => (RawSettings)(RawSettingsEditable)p.Value);

                return new RawSettings(dict, settings.Name, settings.Value);
            }
        }
    }
}