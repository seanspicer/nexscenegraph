using NUnit.Framework;
using Veldrid.SceneGraph.InputAdapter;

namespace Veldrid.SceneGraph.Tests.InputAdapter
{
    [TestFixture]
    public class InputSnapshotAdapterShould
    {
        [TestCase]
        public void PassCanaryTest()
        {
            Assert.That(2 + 2, Is.EqualTo(4));
        }

        [TestCase]
        public void GenerateCorrectKeyMap()
        {
            var sut = new InputSnapshotAdapter();

            Assert.That(sut.MapKey(Key.Q), Is.EqualTo(IUiEventAdapter.KeySymbol.KeyQ));
            Assert.That(sut.MapKey(Key.F13), Is.EqualTo(IUiEventAdapter.KeySymbol.KeyF13));
            Assert.That(sut.MapKey(Key.Number6), Is.EqualTo(IUiEventAdapter.KeySymbol.Key6));
            Assert.That(sut.MapKey(Key.Comma), Is.EqualTo(IUiEventAdapter.KeySymbol.Unknown));
        }
    }
}