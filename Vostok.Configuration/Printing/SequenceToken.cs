using System.Collections.Generic;

namespace Vostok.Configuration.Printing
{
    internal class SequenceToken : IPrintToken
    {
        private readonly IReadOnlyList<IPrintToken> elements;

        public SequenceToken(IReadOnlyList<IPrintToken> elements)
        {
            this.elements = elements;
        }

        public void Print(IPrintContext context)
        {
            for (var index = 0; index < elements.Count; index++)
            {
                context.Indent();
                context.Write("- ");

                var element = elements[index];
                if (element is ObjectToken || element is SequenceToken)
                    context.WriteLine();

                using (context.IncreaseDepth())
                    element.Print(context);

                if (index < elements.Count - 1)
                    context.WriteLine();
            }
        }
    }
}