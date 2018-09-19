using System;
using System.Linq;
using System.Reactive.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.SettingsTree;
using Vostok.Configuration.SettingsTree.Mutable;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Ini converter to <see cref="ISettingsNode"/> tree from string
    /// </summary>
    public class IniStringSource : IConfigurationSource
    {
        private readonly string ini;
        private readonly bool allowMultiLevelValues;
        private readonly TaskSource taskSource;
        private volatile bool neverParsed;
        private (ISettingsNode settings, Exception error) currentSettings;

        /// <summary>
        /// <para>Creates a <see cref="IniStringSource"/> instance using given string in <paramref name="ini"/> parameter</para>
        /// <para>Parsing is here.</para>
        /// </summary>
        /// <param name="ini">ini data in string</param>
        /// <param name="allowMultiLevelValues">Allow interpret point divided values as fields of inner objects</param>
        /// <exception cref="Exception">Ini has wrong format</exception>
        public IniStringSource(string ini, bool allowMultiLevelValues = true)
        {
            this.ini = ini;
            this.allowMultiLevelValues = allowMultiLevelValues;
            taskSource = new TaskSource();
            neverParsed = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously parsed <see cref="ISettingsNode"/> tree.
        /// </summary>
        public ISettingsNode Get() => taskSource.Get(Observe()).settings;

        private ISettingsNode ParseIni(string text, string name)
        {
            var res = new UniversalNode(name);
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

            return res.ChildrenDict.Any() ? (ObjectNode) res : null;
        }

        private UniversalNode ParseSection(string section, UniversalNode settings, int currentLine)
        {
            section = section.Replace(" ", "");

            if (settings[section] != null)
                throw new FormatException($"{nameof(IniStringSource)}: wrong ini file ({currentLine}): section \"{section}\" already exists");
            var res = new UniversalNode(section);
            settings.Add(section, res);
            return res;
        }

        private void ParsePair(string key, string value, UniversalNode settings, int currentLine)
        {
            var keys = allowMultiLevelValues ? key.Replace(" ", "").Split('.') : new[] {key.Replace(" ", "")};
            var isObj = false;
            var obj = settings;
            for (var i = 0; i < keys.Length; i++)
            {
                if (i == keys.Length - 1)
                {
                    if (obj[keys[i]] != null)
                    {
                        var child = (UniversalNode) obj[keys[i]];
                        if (child.Value != null)
                            throw new FormatException($"{nameof(IniStringSource)}: wrong ini file ({currentLine}): key \"{keys[i]}\" with value \"{child.Value}\" already exists");
                        child.Value = value;
                    }
                    else
                        obj.Add(keys[i], new UniversalNode(value, keys[i]));
                }
                else if (obj[keys[i]] == null)
                {
                    var newObj = new UniversalNode(keys[i]);
                    obj.Add(keys[i], newObj);
                    obj = newObj;
                }
                else
                    obj = (UniversalNode) obj[keys[i]];

                isObj = !isObj;
            }
        }

        public IObservable<(ISettingsNode settings, Exception error)> Observe()
        {
            if (neverParsed)
                try
                {
                    currentSettings = string.IsNullOrWhiteSpace(ini) ? (null, null) : (ParseIni(ini, "root"), null as Exception);
                    neverParsed = false;
                }
                catch (Exception e)
                {
                    return Observable.Throw<(ISettingsNode settings, Exception error)>(e);
                }

            return Observable.Return(currentSettings);
        }
    }
}