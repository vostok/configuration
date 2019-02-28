namespace Vostok.Configuration.Printing
{
    internal class PropertyToken : IPrintToken
    {
        private readonly IPrintToken value;

        public PropertyToken(string name, IPrintToken value)
        {
            Name = name;

            this.value = value;
        }

        public string Name { get; }

        public void Print(IPrintContext context)
        {
            context.Write(Name);
            context.Write(": ");

            if (value is ObjectToken || value is SequenceToken)
                context.WriteLine();

            using (context.IncreaseDepth())
                value.Print(context);
        }
    }
}
