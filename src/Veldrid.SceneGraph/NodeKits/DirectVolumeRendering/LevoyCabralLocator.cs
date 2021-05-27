//
// Copyright 2018-2021 Sean Spicer 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.Numerics;
using System.Xml.Serialization;
using Veldrid.SceneGraph.Math.IsoSurface;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.NodeKits.DirectVolumeRendering
{
    public interface ILevoyCabralLocator : ILocator, IVoxelVolume
    {
        public double MinDist { get; }
        public double MaxDist { get; }

        public double XMin { get; }
        public double XMax { get; }

        public double YMin { get; }
        public double YMax { get; }

        public double ZMin { get; }
        public double ZMax { get; }
        public void UpdateDistances(Vector3 eyePoint);
        Vector3 TexGen(Vector3 point);

        void SetRotation(Quaternion rotation);
    }

    public class LevoyCabralLocator : Locator, ILevoyCabralLocator
    {
        private Vector3 _lastEyePoint = Vector3.Zero;

        private readonly IVoxelVolume _source;
        private double _sourceXMin;
        private double _sourceYMin;
        private double _sourceZMin;
        private double _sourceXMax;
        private double _sourceYMax;
        private double _sourceZMax;

        protected LevoyCabralLocator(IVoxelVolume voxelVolume)
        {
            _source = voxelVolume;

            var xdim = _source.XValues.GetLength(0) - 1;
            var ydim = _source.XValues.GetLength(1) - 1;
            var zdim = _source.XValues.GetLength(2) - 1;

            Values = new double[2, 2, 2];
            XValues = new double[2, 2, 2];
            YValues = new double[2, 2, 2];
            ZValues = new double[2, 2, 2];

            XMin = _source.XValues[0, 0, 0];
            XMax = _source.XValues[xdim, 0, 0];
            _sourceXMin = XMin;
            _sourceXMax = XMax;

            YMin = _source.YValues[0, 0, 0];
            YMax = _source.YValues[0, ydim, 0];
            _sourceYMin = YMin;
            _sourceYMax = YMax;

            ZMin = _source.ZValues[0, 0, 0];
            ZMax = _source.ZValues[0, 0, zdim];
            _sourceZMin = ZMin;
            _sourceZMax = ZMax;

            UpdateDistances(Vector3.Zero);

            SetTransformAsExtents(
                (float) XMin,
                (float) XMax,
                (float) YMin,
                (float) YMax,
                (float) ZMin,
                (float) ZMax);
        }

        public double[,,] Values { get; }
        public double[,,] XValues { get; }
        public double[,,] YValues { get; }
        public double[,,] ZValues { get; }

        public double XMin { get; protected set; }
        public double XMax { get; protected set; }

        public double YMin { get; protected set; }
        public double YMax { get; protected set; }

        public double ZMin { get; protected set; }
        public double ZMax { get; protected set; }

        public double MinDist { get; private set; }
        public double MaxDist { get; private set; }

        private Matrix4x4 Rotation { get; set; } =
            Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), (float) 0 / 8));

        private Matrix4x4 TranslateToCenter { get; set; } = Matrix4x4.Identity;
        private Matrix4x4 TranslateToCenterInv { get; set; } = Matrix4x4.Identity;

        public virtual void SetRotation(Quaternion rotation)
        {
            Rotation = Matrix4x4.CreateFromQuaternion(rotation);
            SetTransform(Transform);
        }
        
        public virtual Vector3 TexGen(Vector3 point)
        {
            return new Vector3(
                (float) ((point.X - _sourceXMin) / (_sourceXMax - _sourceXMin)),
                (float) ((point.Y - _sourceYMin) / (_sourceYMax - _sourceYMin)),
                (float) ((point.Z - _sourceZMin) / (_sourceZMax - _sourceZMin))
            );
        }

        public override void SetTransform(Matrix4x4 transform)
        {
            _transform = transform;
            if (Matrix4x4.Invert(_transform, out var inverse))
            {
                _inverse = inverse;
            }
            
            
            XMin = 0;
            YMin = 0;
            ZMin = 0;

            XMax = 1;
            YMax = 1;
            ZMax = 1;

            var center = new Vector3(0.5f, 0.5f, 0.5f);

            TranslateToCenter = Matrix4x4.CreateTranslation(center);
            
            if (Matrix4x4.Invert(TranslateToCenter, out var tInv))
            {
                TranslateToCenterInv = tInv;
            }
            
            UpdateDistances(_lastEyePoint);

            LocatorModified();
        }

        public void UpdateDistances(Vector3 eyePoint)
        {
            _lastEyePoint = eyePoint;

            XValues[0, 0, 0] = XMin;
            YValues[0, 0, 0] = YMin;
            ZValues[0, 0, 0] = ZMin;

            XValues[1, 0, 0] = XMax;
            YValues[1, 0, 0] = YMin;
            ZValues[1, 0, 0] = ZMin;

            XValues[0, 1, 0] = XMin;
            YValues[0, 1, 0] = YMax;
            ZValues[0, 1, 0] = ZMin;

            XValues[0, 0, 1] = XMin;
            YValues[0, 0, 1] = YMin;
            ZValues[0, 0, 1] = ZMax;

            XValues[1, 1, 0] = XMax;
            YValues[1, 1, 0] = YMax;
            ZValues[1, 1, 0] = ZMin;

            XValues[0, 1, 1] = XMin;
            YValues[0, 1, 1] = YMax;
            ZValues[0, 1, 1] = ZMax;

            XValues[1, 0, 1] = XMax;
            YValues[1, 0, 1] = YMin;
            ZValues[1, 0, 1] = ZMax;

            XValues[1, 1, 1] = XMax;
            YValues[1, 1, 1] = YMax;
            ZValues[1, 1, 1] = ZMax;

            MinDist = double.MaxValue;
            MaxDist = double.MinValue;

            for (var z = 0; z < 2; ++z)
            for (var y = 0; y < 2; ++y)
            for (var x = 0; x < 2; ++x)
            {
                var pos = new Vector3(
                    (float) XValues[x, y, z],
                    (float) YValues[x, y, z],
                    (float) ZValues[x, y, z]);

                pos = Vector3.Transform(pos, TranslateToCenterInv*Rotation*TranslateToCenter*_transform);
                
                var dist = System.Math.Abs((pos - eyePoint).Length());

                if (dist < MinDist) MinDist = dist;
                if (dist > MaxDist) MaxDist = dist;

                XValues[x, y, z] = pos.X;
                YValues[x, y, z] = pos.Y;
                ZValues[x, y, z] = pos.Z;
                Values[x, y, z] = dist;
            }
        }

        public static ILevoyCabralLocator Create(IVoxelVolume voxelVolume)
        {
            return new LevoyCabralLocator(voxelVolume);
        }
    }
}