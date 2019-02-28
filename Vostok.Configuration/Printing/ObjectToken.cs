using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Printing
{
    internal class ObjectToken : IPrintToken
    {
        private readonly PropertyToken[] properties;

        public ObjectToken(IEnumerable<PropertyToken> properties)
        {
            this.properties = properties.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        }

        public void Print(IPrintContext context)
        {
            if (properties.Length == 0)
            {
                context.Indent();
                context.Write("<empty>");
                return;
            }

            for (var index = 0; index < properties.Length; index++)
            {
                context.Indent();

                properties[index].Print(context);

                if (index < properties.Length - 1)
                    context.WriteLine();
            }
        }
    }
}