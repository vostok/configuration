using System;
using Vostok.Commons.Formatting;

namespace Vostok.Configuration.Helpers
{
    internal class ToStringHelper
    {
        public static bool TryUseCustomToString(object item, Type itemType, out string result)
        {
            result = null;

            if (!ParseMethodFinder.HasAnyKindOfParseMethod(itemType))
                return false;

            var toString = ToStringDetector.TryGetCustomToString(itemType);
            if (toString == null)
                return false;

            result = toString(item);
            return true;
        }
    }
}