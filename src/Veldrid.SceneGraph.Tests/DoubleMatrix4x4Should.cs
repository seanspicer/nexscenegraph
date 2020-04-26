using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;

namespace Veldrid.SceneGraph.Tests
{
    [TestFixture]
    public class DoubleMatrix4x4Should
    {
        [TestCase]
        public void PassCanaryTest()
        {
            Assert.That(2+2, Is.EqualTo(4));
        }

        [TestCase]
        public void GetValueFromIndexerCorrectly()
        {
            var mat = DoubleMatrix4x4.Identity;
            mat.M43 = 4d;
            mat.M32 = 5d;
            
            Assert.That(mat[3, 2], Is.EqualTo(4d));
            Assert.That(mat[2, 1], Is.EqualTo(5d));
            
            Assert.That(mat[3][2], Is.EqualTo(4d));
            Assert.That(mat[2][1], Is.EqualTo(5d));
        }
        
        [TestCase]
        public void SetValueFromIndexerCorrectly()
        {
            var mat = DoubleMatrix4x4.Identity;
            mat[3,2] = 4d;
            mat[2,1] = 5d;
            
            Assert.That(mat[3, 2], Is.EqualTo(4d));
            Assert.That(mat[2, 1], Is.EqualTo(5d));
        }
    }
}