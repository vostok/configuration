namespace Vostok.Configuration.Cache
{
    internal class ValueWrapper<T>
    {
        public T Value { get; }

        public ValueWrapper(T value)
        {
            Value = value;
        }

        public static implicit operator ValueWrapper<T>(T value)
        {
            return new ValueWrapper<T>(value);
        }
    }
}