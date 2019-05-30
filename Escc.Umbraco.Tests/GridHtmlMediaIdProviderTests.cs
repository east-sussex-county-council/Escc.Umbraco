using Escc.Umbraco.Media;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using Umbraco.Core.Services;

namespace Escc.Umbraco.Tests
{
    [TestFixture]
    public class GridHtmlMediaIdProviderTests
    {
        [Test]
        public void MediaUdiIsFound()
        {
            var mediaService = new Mock<IMediaService>();
            var provider = new GridHtmlMediaIdProvider(new string[] { "Umbraco.Grid" }, mediaService.Object);

            var mediaGuids = provider.ReadMediaGuidsFromGridJson(ExampleValues.GridJsonWithHtmlLink);

            Assert.AreEqual(mediaGuids.FirstOrDefault(), new Guid("cee5459177ba48fd8db8739d2a1cc8d0"));
        }
    }
}
