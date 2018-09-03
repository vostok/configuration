using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Vostok.Commons.Collections;

namespace Vostok.Configuration.Helpers
{
    internal static class ObjectPropertiesExtractor
    {
        private const int CacheCapacity = 1000;

        private static readonly RecyclingBoundedCache<Type, (string name, Func<object, object> getter)[]> Cache =
            new RecyclingBoundedCache<Type, (string name, Func<object, object> getter)[]>(CacheCapacity);

        public static bool HasProperties(Type type) =>
            Cache.Obtain(type, LocateProperties).Length > 0;

        public static IEnumerable<(string, object)> ExtractProperties(object @object)
        {
            foreach (var (name, getter) in Cache.Obtain(@object.GetType(), LocateProperties))
                yield return (name, ObtainPropertyValue(@object, getter));
        }

        private static (string, Func<object, object>)[] LocateProperties(Type type)
        {
            try
            {
                var properties = type
                    .GetTypeInfo()
                    .DeclaredProperties
                    .Where(property => property.CanRead)
                    .Where(property => property.GetMethod.IsPublic)
                    .Where(property => !property.GetMethod.IsStatic)
                    .Where(property => !property.GetIndexParameters().Any())
                    .ToArray();

                var getters = new (string, Func<object, object>)[properties.Length];

                for (var i = 0; i < properties.Length; i++)
                {
                    var parameter = Expression.Parameter(typeof(object));
                    var convertedParameter = Expression.Convert(parameter, type);

                    var property = Expression.Property(convertedParameter, properties[i].Name);
                    var convertedProperty = Expression.Convert(property, typeof(object));

                    getters[i] = (properties[i].Name, Expression.Lambda<Func<object, object>>(convertedProperty, parameter).Compile());
                }

                return getters;
            }
            catch
            {
                return Array.Empty<(string, Func<object, object>)>();
            }
        }

        private static object ObtainPropertyValue(object @object, Func<object, object> getter)
        {
            try
            {
                return getter(@object);
            }
            catch (Exception error)
            {
                return $"<error: {error.Message}>";
            }
        }
    }
}