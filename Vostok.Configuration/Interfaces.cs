using System;
using System.Collections.Generic;

namespace Vostok.Configuration
{
    // TODO(krait): ICP decides whether to throw on invalid configs or ignore errors
    /// <summary>
    /// In tests you substitute this one.
    /// Using a per-project extension method you can get rid of generic type on Get.
    /// </summary>
    public interface IConfigurationProvider
    {
        TSettings Get<TSettings>();

        // TODO(krait): take ISettings?
        TSettings Get<TSettings>(IConfigurationSource source);

        IObservable<TSettings> Observe<TSettings>();

        IObservable<TSettings> Observe<TSettings>(IConfigurationSource source);
    }
    
    public interface IConfigurationSource
    {
        RawSettings Get();

        IObservable<RawSettings> Observe();
    }

    /// <summary>
    /// Not static to be configured with custom type parsers.
    /// </summary>
    public interface ISettingsBinder
    {
        // TODO(krait): throws on error
        TSettings Bind<TSettings>(RawSettings rawSettings);

        //RawSettings Unbind<TSettings>(TSettings settings);
    }

    public sealed class RawSettings
    {
        public RawSettings() {}

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
