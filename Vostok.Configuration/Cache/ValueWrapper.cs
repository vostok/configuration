namespace Vostok.Configuration.Cache
{
    internal class ValueWrapper<T>
    {
        public ValueWrapper(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public static implicit operator ValueWrapper<T>(T value) => new ValueWrapper<T>(value);
    }
}