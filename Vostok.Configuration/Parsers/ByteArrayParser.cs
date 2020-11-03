using System;
using JetBrains.Annotations;

namespace Vostok.Configuration.Parsers
{
    [PublicAPI]
    public static class ByteArrayParser
    {
        public static bool TryParse(string input, out byte[] result)
        {
            try
            {
                result = Convert.FromBase64String(input);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}