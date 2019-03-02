using System.Reflection;

namespace Vostok.Configuration.Parsers
{
    internal class ParseMethodParser : ITypeParser
    {
        private readonly MethodInfo parseMethod;

        public ParseMethodParser(MethodInfo parseMethod)
        {
            this.parseMethod = parseMethod;
        }

        public bool TryParse(string input, out object value)
        {
            try
            {
                value = parseMethod.Invoke(null, new object[] {input});

                return true;
            }
            catch
            {
                value = null;

                return false;
            }
        }
    }
}