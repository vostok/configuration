namespace Vostok.Configuration.Printing
{
    internal class ValueToken : IPrintToken
    {
        private readonly string value;
        private readonly bool mightNeedQuoting;

        public ValueToken(string value, bool mightNeedQuoting = true)
        {
            this.value = value;
            this.mightNeedQuoting = mightNeedQuoting;
        }

        public void Print(IPrintContext context)
        {
            if (context.Format == PrintFormat.YAML || !mightNeedQuoting)
            {
                context.Write(value);
            }
            else
            {
                context.WriteQuoted(value);
            }
        }
    }
}
