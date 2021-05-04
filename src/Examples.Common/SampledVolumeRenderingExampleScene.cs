using System.Numerics;
using System.Reflection;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Manipulators;
using Veldrid.SceneGraph.Manipulators.Commands;
using Veldrid.SceneGraph.Math.IsoSurface;
using Veldrid.SceneGraph.NodeKits.DirectVolumeRendering;
using Veldrid.SceneGraph.Shaders;
using Veldrid.SceneGraph.Util;

namespace Examples.Common
{
    public class VoxelVolume : IVoxelVolume
    {
        protected int XDim = 2;
        protected int YDim = 2;
        protected int ZDim = 2;


        public VoxelVolume()
        {
            Values = new double[XDim, YDim, ZDim];
            XValues = new double[XDim, YDim, ZDim];
            YValues = new double[XDim, YDim, ZDim];
            ZValues = new double[XDim, YDim, ZDim];
        }

        public double[,,] Values { get; }
        public double[,,] XValues { get; }
        public double[,,] YValues { get; }
        public double[,,] ZValues { get; }
    }

    public class CornerVoxelVolume : VoxelVolume
    {
        public CornerVoxelVolume()
        {
            for (var z = 0; z < ZDim; ++z)
            for (var y = 0; y < YDim; ++y)
            for (var x = 0; x < XDim; ++x)
            {
                XValues[x, y, z] = x;
                YValues[x, y, z] = y;
                ZValues[x, y, z] = z;
                Values[x, y, z] = 0;
            }

            //
            // Values[0, 0, 0] = 1.0;
            // Values[1, 1, 1] = 1.0;
        }
    }

    public class DraggerVolumeTileCallback : DraggerCallback
    {
        private Matrix4x4 _localToWorld;
        private readonly ILocator _locator;

        private Matrix4x4 _startMotionMatrix;
        private readonly IVolumeTile _volumeTile;
        private Matrix4x4 _worldToLocal;

        public DraggerVolumeTileCallback(IVolumeTile volumeTile, ILocator locator)
        {
            _volumeTile = volumeTile;
            _locator = locator;
        }

        public override bool Receive(IMotionCommand command)
        {
            if (null == _locator) return false;

            switch (command.Stage)
            {
                case IMotionCommand.MotionStage.Start:
                {
                    // Save the current matrix
                    _startMotionMatrix = _locator.Transform;

                    // Get the LocalToWorld and WorldToLocal matrix for this node.
                    var nodePathToRoot = Util.ComputeNodePathToRoot(_volumeTile);
                    _localToWorld = _startMotionMatrix * Transform.ComputeLocalToWorld(nodePathToRoot);
                    if (Matrix4x4.Invert(_localToWorld, out var worldToLocal)) _worldToLocal = worldToLocal;

                    return true;
                }
                case IMotionCommand.MotionStage.Move:
                {
                    // Transform the command's motion matrix into local motion matrix.
                    var localMotionMatrix = _localToWorld * command.GetWorldToLocal()
                                                          * command.GetMotionMatrix()
                                                          * command.GetLocalToWorld() * _worldToLocal;

                    var matrix = localMotionMatrix.PostMultiply(_startMotionMatrix);

                    var xMin = matrix.M41;
                    var yMin = matrix.M42;
                    var zMin = matrix.M43;

                    var xMax = matrix.M11 + xMin;
                    var yMax = matrix.M22 + yMin;
                    var zMax = matrix.M33 + zMin;

                    if (_volumeTile.Layer is IVoxelVolumeLayer layer)
                    {
                        var lcl = layer.BaseLocator;
                        if (xMin >= lcl.XMin &&
                            xMax <= lcl.XMax &&
                            yMin >= lcl.YMin &&
                            yMax <= lcl.YMax &&
                            zMin >= lcl.ZMin &&
                            zMax <= lcl.ZMax)
                        {
                        }
                    }

                    // Transform by the localMotionMatrix
                    _locator.SetTransform(localMotionMatrix.PostMultiply(_startMotionMatrix));

                    return true;
                }
                case IMotionCommand.MotionStage.Finish:
                {
                    return true;
                }
                case IMotionCommand.MotionStage.None:
                default:
                    return false;
            }
        }
    }

