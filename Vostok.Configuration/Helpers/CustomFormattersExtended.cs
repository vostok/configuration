using System;
using System.Collections.Generic;
using Vostok.Commons.Formatting;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Helpers
{
    internal static class CustomFormattersExtended
    {
        private static readonly Dictionary<Type, Func<object, string>> Formatters
            = new()
            {
                [typeof(ISettingsNode)] = value => value.ToString()
            };

        public static bool TryFormat(object item, out string s) =>
            CustomFormatters.TryFormatWithExplicitFormatters(item, Formatters, out s) || CustomFormatters.TryFormat(item, out s);
    }
}