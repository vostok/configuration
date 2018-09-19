using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Xml;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.SettingsTree;
using Vostok.Configuration.SettingsTree.Mutable;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Xml converter to <see cref="ISettingsNode"/> tree from string
    /// </summary>
    public class XmlStringSource : IConfigurationSource
    {
        private readonly string xml;
        private readonly TaskSource taskSource;
        private volatile bool neverParsed;
        private (ISettingsNode settings, Exception error) currentSettings;
        private XmlDocument doc;

        /// <summary>
        /// <para>Creates a <see cref="XmlStringSource"/> instance using given string in <paramref name="xml"/> parameter</para>
        /// <para>Parsing is here.</para>
        /// </summary>
        /// <param name="xml">ini data in string</param>
        /// <exception cref="Exception">Ini has wrong format</exception>
        public XmlStringSource(string xml)
        {
            this.xml = xml;
            taskSource = new TaskSource();
            neverParsed = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously parsed <see cref="ISettingsNode"/> tree.
        /// </summary>
        public ISettingsNode Get() => taskSource.Get(Observe()).settings;

        private ISettingsNode ParseXml()
        {
            doc = new XmlDocument();
            doc.LoadXml(xml);
            var root = doc.DocumentElement;
            if (root == null) return null;

            var res = new UniversalNode(null as string, "root");
            res.Add(root.Name, ParseElement(root, root.Name));

            return res.ChildrenDict.Any() ? (ObjectNode) res : null;
        }

        private UniversalNode ParseElement(XmlElement element, string name)
        {
            if (!element.HasChildNodes && !element.HasAttributes)
                return new UniversalNode(element.InnerText, name);

            var nodeList = new List<XmlNode>(element.ChildNodes.Count);
            foreach (XmlNode node in element.ChildNodes)
                nodeList.Add(node);
            foreach (XmlAttribute attribute in element.Attributes)
                if (nodeList.All(n => n.Name != attribute.Name))
                {
                    var elem = doc.CreateElement(attribute.Name);
                    elem.InnerText = attribute.Value;
                    nodeList.Add(elem);
                }

            if (!nodeList.OfType<XmlElement>().Any())
                return new UniversalNode(element.InnerText, name);

            var lookup = nodeList.Cast<XmlElement>().ToLookup(l => l.Name);
            var res = new UniversalNode(null as string, name);
            foreach (var elements in lookup)
            {
                var elem = elements.First();
                if (elements.Count() == 1)
                    res.Add(elem.Name, ParseElement(elem, elem.Name));
                else
                {
                    var array = new UniversalNode(null as string, elem.Name);
                    res.Add(elem.Name, array);
                    var i = 0;
                    foreach (var node in elements)
                        array.Add(ParseElement(node, i++.ToString()));
                }
            }

            return res;
        }

        public IObservable<(ISettingsNode settings, Exception error)> Observe()
        {
            if (neverParsed)
                try
                {
                    currentSettings = string.IsNullOrWhiteSpace(xml) ? (null, null) : (ParseXml(), null as Exception);
                    neverParsed = false;
                }
                catch (Exception e)
                {
                    return Observable.Throw<(ISettingsNode, Exception)>(e);
                }

            return Observable.Return(currentSettings);
        }
    }
}