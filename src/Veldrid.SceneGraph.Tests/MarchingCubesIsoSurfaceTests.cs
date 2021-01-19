//
// Implementation of Marching Cubes:
//     W.Lorensen, H.Cline. Marching Cubes: A High Resolution 3D Surface Construction Algorithm. Computer Graphics, 21 (4): 163-169, July 1987
//

using System.CodeDom.Compiler;
using System.Numerics;
using NUnit.Framework;
using Veldrid.SceneGraph.Math.IsoSurface;

namespace Veldrid.SceneGraph.Tests
{
    public class VoxelVolume : IVoxelVolume
    {
        public double[,,] Values { get; }
        public double[,,] XValues { get; }
        public double[,,] YValues { get; }
        public double[,,] ZValues { get; }

        public VoxelVolume()
        {
            Values = new double[2, 2, 2];
            XValues = new double[2, 2, 2];
            YValues = new double[2, 2, 2];
            ZValues = new double[2, 2, 2];
        }
    }

    public class ZeroVoxelVolume : VoxelVolume
    {
        public ZeroVoxelVolume() : base()
        {
            for (var z = 0; z < 2; ++z)
            {
                for (var y = 0; y < 2; ++y)
                {
                    for (var x = 0; x < 2; ++x)
                    {
                        XValues[x, y, z] = x;
                        YValues[x, y, z] = y;
                        ZValues[x, y, z] = z;
                        Values[x, y, z] = 0.0;
                    }
                }
            }
        }
    }
    
    public class CornerVoxelVolume : VoxelVolume
    {
        
        
        public CornerVoxelVolume() : base()
        {
            for (var z = 0; z < 2; ++z)
            {
                for (var y = 0; y < 2; ++y)
                {
                    for (var x = 0; x < 2; ++x)
                    {
                        XValues[x, y, z] = x;
                        YValues[x, y, z] = y;
                        ZValues[x, y, z] = z;
                        Values[x, y, z] = 0;
                    }
                }
            }

            Values[0, 0, 0] = -1.0;
            Values[1, 1, 1] = 1.0;
        }
    }
    
    public class ZPlaneVoxelVolume : VoxelVolume
    {
        public ZPlaneVoxelVolume() : base()
        {
            for (var z = 0; z < 2; ++z)
            {
                for (var y = 0; y < 2; ++y)
                {
                    for (var x = 0; x < 2; ++x)
                    {
                        XValues[x, y, z] = x;
                        YValues[x, y, z] = y;
                        ZValues[x, y, z] = z;
                        Values[x, y, z] = z;
                    }
                }
            }
        }
    }
    
    public class YPlaneVoxelVolume : VoxelVolume
    {
        public YPlaneVoxelVolume() : base()
        {
            for (var z = 0; z < 2; ++z)
            {
                for (var y = 0; y < 2; ++y)
                {
                    for (var x = 0; x < 2; ++x)
                    {
                        XValues[x, y, z] = x;
                        YValues[x, y, z] = y;
                        ZValues[x, y, z] = z;
                        Values[x, y, z] = y;
                    }
                }
            }
        }
    }
    
    public class XPlaneVoxelVolume : VoxelVolume
    {
        public XPlaneVoxelVolume() : base()
        {
            for (var z = 0; z < 2; ++z)
            {
                for (var y = 0; y < 2; ++y)
                {
                    for (var x = 0; x < 2; ++x)
                    {
                        XValues[x, y, z] = x;
                        YValues[x, y, z] = y;
                        ZValues[x, y, z] = z;
                        Values[x, y, z] = x;
                    }
                }
            }
        }
    }
    
    [TestFixture]
    public class MarchingCubesIsoSurfaceTests
    {
        [TestCase]
        public void PassCanaryTest()
        {
            Assert.That(2+2, Is.EqualTo(4));
        }

        [TestCase]
        public void ShouldGenerateZPlaneCorrectly()
        {
            var zPlaneVoxelVolume = new ZPlaneVoxelVolume();
            var isoValue = 0.5d;

            var isoSurfaceGenerator = new MarchingCubesIsoSurfaceGenerator();
            var isoSurface = isoSurfaceGenerator.CreateIsoSurface(zPlaneVoxelVolume, isoValue);

            Assert.That(6, Is.EqualTo(isoSurface.IsoSurfaceVertices.Count));

            foreach (var vtx in isoSurface.IsoSurfaceVertices)
            {
                Assert.That(vtx.Z, Is.EqualTo(0.5));
            }
        }
        
        [TestCase]
        public void ShouldGenerateYPlaneCorrectly()
        {
            var yPlaneVoxelVolume = new YPlaneVoxelVolume();
            var isoValue = 0.5d;

            var isoSurfaceGenerator = new MarchingCubesIsoSurfaceGenerator();
            var isoSurface = isoSurfaceGenerator.CreateIsoSurface(yPlaneVoxelVolume, isoValue);

            Assert.That(6, Is.EqualTo(isoSurface.IsoSurfaceVertices.Count));

            foreach (var vtx in isoSurface.IsoSurfaceVertices)
            {
                Assert.That(vtx.Y, Is.EqualTo(0.5));
            }
        }
        
        [TestCase]
        public void ShouldGenerateXPlaneCorrectly()
        {
            var xPlaneVoxelVolume = new XPlaneVoxelVolume();
            var isoValue = 0.5d;

            var isoSurfaceGenerator = new MarchingCubesIsoSurfaceGenerator();
            var isoSurface = isoSurfaceGenerator.CreateIsoSurface(xPlaneVoxelVolume, isoValue);

            Assert.That(6, Is.EqualTo(isoSurface.IsoSurfaceVertices.Count));

            foreach (var vtx in isoSurface.IsoSurfaceVertices)
            {
                Assert.That(vtx.X, Is.EqualTo(0.5));
            }
        }
        
        [TestCase]
        public void ShouldGenerateOutsideCorrectly()
        {
            var voxelVolume = new ZeroVoxelVolume();
            var isoValue = 0.5d;

            var isoSurfaceGenerator = new MarchingCubesIsoSurfaceGenerator();
            var isoSurface = isoSurfaceGenerator.CreateIsoSurface(voxelVolume, isoValue);

            Assert.That(0, Is.EqualTo(isoSurface.IsoSurfaceVertices.Count));
        }
        
        [TestCase]
        public void ShouldGenerateCornersCorrectly()
        {
            var voxelVolume = new CornerVoxelVolume();
            var isoValue = 0.5d;

            var isoSurfaceGenerator = new MarchingCubesIsoSurfaceGenerator();
            var isoSurface = isoSurfaceGenerator.CreateIsoSurface(voxelVolume, isoValue);

            Assert.That(6, Is.EqualTo(isoSurface.IsoSurfaceVertices.Count));
        }
    }
}