namespace Vostok.Configuration.Printing
{
    internal class PropertyToken : IPrintToken
    {
        private readonly string name;
        private readonly IPrintToken value;

        public PropertyToken(string name, IPrintToken value)
        {
            this.name = name;
            this.value = value;
        }

        public void Print(IPrintContext context)
        {
            context.Write(name);
            context.Write(": ");

            if (value is ObjectToken || value is SequenceToken)
                context.WriteLine();

            using (context.IncreaseDepth())
                value.Print(context);
        }
    }
}