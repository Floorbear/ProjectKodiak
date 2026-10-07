using Kodiak.Core;
using NUnit.Framework;

namespace Kodiak.Tests.EditMode
{
    public class CoreMarkerTests
    {
        [Test]
        public void CoreMarker_CanBeConstructed()
        {
            var marker = new CoreMarker();

            Assert.IsNotNull(marker);
        }
    }
}
