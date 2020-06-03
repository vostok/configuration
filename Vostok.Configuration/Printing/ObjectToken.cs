using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Printing
{
    internal class ObjectToken : IPrintToken
    {
        private readonly PropertyToken[] properties;

        public ObjectToken(IEnumerable<PropertyToken> properties)
        {
            this.properties = properties.ToArray();
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
            for (var index = 0; index < properties.Length; index++)
            {
                context.Indent();

                properties[index].Print(context);

                if (index < properties.Length - 1)
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
                for (var index = 0; index < properties.Length; index++)
                {
                    context.Indent();

                    properties[index].Print(context);

                    if (index < properties.Length - 1)
                        context.Write(',');

                    context.WriteLine();
                }
            }

            context.Indent();
            context.Write('}');
        }
    }
}