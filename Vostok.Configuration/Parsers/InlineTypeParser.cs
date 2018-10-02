namespace Vostok.Configuration.Parsers
{
    internal class InlineTypeParser<T> : ITypeParser
    {
        private readonly TryParse<T> parseMethod;

        public InlineTypeParser(TryParse<T> parseMethod) =>
            this.parseMethod = parseMethod;

        public bool TryParse(string s, out object value)
        {
            var result = parseMethod(s, out var v);
            value = v;
            return result;
        }
    }
}