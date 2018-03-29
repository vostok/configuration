using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Vostok.Commons;
using Vostok.Commons.Parsers;
using UriParser = Vostok.Commons.Parsers.UriParser;

namespace Vostok.Configuration
{
    public delegate bool TryParse<T>(string s, out T value);

    /// <inheritdoc />
    /// <summary>
    /// Default binder
    /// </summary>
    public class DefaultSettingsBinder : ISettingsBinder
    {
        #region Constants

        private const string ParameterIsNull = "Settings parameter \"%p\" is null";
        private const string ValueIsNull = "Settings value \"%v\" of type \"%t\" is empty";
        private const string DictIsEmpty = "Settings dictionary of value type \"%t\" is empty";
        private const string DictItemIsEmpty = "Settings dictionary item by key \"%k\" of value type \"%t\" is empty";
        private const string ListIsEmpty = "Settings list of type \"%t\" is empty";
        private const string ListItemIsEmpty = "Settings list item #%i of type \"%t\" is empty";

        #endregion

        private readonly Dictionary<Type, ITypeParser> primitiveAndSimpleParsers;

        public DefaultSettingsBinder()
        {
            primitiveAndSimpleParsers = new Dictionary<Type, ITypeParser>
            {
                {typeof(bool), new InlineTypeParser<bool>(bool.TryParse)},
                {typeof(byte), new InlineTypeParser<byte>(byte.TryParse)},
                {typeof(char), new InlineTypeParser<char>(char.TryParse)},
                {typeof(decimal), new InlineTypeParser<decimal>(DecimalParser.TryParse)},
                {typeof(double), new InlineTypeParser<double>(DoubleParser.TryParse)},
                {typeof(float), new InlineTypeParser<float>(FloatParser.TryParse)},
                {typeof(int), new InlineTypeParser<int>(int.TryParse)},
                {typeof(long), new InlineTypeParser<long>(long.TryParse)},
                {typeof(sbyte), new InlineTypeParser<sbyte>(sbyte.TryParse)},
                {typeof(short), new InlineTypeParser<short>(short.TryParse)},
                {typeof(uint), new InlineTypeParser<uint>(uint.TryParse)},
                {typeof(ulong), new InlineTypeParser<ulong>(ulong.TryParse)},
                {typeof(ushort), new InlineTypeParser<ushort>(ushort.TryParse)},
                {typeof(DateTime), new InlineTypeParser<DateTime>(DateTimeParser.TryParse)},
                {typeof(DateTimeOffset), new InlineTypeParser<DateTimeOffset>(DateTimeOffsetParser.TryParse)},
                {typeof(TimeSpan), new InlineTypeParser<TimeSpan>(TimeSpanParser.TryParse)},
                {typeof(IPAddress), new InlineTypeParser<IPAddress>(IPAddress.TryParse)},
                {typeof(IPEndPoint), new InlineTypeParser<IPEndPoint>(IPEndPointParser.TryParse)},
                {typeof(Guid), new InlineTypeParser<Guid>(Guid.TryParse)},
                {typeof(Uri), new InlineTypeParser<Uri>(UriParser.TryParse)},
                {typeof(DataSize), new InlineTypeParser<DataSize>(DataSizeParser.TryParse)},
                {typeof(DataRate), new InlineTypeParser<DataRate>(DataRateParser.TryParse)},
            };
        }

        public TSettings Bind<TSettings>(RawSettings settings)
        {
            return (TSettings)BindInternal(settings, typeof(TSettings), "root");
        }

        /// <summary>
        /// Adds custom parser which can parse from string value to specified type
        /// </summary>
        /// <typeparam name="T">Type of in which you need to parse</typeparam>
        /// <param name="parser">Class with method implemented TryParse_T_ delegate</param>
        /// <returns>This binder with new parser</returns>
        public DefaultSettingsBinder WithCustomParser<T>(ITypeParser parser)
        {
            primitiveAndSimpleParsers.Add(typeof(T), parser);
            return this;
        }

        /// <summary>
        /// Adds custom parser which can parse from string value to specified type
        /// </summary>
        /// <typeparam name="T">Type of in which you need to parse</typeparam>
        /// <param name="parseMethod">Method implemented TryParse_T_ delegate</param>
        /// <returns>This binder with new parser</returns>
        public DefaultSettingsBinder WithCustomParser<T>(TryParse<T> parseMethod)
        {
            primitiveAndSimpleParsers.Add(typeof(T), new InlineTypeParser<T>(parseMethod));
            return this;
        }

