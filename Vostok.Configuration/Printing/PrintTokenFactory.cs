using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Vostok.Commons.Collections;
using Vostok.Commons.Formatting;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Printing
{
    internal static class PrintTokenFactory
    {
        private const string NullValue = "<null>";
        private const string ErrorValue = "<error>";
        private const string SecretValue = "<secret>";
        private const string CyclicValue = "<cyclic>";
        private const string EmptySequenceValue = "[]";
        private const string EmptyDictionaryValue = "{}";

        private static readonly Dictionary<Type, Func<object, string>> CustomFormatters
            = new Dictionary<Type, Func<object, string>>
            {
                [typeof(Encoding)] = value => ((Encoding)value).WebName
            };

        [NotNull]
        public static IPrintToken Create([CanBeNull] object item, [NotNull] PrintSettings settings)
            => CreateInternal(item, new HashSet<object>(ByReferenceEqualityComparer<object>.Instance), settings);

        [NotNull]
        private static IPrintToken CreateInternal([CanBeNull] object item, [NotNull] HashSet<object> path, [NotNull] PrintSettings settings)
        {
            if (item == null)
                return new ValueToken(NullValue);

            if (!path.Add(item))
                return new ValueToken(CyclicValue);

            try
            {
                var itemType = item.GetType();

                using (SecurityHelper.StartSecurityScope(itemType))
                {
                    if (ToStringDetector.HasCustomToString(itemType))
                        return new ValueToken(item.ToString());

                    foreach (var pair in CustomFormatters)
                    {
                        if (pair.Key.IsAssignableFrom(itemType))
                            return new ValueToken(pair.Value(item));
                    }

                    if (DictionaryInspector.IsSimpleDictionary(itemType))
                    {
                        var pairs = DictionaryInspector.EnumerateSimpleDictionary(item);
                        var tokens = pairs.Select(pair => new PropertyToken(pair.Item1, CreateInternal(pair.Item2, path, settings))).ToArray();
                        if (tokens.Length == 0)
                            return new ValueToken(EmptyDictionaryValue);

                        return new ObjectToken(tokens);
                    }

                    if (item is IEnumerable sequence)
                    {
                        if (!sequence.GetEnumerator().MoveNext())
                            return new ValueToken(EmptySequenceValue);

                        var tokens = new List<IPrintToken>();

                        foreach (var element in sequence)
                            tokens.Add(CreateInternal(element, path, settings));

                        return new SequenceToken(tokens);
                    }

                    var fieldsAndProperties = new List<PropertyToken>();

                    foreach (var field in itemType.GetInstanceFields())
                        fieldsAndProperties.Add(ConstructProperty(field, () => CreateInternal(field.GetValue(item), path, settings), settings));

                    foreach (var property in itemType.GetInstanceProperties())
                        fieldsAndProperties.Add(ConstructProperty(property, () => CreateInternal(property.GetValue(item), path, settings), settings));

                    return new ObjectToken(fieldsAndProperties);
                }
            }
            finally
            {
                path.Remove(item);
            }
        }

        private static PropertyToken ConstructProperty(MemberInfo member, Func<IPrintToken> getValue, PrintSettings settings)
        {
            IPrintToken value;

            if (settings.HideSecretValues && SecurityHelper.IsSecret(member))
            {
                value = new ValueToken(SecretValue);
            }
            else
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

            return new PropertyToken(member.Name, value);
        }
    }
}
