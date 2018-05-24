namespace Vostok.Configuration.Binders
{
    public interface ISettingsBinder<out T>
    {
        T Bind(IRawSettings rawSettings);
    }
}