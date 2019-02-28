using System.Reactive.PlatformServices;

#pragma warning disable 618

namespace Vostok.Configuration.Helpers
{
    // (iloktionov): This hack exists to fix crashes that happen with attached debugger due to ILRepack'ed Rx library.
    // (iloktionov): The origin of these crashes is some very hairy code in CurrentPlatformEnlightenmentProvider that messes with assemblies and type loading -_-
    internal static class RxHacker
    {
        static RxHacker()
        {
            PlatformEnlightenmentProvider.Current = new CustomProvider(PlatformEnlightenmentProvider.Current);
        }

        public static void Hack()
        {
        }

        private class CustomProvider : IPlatformEnlightenmentProvider
        {
            private readonly IPlatformEnlightenmentProvider provider;

            public CustomProvider(IPlatformEnlightenmentProvider provider)
            {
                this.provider = provider;
            }

            public T GetService<T>(params object[] args)
                where T : class
            {
                try
                {
                    return provider.GetService<T>();
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
