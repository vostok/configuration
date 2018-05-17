namespace Vostok.Configuration.Binders
{
    public interface ISettingsBinder<out T>
    {
        T Bind(RawSettings rawSettings);
    }
}