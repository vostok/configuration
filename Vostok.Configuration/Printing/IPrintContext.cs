using System;

namespace Vostok.Configuration.Printing
{
    internal interface IPrintContext
    {
        void Indent();

        void Write(char value);

        void Write(string value);

        void WriteLine();

        IDisposable IncreaseDepth();
    }
}