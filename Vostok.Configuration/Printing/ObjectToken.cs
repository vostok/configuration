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
            for (var index = 0; index < properties.Count; index++)
            {
                context.Indent();

                properties[index].Print(context);

                if (index < properties.Count - 1)
                    context.WriteLine();
            }
        }
    }
}