using System.Collections.Generic;

namespace Vostok.Configuration
{
    public sealed class RawSettings
    {
        public RawSettings() { }

        public RawSettings(string value)
        {
            Value = value;
        }

        public RawSettings(IDictionary<string, RawSettings> children, string value = null)
        {
            ChildrenByKey = children;
            Value = value;
        }
        public RawSettings(IEnumerable<RawSettings> children, string value = null)
        {
            Children = children;
            Value = value;
        }

        public void CreateDictionary()
        {
            ChildrenByKey = new Dictionary<string, RawSettings>();
        }

        public void CreateList()
        {
            Children = new List<RawSettings>();
        }

        public string Value { get; }

        public IDictionary<string, RawSettings> ChildrenByKey { get; private set; }

        public IEnumerable<RawSettings> Children { get; private set; }
    }

    // TODO(krait): validator (+custom specified by attribute), example generator (+config saving)
    // TODO(krait): attributes list
}

/*Пример:
class MySettings
{
    private int Port;
    private string ConnString;

    class InternalSettings
    {
        private string SettingA;
    }

    private InternalSettings Internal;

    private List<int> Ints;
}
 
new RawSettings(new Dictionary<string, RawSettings>
{
    {"Port", new RawSettings("42")},
    {"ConnString", new RawSettings("")},
    {
        "Internal", new RawSettings(new Dictionary<string, RawSettings>
        {
            {"SettingA", new RawSettings("A")}
        })
    },
    {
        "Ints", new RawSettings(new List<RawSettings>
        {
            new RawSettings("1"),
            new RawSettings("2"),
            new RawSettings("3")
        })
    }
});*/