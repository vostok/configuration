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
            switch (context.Format)
            {
                case PrintFormat.YAML:
                    PrintYaml(context);
                    break;

                case PrintFormat.JSON:
                    PrintJson(context);
                    break;
            }
        }

        private void PrintYaml(IPrintContext context)
        {
            context.Write(Name);
            context.Write(": ");

            if (value is ObjectToken || value is SequenceToken)
                context.WriteLine();

            using (context.IncreaseDepth())
                value.Print(context);
        }

        private void PrintJson(IPrintContext context)
        {
            context.WriteQuoted(Name);
            context.Write(": ");

            if (value is ObjectToken || value is SequenceToken)
                context.WriteLine();

            value.Print(context);
        }
    }
}
