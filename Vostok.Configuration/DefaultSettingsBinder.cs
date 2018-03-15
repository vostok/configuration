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
    public class DefaultSettingsBinder: ISettingsBinder
    {
        private const string ParameterIsNull = "Settings parameter is null";
        private const string ValueIsNull = "Settings value is empty";
        private const string DictIsEmpty = "Settings dictionary is empty";
        private const string DictItemIsEmpty = "Settings dictionary item is empty";
        private const string ListIsEmpty = "Settings list is empty";
        private const string ListItemIsEmpty = "Settings list item is empty";

        private interface ITypeParser
        {
            bool TryParse(string s, out object value);
        }

        private delegate bool TryParse<T>(string s, out T value);
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
        
        public TSettings Bind<TSettings>(RawSettings settings)
        {
            CheckArgumentIsNull(settings, ParameterIsNull);

            var primitiveParsers = new Dictionary<Type, ITypeParser>
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
                {typeof(TimeSpan), new InlineTypeParser<TimeSpan>(TimeSpanParser.TryParse)},
                {typeof(IPAddress), new InlineTypeParser<IPAddress>(IPAddress.TryParse)},
                {typeof(IPEndPoint), new InlineTypeParser<IPEndPoint>(IPEndPointParser.TryParse)},
                {typeof(Guid), new InlineTypeParser<Guid>(Guid.TryParse)},
                {typeof(Uri), new InlineTypeParser<Uri>(UriParser.TryParse)},
                {typeof(DataSize), new InlineTypeParser<DataSize>(DataSizeParser.TryParse)},
                {typeof(DataRate), new InlineTypeParser<DataRate>(DataRateParser.TryParse)},
             };
            var bindType = typeof (TSettings);
            var bindGtd = bindType.IsGenericType ? bindType.GetGenericTypeDefinition() : null;
            
            if (IsPrimitiveOrSimple(bindType))
            {
                if (bindType == typeof(string))
                    return (TSettings)(object) settings.Value;

                if (string.IsNullOrWhiteSpace(settings.Value))
                    CheckArgumentIsNull(null, ValueIsNull);

                if (primitiveParsers[bindType].TryParse(settings.Value, out var res))
                    return (TSettings) res;
                else throw new InvalidCastException($"{settings.Value} to {bindType.Name}");    //Wow! New primitive?
            }
            else if (bindType.IsEnum)
            {
                if (string.IsNullOrWhiteSpace(settings.Value))
                    CheckArgumentIsNull(null, ValueIsNull);

                foreach (var name in Enum.GetNames(typeof (TSettings)).Where(n => n.ToLower() == settings.Value.ToLower()))
                    return (TSettings) Enum.Parse(typeof (TSettings), name, true);
                if (int.TryParse(settings.Value, out var intVal))
                    foreach (var value in Enum.GetValues(typeof(TSettings)))
                        if ((int) value == intVal)
                            return (TSettings)(object) intVal;
                throw new InvalidCastException("Value for enum was not found.");
            }
            else if (bindType.IsValueType && bindType.IsGenericType) //before IsValueType - Nullable<T>
            {
                if (settings.Value == null)
                    return default;

                var res = BindInvokeSettings(new RawSettings(settings.Value), bindType.GenericTypeArguments[0]);
                return (TSettings) res;
            }
            else if (bindType.IsValueType) //structs only
            {
                CheckArgumentIsNull(settings.ChildrenByKey, DictIsEmpty);
                var inst = default(TSettings);
                var boxedInst = (object) inst;

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
                return (TSettings) boxedInst;
            }
            else if (bindType.IsArray)  //before IsClass
            {
                CheckArgumentIsNull(settings.Children, ListIsEmpty);
                var elType = bindType.GetElementType();
                var inst = Array.CreateInstance(elType, settings.Children.Count());
                var i = 0;
                foreach (var item in settings.Children)
                {
                    CheckArgumentIsNull(item, ListItemIsEmpty);
                    var val = BindInvokeList(i, elType, settings);
                    inst.SetValue(val, i++);
                }
                return (TSettings)(object) inst;
            }
            else if (bindType.IsGenericType && (bindGtd == typeof(List<>) || bindGtd == typeof(IEnumerable<>))) //before IsClass
            {
                CheckArgumentIsNull(settings.Children, ListIsEmpty);
//                var inst = Activator.CreateInstance(typeof(List<>).MakeGenericType(bindGtd.GetGenericArguments()));
                var listType = typeof(List<>);
                var genType = listType.MakeGenericType(bindType.GetGenericArguments());
                var inst = Activator.CreateInstance(genType);
                //var addMethod = inst.GetType().GetMethod(nameof(List<int>.Add));
                var i = 0;
                foreach (var item in settings.Children)
                {
                    CheckArgumentIsNull(item, ListItemIsEmpty);
                    var val = BindInvokeList(i, genType.GenericTypeArguments[0], settings);
                    ((IList) inst).Add(val);
                    //addMethod.Invoke(inst, new[] {val});
                    i++;
                }
                return (TSettings) inst;
            }
            else if (bindType.IsGenericType && bindGtd == typeof(Dictionary<,>))    //before IsClass
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
                return (TSettings)inst;
            }
            else if (bindType.IsClass)  //generic classes also here
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
                return inst;
            }

            throw new InvalidCastException("Unknown data type. Ask developers to add it.");
        }
        private void CheckArgumentIsNull(object obj, string message)
        {
            if (obj == null)
                throw new ArgumentNullException(message);
        }

        private object BindInvoke(string fieldOrPropertyName, Type fieldOrPropertyType, RawSettings settings)
        {
            var method = typeof(DefaultSettingsBinder).GetMethod(nameof(Bind));
            var generic = method.MakeGenericMethod(fieldOrPropertyType);
            if (!fieldOrPropertyType.IsClass)
            {
                if (!settings.ChildrenByKey.ContainsKey(fieldOrPropertyName))
                    throw new InvalidCastException($"Key is absent: {fieldOrPropertyName}");
                var rs = settings.ChildrenByKey[fieldOrPropertyName];
                CheckArgumentIsNull(rs, $"Value of field/property '{fieldOrPropertyName}' is null");
                return generic.Invoke(this, new object[] { rs });
            }
            else
            {
                if (!settings.ChildrenByKey.ContainsKey(fieldOrPropertyName))
                    return null;
                else
                {
                    var rs = settings.ChildrenByKey[fieldOrPropertyName];
                    CheckArgumentIsNull(rs, $"Value of field/property '{fieldOrPropertyName}' is null");
                    return generic.Invoke(this, new object[] { rs });
                }
            }
        }

        private object BindInvokeList(int index, Type listType, RawSettings settings)
        {
            var method = typeof(DefaultSettingsBinder).GetMethod(nameof(Bind));
            var generic = method.MakeGenericMethod(listType);
            if (!listType.IsClass)
            {
                if (settings.Children.Count() <= index)
                    throw new InvalidCastException($"Key is absent by index: {index}"); //it must never be thrown
                var rs = settings.Children.ElementAt(index);
                CheckArgumentIsNull(rs, $"Value of list on index '{index}' is null");
                return generic.Invoke(this, new object[] { rs });
            }
            else
            {
                if (settings.Children.Count() <= index)
                    return null;
                else
                {
                    var rs = settings.Children.ElementAt(index);
                    CheckArgumentIsNull(rs, $"Value of list on index '{index}' is null");
                    return generic.Invoke(this, new object[] { rs });
                }
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
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(TimeSpan)
            || type == typeof(IPAddress)
            || type == typeof(IPEndPoint)
            || type == typeof(Guid)
            || type == typeof(Uri)
            || type == typeof(DataSize)
            || type == typeof(DataRate);
    }
}