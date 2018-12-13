using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Tests
{
    public class TreeConstructionSet
    {
        public static ValueNode Value(string name, string value) => new ValueNode(name, value);

        public static ValueNode Value(string value) => new ValueNode(value);

        public static ArrayNode Array(string name, params ISettingsNode[] children) => new ArrayNode(name, children);

        public static ArrayNode Array(params ISettingsNode[] children) => new ArrayNode(children);

        public static ArrayNode Array(string name, params string[] children) => new ArrayNode(name, children.Select(e => new ValueNode(e)).ToArray());

        public static ArrayNode Array(params string[] children) => new ArrayNode(children.Select(e => new ValueNode(e)).ToArray());

        public static ObjectNode Object(string name, params ISettingsNode[] children) => new ObjectNode(name, children);

        public static ObjectNode Object(params ISettingsNode[] children) => new ObjectNode(children);

        public static ObjectNode Object(string name, params (string key, string value)[] children) => new ObjectNode(name, children.Select(e => new ValueNode(e.key, e.value) as ISettingsNode).ToArray());

        public static ObjectNode Object(params (string key, string value)[] children) => new ObjectNode(children.Select(e => new ValueNode(e.key, e.value) as ISettingsNode).ToArray());
    }
}