namespace Vostok.Configuration
{
    /// <summary>
    /// Not static to be configured with custom type parsers.
    /// </summary>
    public interface ISettingsBinder
    {
        // TODO(krait): throws on error
        TSettings Bind<TSettings>(RawSettings rawSettings);

        //RawSettings Unbind<TSettings>(TSettings settings);
    }
}