        private bool TryBindToPrimitiveOrSimple(RawSettings settings, Type bindType, out object result)
        {
            result = default;
            if (!IsPrimitiveOrSimple(bindType))
                return false;

            if (bindType == typeof(string))
            {
                result = settings.Value;
                return true;
            }

            string value = null;
            if (!string.IsNullOrWhiteSpace(settings.Value))
                value = settings.Value;
            else if (settings.Value == null && settings.Children == null && settings.ChildrenByKey != null && settings.ChildrenByKey.Count == 1)
                value = settings.ChildrenByKey.First().Value.Value;
            else
                CheckArgumentIsNull(null, ValueIsNull.Replace("%v", settings.Value).Replace("%t", bindType.Name));
            
            if (primitiveAndSimpleParsers[bindType].TryParse(value, out var res))
            {
                result = res;
                return true;
            }
                
            // (Mansiper): Must throw only if get new primitive like int128
            throw new InvalidCastException($"\"{value}\" to \"{bindType.Name}\"");
        }

        private static bool TryBindToEnum(RawSettings settings, Type bindType, out object result)
        {
            result = default;
            if (!bindType.IsEnum)
                return false;

            if (string.IsNullOrWhiteSpace(settings.Value))
                CheckArgumentIsNull(null, ValueIsNull.Replace("%v", settings.Value).Replace("%t", bindType.Name));

            foreach (var name in Enum.GetNames(bindType).Where(n => string.Equals(n, settings.Value, StringComparison.OrdinalIgnoreCase)))
            {
                result = Enum.Parse(bindType, name, true);
                return true;
            }

            if (int.TryParse(settings.Value, out var intVal) && Enum.IsDefined(bindType, intVal))
            {
                result = intVal;
                return true;
            }

            throw new InvalidCastException($"Value \"{settings.Value}\" for enum \"{bindType.Name}\" was not found.");
        }

        private bool TryBindToNullable(RawSettings settings, Type bindType, out object result)
        {
            result = default;
            if (!IsNullableValue(bindType))
                return false;
            if (settings.Value == null)
                return true;

            var res = BindInternal(new RawSettings(settings.Value), bindType.GenericTypeArguments[0]);
            result = res;
            return true;
        }

        private bool TryBindToStruct(RawSettings settings, Type bindType, out object result)
        {
            result = default;
            if (!bindType.IsValueType)
                return false;

            CheckArgumentIsNull(settings.ChildrenByKey, DictIsEmpty);
            result = Activator.CreateInstance(bindType);

            foreach (var field in bindType.GetFields())
            {
                var binderAttributes = GetAttributes(field.GetCustomAttributes().ToArray());
                var res = BindInvokeFP(field.Name, field.FieldType, settings, binderAttributes);
                field.SetValue(result, res);
            }
            foreach (var prop in bindType.GetProperties().Where(p => p.CanWrite))
            {
                var binderAttributes = GetAttributes(prop.GetCustomAttributes().ToArray());
                var res = BindInvokeFP(prop.Name, prop.PropertyType, settings, binderAttributes);
                prop.SetValue(result, res);
            }
            return true;
        }

        private bool TryBindToArray(RawSettings settings, Type bindType, out object result)
        {
            result = default;
            if (!bindType.IsArray)
                return false;

            var elType = bindType.GetElementType();
            CheckArgumentIsNull(settings.Children, ListIsEmpty.Replace("%t", elType.Name));
            var inst = Array.CreateInstance(elType, settings.Children.Count);
            var i = 0;
            foreach (var item in settings.Children)
            {
                CheckArgumentIsNull(item, ListItemIsEmpty.Replace("%i", i.ToString()).Replace("%t", elType.Name));
                var val = BindInvokeList(i, elType, settings);
                inst.SetValue(val, i++);
            }
            result = inst;
            return true;
        }

