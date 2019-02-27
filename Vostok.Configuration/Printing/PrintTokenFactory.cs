using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Vostok.Commons.Collections;
using Vostok.Commons.Formatting;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Printing
{
    internal static class PrintTokenFactory
    {
        private const string NullValue = "<null>";
        private const string ErrorValue = "<error>";
        private const string EmptyValue = "<empty>";
        private const string SecretValue = "<secret>";
        private const string CyclicValue = "<cyclic>";

        [NotNull]
        public static IPrintToken Create([CanBeNull] object item)
            => CreateInternal(item, new HashSet<object>(ByReferenceEqualityComparer<object>.Instance));

        [NotNull]
        private static IPrintToken CreateInternal([CanBeNull] object item, [NotNull] HashSet<object> path)
        {
            if (item == null)
                return new ValueToken(NullValue);

            if (!path.Add(item))
                return new ValueToken(CyclicValue);

            try
            {
                var itemType = item.GetType();

                if (ToStringDetector.HasCustomToString(itemType))
                    return new ValueToken(item.ToString());

                if (DictionaryInspector.IsSimpleDictionary(itemType))
                {
                    var pairs = DictionaryInspector.EnumerateSimpleDictionary(item);
                    var tokens = pairs.Select(pair => new PropertyToken(pair.Item1, CreateInternal(pair.Item2, path))).ToArray();

                    return new ObjectToken(tokens);
                }

                if (item is IEnumerable sequence)
                {
                    if (!sequence.GetEnumerator().MoveNext())
                        return new ValueToken(EmptyValue);

                    var tokens = new List<IPrintToken>();

                    foreach (var element in sequence)
                        tokens.Add(CreateInternal(element, path));

                    return new SequenceToken(tokens);
                }

                var fieldsAndProperties = new List<PropertyToken>();

                foreach (var field in itemType.GetInstanceFields())
                    fieldsAndProperties.Add(ConstructProperty(field, () => CreateInternal(field.GetValue(item), path)));

                foreach (var property in itemType.GetInstanceProperties())
                    fieldsAndProperties.Add(ConstructProperty(property, () => CreateInternal(property.GetValue(item), path)));

                return new ObjectToken(fieldsAndProperties);
            }
            finally
            {
                path.Remove(item);
            }
        }

        private static PropertyToken ConstructProperty(MemberInfo member, Func<IPrintToken> getValue)
        {
            IPrintToken value;

            if (member.GetCustomAttribute<SecretAttribute>() == null)
            {
                try
                {
                    value = getValue();
                }
                catch
                {
                    value = new ValueToken(ErrorValue);
                }
            }
            else
            {
                value = new ValueToken(SecretValue);
            }

            return new PropertyToken(member.Name, value);
        }
    }
}