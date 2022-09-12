using System.Collections.Generic;

namespace Vostok.Configuration.Printing
{
    internal class ObjectToken : IPrintToken
    {
        private readonly IReadOnlyList<PropertyToken> properties;

        public ObjectToken(IReadOnlyList<PropertyToken> properties)
        {
            this.properties = properties;
        }

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
            for (var index = 0; index < properties.Count; index++)
            {
                context.Indent();

                properties[index].Print(context);

                if (index < properties.Count - 1)
                    context.WriteLine();
            }
        }

        private void PrintJson(IPrintContext context)
        {
            context.Indent();
            context.Write('{');
            context.WriteLine();

            using (context.IncreaseDepth())
            {
                for (var index = 0; index < properties.Count; index++)
                {
                    context.Indent();

                    properties[index].Print(context);

                    if (index < properties.Count - 1)
                        context.Write(',');

                    context.WriteLine();
                }
            }

            context.Indent();
            context.Write('}');
        }
    }
}