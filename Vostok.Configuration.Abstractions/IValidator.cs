using System.Collections.Generic;

namespace Vostok.Configuration.Abstractions
{
    public interface IValidator
    {
        IReadOnlyDictionary<string, string> Errors { get; }
        void Validate<T>(T obj);
        /*void Validate<T>(T obj)
        {
            if (IsValid(obj)) return;
            var sb = new StringBuilder($"{typeof(T).Name} validation exception:\r\n");
            foreach (var pair in Errors) sb.AppendLine($"{pair.Key}: {pair.Value}");
            throw new FormatException(sb.ToString());
        }*/
        bool IsValid<T>(T obj);
    }
}