        private bool TryBindToList(RawSettings settings, Type bindType, out object result)
        {
            var bindGtd = bindType.IsGenericType ? bindType.GetGenericTypeDefinition() : null;
            result = default;
            if (!bindType.IsGenericType || bindGtd != typeof(List<>) && bindGtd != typeof(IEnumerable<>))
                return false;

            var genType = typeof(List<>).MakeGenericType(bindType.GetGenericArguments());
            CheckArgumentIsNull(settings.Children, ListIsEmpty.Replace("%t", genType.Name));
            var inst = Activator.CreateInstance(genType);
            var i = 0;
            foreach (var item in settings.Children)
            {
                CheckArgumentIsNull(item, ListItemIsEmpty.Replace("%i", i.ToString()).Replace("%t", genType.Name));
                var val = BindInvokeList(i, genType.GenericTypeArguments[0], settings);
                ((IList)inst).Add(val);
                i++;
            }
            result = inst;
            return true;
        }

        private bool TryBindToDictionary(RawSettings settings, Type bindType, out object result)
        {
            var bindGtd = bindType.IsGenericType ? bindType.GetGenericTypeDefinition() : null;
            result = default;
            if (!bindType.IsGenericType || bindGtd != typeof(Dictionary<,>))
                return false;

            var genType = typeof(Dictionary<,>).MakeGenericType(bindType.GetGenericArguments());
            CheckArgumentIsNull(settings.ChildrenByKey, DictIsEmpty.Replace("%t", genType.GenericTypeArguments[1].Name));
            var inst = Activator.CreateInstance(genType);
            var i = 0;
            foreach (var item in settings.ChildrenByKey)
            {
                CheckArgumentIsNull(item, DictItemIsEmpty.Replace("%k", item.Key).Replace("%t", genType.GenericTypeArguments[1].Name));
                var key = BindInternal(new RawSettings(item.Key), genType.GenericTypeArguments[0]);
                var val = BindInternal(item.Value, genType.GenericTypeArguments[1], item.Key);
                ((IDictionary)inst).Add(key, val);
                i++;
            }
            result = inst;
            return true;
        }

        private bool TryBindToHashSet(RawSettings settings, Type bindType, out object result)
        {
            var bindGtd = bindType.IsGenericType ? bindType.GetGenericTypeDefinition() : null;
            result = default;
            if (!bindType.IsGenericType || bindGtd != typeof(HashSet<>))
                return false;

            CheckArgumentIsNull(settings.Children, ListIsEmpty.Replace("%t", bindType.Name));
            var inst = Activator.CreateInstance(bindType);
            var addMethod = bindType.GetMethods().FirstOrDefault(m => m.Name == nameof(HashSet<int>.Add));
            var i = 0;
            foreach (var item in settings.Children)
            {
                CheckArgumentIsNull(item, ListItemIsEmpty.Replace("%i", i.ToString()).Replace("%t", bindType.GenericTypeArguments[0].Name));
                var value = BindInvokeList(i, bindType.GenericTypeArguments[0], settings);
                addMethod.Invoke(inst, new[] {value});
                i++;
            }
            result = inst;
            return true;
        }

        private bool TryBindToClass(RawSettings settings, Type bindType, out object result)
        {
            result = default;
            if (!bindType.IsClass)
                return false;

            CheckArgumentIsNull(settings.ChildrenByKey, DictIsEmpty.Replace("%t", bindType.Name));
            var inst = Activator.CreateInstance(bindType);
            foreach (var field in bindType.GetFields())
            {
                var binderAttributes = GetAttributes(field.GetCustomAttributes().ToArray());
                var res = BindInvokeFP(field.Name, field.FieldType, settings, binderAttributes);
                field.SetValue(inst, res);
            }
            foreach (var prop in bindType.GetProperties().Where(p => p.CanWrite))
            {
                var binderAttributes = GetAttributes(prop.GetCustomAttributes().ToArray());
                var res = BindInvokeFP(prop.Name, prop.PropertyType, settings, binderAttributes);
                prop.SetValue(inst, res);
            }
            result = inst;
            return true;
        }

        private static void CheckArgumentIsNull(object obj, string message)
        {
            if (obj == null)
                throw new ArgumentNullException(message);
        }

        private object BindInternal(RawSettings settings, Type type, string paramName = null)
        {
            CheckArgumentIsNull(settings, ParameterIsNull.Replace("%p", paramName));

            // (Mansiper): The order is important!
            if (TryBindToPrimitiveOrSimple(settings, type, out var mainRes)
                || TryBindToEnum(settings, type, out mainRes)
                || TryBindToNullable(settings, type, out mainRes)
                || TryBindToStruct(settings, type, out mainRes)
                || TryBindToArray(settings, type, out mainRes)
                || TryBindToList(settings, type, out mainRes)
                || TryBindToDictionary(settings, type, out mainRes)
                || TryBindToHashSet(settings, type, out mainRes)
                || TryBindToClass(settings, type, out mainRes))
                return mainRes;

