using System;

namespace Vostok.Configuration.Printing
{
    internal interface IPrintContext
    {
        PrintFormat Format { get; }

        void Indent();

        void Write(char value);

        void Write(string value);

        void WriteQuoted(string value);

        void WriteLine();

        IDisposable IncreaseDepth();
    }
}