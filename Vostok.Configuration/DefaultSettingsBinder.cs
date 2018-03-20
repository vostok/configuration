using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Vostok.Commons;
using Vostok.Commons.Parsers;
using UriParser = Vostok.Commons.Parsers.UriParser;

namespace Vostok.Configuration
{
    public delegate bool TryParse<T>(string s, out T value);

    // CR(krait): The binder must support [Required]/[Optional] attributes on fields and properties.
    /// <inheritdoc />
    /// <summary>
    /// Default binder
    /// </summary>
    public class DefaultSettingsBinder : ISettingsBinder
    {
        // CR(krait): Lets hide these in a region.
        private const string ParameterIsNull = "Settings parameter is null";
        private const string ValueIsNull = "Settings value is empty";
        private const string DictIsEmpty = "Settings dictionary is empty";
        private const string DictItemIsEmpty = "Settings dictionary item is empty";
        private const string ListIsEmpty = "Settings list is empty";
        private const string ListItemIsEmpty = "Settings list item is empty";

        private readonly Dictionary<Type, ITypeParser> primitiveAndSimpleParsers;

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
            CheckArgumentIsNull(settings, ParameterIsNull);

            // (Mansiper): The order is important!
            if (TryBindToPrimitiveOrSimple(settings, out TSettings mainRes)
             || TryBindToEnum(settings, out mainRes)
             || TryBindToNullable(settings, out mainRes)
             || TryBindToStruct(settings, out mainRes)
             || TryBindToArray(settings, out mainRes)
             || TryBindToList(settings, out mainRes)
             || TryBindToDictionary(settings, out mainRes)
             || TryBindToClass(settings, out mainRes))
                return mainRes;

            // CR(krait): Add the type name to other exception messages as well.
            throw new InvalidCastException($"Unknown data type '{typeof(TSettings)}'. If it is primitive ask developers to add it.");
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

        private bool TryBindToPrimitiveOrSimple<TSettings>(RawSettings settings, out TSettings result)
        {
            var bindType = typeof(TSettings);
            result = default;

            if (IsPrimitiveOrSimple(bindType))
            {
                if (bindType == typeof(string))
                {
                    result = (TSettings)(object)settings.Value;
                    return true;
                }

                // CR(krait): We'll never know which setting it was.
                if (string.IsNullOrWhiteSpace(settings.Value))
                    CheckArgumentIsNull(null, ValueIsNull);

                if (primitiveAndSimpleParsers[bindType].TryParse(settings.Value, out var res))
                {
                    result = (TSettings)res;
                    return true;
                }
                
                // (Mansiper): Must throw only if get new primitive like int128
                throw new InvalidCastException($"{settings.Value} to {bindType.Name}");
            }

            return false;
        }

        private static bool TryBindToEnum<TSettings>(RawSettings settings, out TSettings result)
        {
            var bindType = typeof(TSettings);
            result = default;

            if (bindType.IsEnum)
            {
                if (string.IsNullOrWhiteSpace(settings.Value))
                    CheckArgumentIsNull(null, ValueIsNull);

                // CR(krait): string.Equals(a, b, StringComparison.OrdinalIgnoreCase)
                foreach (var name in Enum.GetNames(typeof(TSettings)).Where(n => n.ToLower() == settings.Value.ToLower()))
                {
                    result = (TSettings)Enum.Parse(typeof(TSettings), name, true);
                    return true;
                }
                // CR(krait): Enum.IsDefined() + cast?
                if (int.TryParse(settings.Value, out var intVal))
                    foreach (var value in Enum.GetValues(typeof(TSettings)))
                        if ((int) value == intVal)
                        {
                            result = (TSettings)(object)intVal;
                            return true;
                        }

                throw new InvalidCastException("Value for enum was not found.");
            }

            return false;
        }

        private bool TryBindToNullable<TSettings>(RawSettings settings, out TSettings result)
        {
            var bindType = typeof(TSettings);
            result = default;

            if (bindType.IsValueType && bindType.IsGenericType)
            {
                if (settings.Value == null)
                    return true;

                var res = BindInvokeSettings(new RawSettings(settings.Value), bindType.GenericTypeArguments[0]);
                result = (TSettings)res;
                return true;
            }

            return false;
        }

        private bool TryBindToStruct<TSettings>(RawSettings settings, out TSettings result)
        {
            var bindType = typeof(TSettings);
            result = default;

            if (bindType.IsValueType)
            {
                CheckArgumentIsNull(settings.ChildrenByKey, DictIsEmpty);
                var boxedInst = (object)default(TSettings);

                foreach (var field in bindType.GetFields())
                {
                    var res = BindInvoke(field.Name, field.FieldType, settings);
                    field.SetValue(boxedInst, res);
                }
                foreach (var prop in bindType.GetProperties().Where(p => p.CanWrite))
                {
                    var res = BindInvoke(prop.Name, prop.PropertyType, settings);
                    prop.SetValue(boxedInst, res);
                }
                result = (TSettings)boxedInst;
                return true;
            }

            return false;
        }

        private bool TryBindToArray<TSettings>(RawSettings settings, out TSettings result)
        {
            var bindType = typeof(TSettings);
            result = default;

            if (bindType.IsArray)
            {
                CheckArgumentIsNull(settings.Children, ListIsEmpty);
                var elType = bindType.GetElementType();
                var inst = Array.CreateInstance(elType, settings.Children.Count);
                var i = 0;
                foreach (var item in settings.Children)
                {
                    CheckArgumentIsNull(item, ListItemIsEmpty);
                    var val = BindInvokeList(i, elType, settings);
                    inst.SetValue(val, i++);
                }
                result = (TSettings)(object)inst;
                return true;
            }

            return false;
        }

