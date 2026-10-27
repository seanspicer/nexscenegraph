using System.IO;
using NUnit.Framework;
using SkiaSharp;
using Veldrid.SceneGraph.AssetProcessor;

namespace Veldrid.SceneGraph.Tests
{
    [TestFixture]
    public class SkiaImageProcessorShould
    {
        [Test]
        public void DecodePngIntoRgbaWithMipChain()
        {
            using var bitmap = new SKBitmap(new SKImageInfo(8, 4, SKColorType.Rgba8888, SKAlphaType.Unpremul));
            bitmap.Erase(new SKColor(10, 20, 30, 255));
            bitmap.SetPixel(0, 0, new SKColor(200, 100, 50, 255));

            using var stream = new MemoryStream();
            bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
            stream.Position = 0;

            var texture = new SkiaImageProcessor().ProcessT(stream, "png");

            Assert.That(texture.Width, Is.EqualTo(8u));
            Assert.That(texture.Height, Is.EqualTo(4u));
            Assert.That(texture.MipLevels, Is.EqualTo(4u)); // 8x4, 4x2, 2x1, 1x1
            Assert.That(texture.TextureData.Length, Is.EqualTo((8 * 4 + 4 * 2 + 2 * 1 + 1 * 1) * 4));
            Assert.That(texture.TextureData[..4], Is.EqualTo(new byte[] { 200, 100, 50, 255 }));
            Assert.That(texture.TextureData[4..8], Is.EqualTo(new byte[] { 10, 20, 30, 255 }));
        }

        [Test]
        public void RejectUndecodableData()
        {
            using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });

            Assert.Throws<InvalidDataException>(() => new SkiaImageProcessor().ProcessT(stream, "png"));
        }
    }
}
