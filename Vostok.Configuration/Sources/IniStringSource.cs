using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Json converter to RawSettings tree from string
    /// </summary>
    public class IniStringSource : IConfigurationSource
    {
        private readonly string ini;
        private int currentLine;

        /// <summary>
        /// Creating ini converter
        /// </summary>
        /// <param name="ini">Ini file data in string</param>
        public IniStringSource(string ini)
        {
            this.ini = ini;
            currentLine = -1;
        }

        public RawSettings Get() =>
            string.IsNullOrWhiteSpace(ini) ? null : ParseIni(ini);

        public IObservable<RawSettings> Observe() => 
            Observable.Empty<RawSettings>();

        private RawSettings ParseIni(string text)
        {
            var res = new Rs();
            var section = res;

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
                    section = ParseSection(line.Substring(1, line.Length - 2), res);
                else
                {
                    var pair = line.Split(new[] {'='}, 2).Select(s => s.Trim()).ToArray();
                    if (pair.Length == 2 && pair[0].Length > 0 && !pair[0].Contains(" "))
                        ParsePair(pair[0], pair[1], section);
                    else
                        throw new FormatException($"Wrong ini file ({currentLine}): line \"{line}\"");
                }
            }
            currentLine = -1;

            if (res.Children.Count == 0 && res.ChildrenByKey.Count == 0 && res.Value == null)
                return null;
            return (RawSettings)Convert.ChangeType(res, typeof(RawSettings));
        }

        private Rs ParseSection(string section, Rs settings)
        {
            section = section.Replace(" ", "");

            if (settings.ChildrenByKey.ContainsKey(section))
                throw new FormatException($"Wrong ini file ({currentLine}): section \"{section}\" already exists");
            var res = new Rs();
            settings.ChildrenByKey.Add(section, res);
            return res;
        }

        private void ParsePair(string key, string value, Rs settings)
        {
            var keys = key.Replace(" ", "").Split('.');
            var isObj = false;
            var obj = settings;
            for (var i = 0; i < keys.Length; i++)
            {
                if (i == keys.Length - 1)
                {
                    if (obj.ChildrenByKey.ContainsKey(keys[i]))
                    {
                        var val = obj.ChildrenByKey[keys[i]].Value;
                        if (val != null)
                            throw new FormatException($"Wrong ini file ({currentLine}): key \"{keys[i]}\" with value \"{val}\" already exists");
                        else
                            obj.ChildrenByKey[keys[i]].Value = value;
                    }
                    else
                        obj.ChildrenByKey.Add(keys[i], new Rs(value));
                }
                else if (!obj.ChildrenByKey.ContainsKey(keys[i]))
                {
                    var newObj = new Rs();
                    obj.ChildrenByKey.Add(keys[i], newObj);
                    obj = newObj;
                }
                else
                {
                    obj = obj.ChildrenByKey[keys[i]];
                    if (obj.ChildrenByKey == null)
                        obj.ChildrenByKey = new Dictionary<string, Rs>();
                }
                isObj = !isObj;
            }
        }

        public void Dispose() { }

        // CR(krait): The name is too mysterious.
        private class Rs : IConvertible
        {
            public Rs()
            {
                ChildrenByKey = new Dictionary<string, Rs>();
                Children = new List<Rs>();
            }
            public Rs(string value)
            {
                Value = value;
            }

            // CR(krait): Unused constructors?
            public Rs(IDictionary<string, Rs> children, string value = null)
            {
                ChildrenByKey = children;
                Value = value;
            }
            public Rs(IList<Rs> children, string value = null)
            {
                Children = children;
                Value = value;
            }
            public Rs(IDictionary<string, Rs> childrenByKey, IList<Rs> children, string value = null)
            {
                ChildrenByKey = childrenByKey;
                Children = children;
                Value = value;
            }

            public string Value { get; set; }
            public IDictionary<string, Rs> ChildrenByKey { get; set; }
            public IList<Rs> Children { get; }

            // CR(krait): Why convertible? Could just override a type cast operator to RawSettings.
            #region Convertible, not implemented
            public TypeCode GetTypeCode()
            {
                throw new NotImplementedException();
            }

            public bool ToBoolean(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public byte ToByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public char ToChar(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public DateTime ToDateTime(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public decimal ToDecimal(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public double ToDouble(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public short ToInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public int ToInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public long ToInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public sbyte ToSByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public float ToSingle(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public string ToString(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public ushort ToUInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public uint ToUInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public ulong ToUInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            #endregion

            public object ToType(Type conversionType, IFormatProvider provider)
            {
                var dict = ChildrenByKey == null || ChildrenByKey.Count == 0
                    ? null
                    : ChildrenByKey?.ToDictionary(d => d.Key, d => (RawSettings)d.Value.ToType(typeof(RawSettings), null));
                var list = Children == null || Children.Count == 0
                    ? null
                    : Children?.Select(i => (RawSettings)i.ToType(typeof(RawSettings), null)).ToList();

                return new RawSettings(dict, list, Value);
            }
        }
    }
}