        private bool TryBindToList<TSettings>(RawSettings settings, out TSettings result)
        {
            var bindType = typeof(TSettings);
            var bindGtd = bindType.IsGenericType ? bindType.GetGenericTypeDefinition() : null;
            result = default;

            if (bindType.IsGenericType && (bindGtd == typeof(List<>) || bindGtd == typeof(IEnumerable<>)))
            {
                CheckArgumentIsNull(settings.Children, ListIsEmpty);
                var listType = typeof(List<>);
                var genType = listType.MakeGenericType(bindType.GetGenericArguments());
                var inst = Activator.CreateInstance(genType);
                var i = 0;
                foreach (var item in settings.Children)
                {
                    CheckArgumentIsNull(item, ListItemIsEmpty);
                    var val = BindInvokeList(i, genType.GenericTypeArguments[0], settings);
                    ((IList)inst).Add(val);
                    i++;
                }
                result = (TSettings)inst;
                return true;
            }

            return false;
        }

        private bool TryBindToDictionary<TSettings>(RawSettings settings, out TSettings result)
        {
            var bindType = typeof(TSettings);
            var bindGtd = bindType.IsGenericType ? bindType.GetGenericTypeDefinition() : null;
            result = default;

            if (bindType.IsGenericType && bindGtd == typeof(Dictionary<,>))
            {
                CheckArgumentIsNull(settings.ChildrenByKey, DictIsEmpty);
                var dictType = typeof(Dictionary<,>);
                var genType = dictType.MakeGenericType(bindType.GetGenericArguments());
                var inst = Activator.CreateInstance(genType);
                var i = 0;
                foreach (var item in settings.ChildrenByKey)
                {
                    CheckArgumentIsNull(item, DictItemIsEmpty);
                    var key = BindInvokeSettings(new RawSettings(item.Key), genType.GenericTypeArguments[0]);
                    var val = BindInvokeSettings(item.Value, genType.GenericTypeArguments[1]);
                    ((IDictionary)inst).Add(key, val);
                    i++;
                }
                result = (TSettings)inst;
                return true;
            }

            return false;
        }

        private bool TryBindToClass<TSettings>(RawSettings settings, out TSettings result)
        {
            var bindType = typeof(TSettings);
            result = default;

            if (bindType.IsClass)
            {
                CheckArgumentIsNull(settings.ChildrenByKey, DictIsEmpty);
                var inst = Activator.CreateInstance<TSettings>();
                foreach (var field in bindType.GetFields())
                {
                    var res = BindInvoke(field.Name, field.FieldType, settings);
                    field.SetValue(inst, res);
                }
                foreach (var prop in bindType.GetProperties().Where(p => p.CanWrite))
                {
                    var res = BindInvoke(prop.Name, prop.PropertyType, settings);
                    prop.SetValue(inst, res);
                }
                result = inst;
                return true;
            }

            return false;
        }

        private static void CheckArgumentIsNull(object obj, string message)
        {
            if (obj == null)
                throw new ArgumentNullException(message);
        }

        // CR(krait): Why not just make a BindInternal(RawSettings, Type) and use it everywhere instead of public Bind<T>()?
        private object BindInvoke(string fieldOrPropertyName, Type fieldOrPropertyType, RawSettings settings)
        {
            var method = typeof(DefaultSettingsBinder).GetMethod(nameof(Bind));
            var generic = method.MakeGenericMethod(fieldOrPropertyType);

            if (!fieldOrPropertyType.IsClass)
            {
                if (!settings.ChildrenByKey.ContainsKey(fieldOrPropertyName))
                    throw new InvalidCastException($"Key is absent: {fieldOrPropertyName}");
                return InvokeBind();
            }
            else
            {
                if (!settings.ChildrenByKey.ContainsKey(fieldOrPropertyName))
                    return null;
                else return InvokeBind();
            }

            object InvokeBind()
            {
                var rs = settings.ChildrenByKey[fieldOrPropertyName];
                CheckArgumentIsNull(rs, $"Value of field/property '{fieldOrPropertyName}' is null");
                return generic.Invoke(this, new object[] { rs });
            }
        }

        private object BindInvokeList(int index, Type listType, RawSettings settings)
        {
            var method = typeof(DefaultSettingsBinder).GetMethod(nameof(Bind));
            var generic = method.MakeGenericMethod(listType);

            if (!listType.IsClass)
            {
                if (settings.Children.Count() <= index)
                    //it must never be thrown
                    throw new InvalidCastException($"Key is absent by index: {index}");
                return InvokeBind();
            }
            else
            {
                if (settings.Children.Count() <= index)
                    return null;
                else
                    return InvokeBind();
            }

            object InvokeBind()
            {
                var rs = settings.Children.ElementAt(index);
                CheckArgumentIsNull(rs, $"Value of list on index '{index}' is null");
                return generic.Invoke(this, new object[] { rs });
            }
        }

        private object BindInvokeSettings(RawSettings settings, Type tp)
        {
            var method = typeof(DefaultSettingsBinder).GetMethod(nameof(Bind));
            var generic = method.MakeGenericMethod(tp);
            return generic.Invoke(this, new object[] { settings });
        }

        private bool IsPrimitiveOrSimple(Type type) =>
            type.IsValueType && type.IsPrimitive
            || type == typeof(string)
            || primitiveAndSimpleParsers.ContainsKey(type);
    }
}