            throw new InvalidCastException($"Unknown data type \"{type.Name}\". If it is primitive ask developers to add it.");
        }

        private object BindInvokeFP(string fieldOrPropertyName, Type fieldOrPropertyType, RawSettings settings, BinderAttributes attributes)
        {
            if (!settings.ChildrenByKey.ContainsKey(fieldOrPropertyName))
            {
                if (attributes.HasFlag(BinderAttributes.IsOptional) && attributes.HasFlag(BinderAttributes.IsRequired))
                    return Activator.CreateInstance(
                        fieldOrPropertyType.IsClass || fieldOrPropertyType.GenericTypeArguments.Length == 0 
                            ? fieldOrPropertyType 
                            : fieldOrPropertyType.GenericTypeArguments[0]);
                else if (attributes.HasFlag(BinderAttributes.IsOptional))
                    return fieldOrPropertyType.IsClass ? null : Activator.CreateInstance(fieldOrPropertyType);
                else
                    throw new InvalidCastException($"Key \"{fieldOrPropertyName}\" is absent");
            }
            
            var rs = settings.ChildrenByKey[fieldOrPropertyName];
            if ((IsNullableValue(fieldOrPropertyType) || fieldOrPropertyType.IsClass) &&
                rs.Value == null && rs.Children == null && rs.ChildrenByKey == null)
            {
                if (attributes.HasFlag(BinderAttributes.IsRequired) && attributes.HasFlag(BinderAttributes.IsOptional))
                    return Activator.CreateInstance(fieldOrPropertyType);
                else if (attributes.HasFlag(BinderAttributes.IsRequired))
                    throw new InvalidCastException($"Not nullable value of field/property \"{fieldOrPropertyName}\" is null");
                else
                    return null;
            }
            CheckArgumentIsNull(rs, $"Value of field/property \"{fieldOrPropertyName}\" of type \"{fieldOrPropertyType.Name}\" is null");
            return BindInternal(rs, fieldOrPropertyType);
        }

        private object BindInvokeList(int index, Type listType, RawSettings settings)
        {
            if (!listType.IsClass)
            {
                if (settings.Children.Count <= index)
                    // (Mansiper): it must never be thrown
                    throw new InvalidCastException($"Key by index \"{index}\" is absent of list type {listType.Name}");
                return InvokeBind();
            }
            else
            {
                if (settings.Children.Count <= index)
                    return null;
                else
                    return InvokeBind();
            }

            object InvokeBind()
            {
                var rs = settings.Children.ElementAt(index);
                CheckArgumentIsNull(rs, $"Value of list of type \"{listType.Name}\" on index \"{index}\" is null");
                return BindInternal(rs, listType, $"list index {index}");
            }
        }

        private bool IsPrimitiveOrSimple(Type type) =>
            type.IsValueType && type.IsPrimitive
            || type == typeof(string)
            || primitiveAndSimpleParsers.ContainsKey(type);

        private static bool IsNullableValue(Type type) => 
            type.IsValueType && type.IsGenericType;

        private static BinderAttributes GetAttributes(Attribute[] attributes)
        {
            BinderAttributes binderAttributes = 0;
            var attrs = new Dictionary<Type, BinderAttributes>
            {
                { typeof(RequiredAttribute), BinderAttributes.IsRequired },
                { typeof(OptionalAttribute), BinderAttributes.IsOptional },
            };
            if (attributes != null && attributes.Length > 0)
                foreach (var attribute in attributes)
                    if (attrs.ContainsKey(attribute.GetType()))
                        binderAttributes |= attrs[attribute.GetType()];
            return binderAttributes;
        }

        private class InlineTypeParser<T> : ITypeParser
        {
            private readonly TryParse<T> parseMethod;

            public InlineTypeParser(TryParse<T> parseMethod)
            {
                this.parseMethod = parseMethod;
            }

            public bool TryParse(string s, out object value)
            {
                var result = parseMethod(s, out var v);
                value = v;
                return result;
            }
        }

        [Flags]
        private enum BinderAttributes
        {
            IsRequired = 1,
            IsOptional = 2,
        }
    }
}