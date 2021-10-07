using System.Globalization;
using System.Threading;

namespace AutoScrum.Tests.Utils;

public abstract class TestBase
{
    protected TestBase()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(Constants.LocalCultureIdentifier);
    }
}