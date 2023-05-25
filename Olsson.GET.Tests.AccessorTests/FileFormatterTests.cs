using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors.FileIO;

namespace Olsson.GET.Tests.AccessorTests
{
    [TestClass]
    public class FileFormatterTests
    {
        [TestMethod]
        public void FixedWidthFileFormatter_GetDryLocationIndicator_Supports16CharacterLength()
        {
            var sut = CreateFixedWithFileFormatter();
            var result = sut.GetDryLocationIndicator("        40      -1234.5678  0");
            Assert.AreEqual(-1234.5678, result, .00001);
        }

        [TestMethod]
        public void FixedWidthFileFormatter_GetDryLocationIndicator_SupportsExponents()
        {
            var sut = CreateFixedWithFileFormatter();
            var result = sut.GetDryLocationIndicator("        40 -1.2345678e+002  0");
            Assert.AreEqual(-123.45678, result, .000001);
        }

        private FixedWidthFileFormatter CreateFixedWithFileFormatter()
        {
            //todo: we may need to actually provide a model in the future (or better an IModflowFileAccessor) but right now for these tests, we don't need it.
            return new FixedWidthFileFormatter(new StructuredModflowFileAccessor(null));
        }
    }
}
