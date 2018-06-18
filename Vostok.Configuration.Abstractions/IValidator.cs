using System.Collections.Generic;

namespace Vostok.Configuration.Abstractions
{
    public interface IValidator
    {
        IDictionary<string, string> Errors { get; }
        void Validate<T>(T obj);
        bool IsValid<T>(T obj);
    }
}