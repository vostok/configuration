using System;
using System.Collections.Generic;
using System.Text;

namespace Vostok.Configuration.Abstractions
{
    public abstract class BaseValidator : IValidator
    {
        public IDictionary<string, string> Errors { get; } = new Dictionary<string, string>();

        public void Validate<T>(T obj)
        {
            if (IsValid(obj)) return;
            var sb = new StringBuilder($"{typeof(T).Name} validation exception:\r\n");
            foreach (var pair in Errors)
                sb.AppendLine($"{pair.Key}: {pair.Value}");
            throw new FormatException(sb.ToString());
        }

        public abstract bool IsValid<T>(T obj);

        protected bool CheckNull<T>(T obj)
        {
            if (obj == null)
            {
                Errors["Null"] = "object is null";
                return false;
            }

            return true;
        }

        protected bool CheckType<T>(object obj, out T casted)
        {
            casted = default;
            if (!(obj is T))
            {
                Errors["Type"] = "wrong object type";
                return false;
            }

            casted = (T)obj;
            return true;
        }
    }
}