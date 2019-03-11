using System.Reflection;

namespace Vostok.Configuration.Parsers
{
    internal class TryParseMethodParser : ITypeParser
    {
        private readonly MethodInfo tryParseMethod;

        public TryParseMethodParser(MethodInfo tryParseMethod)
        {
            this.tryParseMethod = tryParseMethod;
        }
    
        public bool TryParse(string input, out object value)
        {
            var parameters = new object[] {input, null};

            var result = tryParseMethod.Invoke(null, parameters);

            value = parameters[1];

            return (bool) result;
        }
    }
}