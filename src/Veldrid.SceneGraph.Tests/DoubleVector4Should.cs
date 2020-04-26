using NUnit.Framework;

namespace Veldrid.SceneGraph.Tests
{
    [TestFixture]
    public class DoubleVector4Should
    {
        [TestCase]
        public void PassCanaryTest()
        {
            Assert.That(2+2, Is.EqualTo(4));
        }

        [TestCase]
        public void GetValueFromIndexerCorrectly()
        {
            var vec = new DoubleVector4();
            vec.X = 1.0d;
            vec.Y = 2.0d;
            vec.Z = 3.0d;
            vec.W = 4.0d;

            Assert.That(vec[0], Is.EqualTo(1.0d));
            Assert.That(vec[1], Is.EqualTo(2.0d));
            Assert.That(vec[2], Is.EqualTo(3.0d));
            Assert.That(vec[3], Is.EqualTo(4.0d));
        }
        
        [TestCase]
        public void SetValueWithIndexerCorrectly()
        {
            var vec = new DoubleVector4();
            vec[0] = 1.0d;
            vec[1] = 2.0d;
            vec[2] = 3.0d;
            vec[3] = 4.0d;

            Assert.That(vec[0], Is.EqualTo(1.0d));
            Assert.That(vec[1], Is.EqualTo(2.0d));
            Assert.That(vec[2], Is.EqualTo(3.0d));
            Assert.That(vec[3], Is.EqualTo(4.0d));
        }
    }
}