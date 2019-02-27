namespace Vostok.Configuration.Printing
{
    internal class ValueToken : IPrintToken
    {
        private readonly string value;

        public ValueToken(string value)
        {
            this.value = value;
        }

        public void Print(IPrintContext context)
        {
            context.Write(value);
        }
    }
}