    public class VolumeExtentsConstraint : Constraint
    {
        private float _xmax;
        private float _xmin;
        private float _ymax;
        private float _ymin;
        private float _zmax;
        private float _zmin;

        public VolumeExtentsConstraint(ILevoyCabralLocator baseLocator)
        {
            _xmin = (float) baseLocator.XMin;
            _xmax = (float) baseLocator.XMax;
            _ymin = (float) baseLocator.YMin;
            _ymax = (float) baseLocator.YMax;
            _zmin = (float) baseLocator.ZMin;
            _zmax = (float) baseLocator.ZMax;
        }

        public override bool Constrain(IMotionCommand command)
        {
            return base.Constrain(command);
        }
    }

    public class SampledVolumeRenderingExampleScene
    {
        public delegate IShaderSet ShaderSetBuilder();

        public static IGroup Build(ShaderSetBuilder builder, IVoxelVolume voxelVolume,
            LevoyCabralTechnique.TextureTranslator textureTranslator)
        {
            
            var volume = Volume.Create();
            var tile1 = VolumeTile.Create();
            volume.AddChild(tile1);

            var layer = VoxelVolumeLayer.Create(voxelVolume);
            tile1.Layer = layer;
            tile1.Locator = layer.GetLocator();
            tile1.SetVolumeTechnique(LevoyCabralTechnique.Create(builder(), textureTranslator));

            var dragger1 = TabBoxDragger.Create();
            dragger1.SetupDefaultGeometry();
            dragger1.ActivationModKeyMask = IUiEventAdapter.ModKeyMaskType.ModKeyCtl;
            dragger1.HandleEvents = true;
            dragger1.DraggerCallbacks.Add(new DraggerVolumeTileCallback(tile1, tile1.Locator));
            dragger1.Matrix = Matrix4x4.CreateTranslation(0.5f, 0.5f, 0.5f)
                .PostMultiply(tile1.Locator.Transform);

            /////////////////

            // var tile2 = VolumeTile.Create();
            // tile2.Layer = layer;
            // tile2.Locator = layer.GetLocator();
            // tile2.SetVolumeTechnique(LevoyCabralTechnique.Create(builder()));
            // volume.AddChild(tile2);
            //
            // var dragger2 = TabBoxDragger.Create();
            // dragger2.SetupDefaultGeometry();
            // dragger2.ActivationModKeyMask = IUiEventAdapter.ModKeyMaskType.ModKeyAlt;
            // dragger2.HandleEvents = true;
            // dragger2.DraggerCallbacks.Add(new DraggerVolumeTileCallback(tile2, tile2.Locator));
            // dragger2.Matrix = Matrix4x4.CreateTranslation(0.5f, 0.5f, 0.5f)
            //     .PostMultiply(tile2.Locator.Transform);

            /////////////////

            var root = Group.Create();
            root.AddChild(volume);
            root.AddChild(dragger1);
            // root.AddChild(dragger2);
            return root;
        }

        public static IGroup Build()
        {
            return Build(CreateShaderSet, new CornerVoxelVolume(), null);
        }

        public static IShaderSet CreateShaderSet()
        {
            var asm = Assembly.GetCallingAssembly();

            var vtxShader =
                new ShaderDescription(ShaderStages.Vertex,
                    ShaderTools.ReadEmbeddedAssetBytes(
                        @"Examples.Common.Assets.Shaders.ProceduralVolumeShader-vertex.glsl", asm), "main");

            var frgShader =
                new ShaderDescription(ShaderStages.Fragment,
                    ShaderTools.ReadEmbeddedAssetBytes(
                        @"Examples.Common.Assets.Shaders.ProceduralVolumeShader-fragment.glsl", asm), "main");

            return ShaderSet.Create("ProceduralVolumeShader", vtxShader, frgShader);
        }
    }
}