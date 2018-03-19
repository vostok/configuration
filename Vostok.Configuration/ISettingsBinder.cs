namespace Vostok.Configuration
{
    /// <summary>
    /// Not static to be configured with custom type parsers.
    /// </summary>
    public interface ISettingsBinder
    {
        // TODO(krait): throws on error
        /// <summary>
        /// Bindes RawSettings tree to specified type
        /// </summary>
        /// <typeparam name="TSettings">Data type you need to get</typeparam>
        /// <param name="rawSettings">RawSettings tree</param>
        /// <returns>Value or object of specified type</returns>
        TSettings Bind<TSettings>(RawSettings rawSettings);

        //RawSettings Unbind<TSettings>(TSettings settings);
    }
}