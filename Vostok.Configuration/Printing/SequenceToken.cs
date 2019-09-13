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

        private void PrintJson(IPrintContext context)
        {
            context.Indent();
            context.Write('[');
            context.WriteLine();

            using (context.IncreaseDepth())
            {
                for (var index = 0; index < elements.Count; index++)
                {
                    if (elements[index] is ValueToken)
                        context.Indent();

                    elements[index].Print(context);

                    if (index < elements.Count - 1)
                        context.Write(',');

                    context.WriteLine();
                }
            }

            context.Indent();
            context.Write(']');
        }
    }
}