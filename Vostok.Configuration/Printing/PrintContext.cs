using System;
using System.Text;

namespace Vostok.Configuration.Printing
{
    internal class PrintContext : IPrintContext
    {
        private const char Space = ' ';

        private readonly StringBuilder builder = new StringBuilder();
        private int depth;

        public string Content
            => builder.ToString();

        public void Indent()
            => builder.Append(Space, depth * 3);

        public void Write(char value)
            => builder.Append(value);

        public void Write(string value)
            => builder.Append(value);

        public void WriteLine()
            => builder.AppendLine();

        public IDisposable IncreaseDepth()
            => new IncreaseDepthToken(this);

        private class IncreaseDepthToken : IDisposable
        {
            private readonly PrintContext context;

            public IncreaseDepthToken(PrintContext context)
            {
                this.context = context;
                this.context.depth++;
            }

            public void Dispose()
            {
                context.depth--;
            }
        }
    }
}