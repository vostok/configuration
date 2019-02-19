using System.Net;

namespace Vostok.Configuration.Parsers
{
    internal static class IPEndPointParser
    {
        public static bool TryParse(string input, out IPEndPoint result)
        {
            result = null;
            IPAddress ipRes;

            var sepPos = input.LastIndexOf(':');
            if (sepPos == -1 || input.EndsWith("]") || !input.Contains(".") && !input.Contains("]:"))
            {
                input = input.Trim('[', ']');
                if (IPAddress.TryParse(input, out ipRes))
                {
                    result = new IPEndPoint(ipRes, 0);
                    return true;
                }

                return false;
            }

            var ip = input.Substring(0, sepPos).Trim('[', ']');
            var port = input.Substring(sepPos + 1, input.Length - sepPos - 1);
            if (IPAddress.TryParse(ip, out ipRes) && int.TryParse(port, out var portRes))
            {
                result = new IPEndPoint(ipRes, portRes);
                return true;
            }

            return false;
        }
    }
}