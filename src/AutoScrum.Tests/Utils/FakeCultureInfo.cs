using System.Globalization;

namespace AutoScrum.Tests.Utils
{
    public class FakeCultureInfo : IDisposable
    {
        private CultureInfo _currentCulture;
        private CultureInfo _currentUICulture;

        private CultureInfo _defaultThreadCurrentCulture;
        private CultureInfo _defaultThreadCurrentUICulture;

        public static IDisposable SetCulture(CultureInfo overrideCulture)
        {
            return new FakeCultureInfo(CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture, CultureInfo.DefaultThreadCurrentCulture, CultureInfo.DefaultThreadCurrentUICulture, overrideCulture);
        }

        public FakeCultureInfo(CultureInfo currentCulture, CultureInfo currentUICulture, CultureInfo defaultThreadCurrentCulture, CultureInfo defaultThreadCurrentUICulture, CultureInfo overrideCulture)
        {
            _currentCulture = currentCulture;
            _currentUICulture = currentUICulture;
            _defaultThreadCurrentCulture = defaultThreadCurrentCulture;
            _defaultThreadCurrentUICulture = defaultThreadCurrentUICulture;

            // TODO: This doesn't seem to be working...
            CultureInfo.CurrentCulture = overrideCulture;
            CultureInfo.CurrentUICulture = overrideCulture;
            CultureInfo.DefaultThreadCurrentCulture = overrideCulture;
            CultureInfo.DefaultThreadCurrentUICulture = overrideCulture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _currentCulture;
            CultureInfo.CurrentUICulture = _currentUICulture;
            CultureInfo.DefaultThreadCurrentCulture = _defaultThreadCurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = _defaultThreadCurrentUICulture;
        }
    }
}
