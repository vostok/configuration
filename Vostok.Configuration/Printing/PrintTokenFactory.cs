using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Vostok.Commons.Collections;
using Vostok.Commons.Formatting;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Printing
{
    internal static class PrintTokenFactory
    {
        private static readonly ValueToken NullValue = new ValueToken("null", false);
        private static readonly ValueToken ErrorValue = new ValueToken("<error>", false);
        private static readonly ValueToken SecretValue = new ValueToken("<secret>", false);
        private static readonly ValueToken CyclicValue = new ValueToken("<cyclic>", false);
        private static readonly ValueToken EmptySequenceValue = new ValueToken("[]", false);
        private static readonly ValueToken EmptyObjectValue = new ValueToken("{}", false);

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
                return NullValue;

            if (!path.Add(item))
                return CyclicValue;

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
                            return EmptyObjectValue;

                        return new ObjectToken(tokens);
                    }

                    if (item is IEnumerable sequence)
                    {
                        if (!sequence.GetEnumerator().MoveNext())
                            return EmptySequenceValue;

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

                    if (fieldsAndProperties.Count == 0)
                        return EmptyObjectValue;

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
                value = SecretValue;
            }
            else
            {
                try
                {
                    value = getValue();
                }
                catch
                {
                    value = ErrorValue;
                }
            }

            return new PropertyToken(member.Name, value);
        }
    }
}
