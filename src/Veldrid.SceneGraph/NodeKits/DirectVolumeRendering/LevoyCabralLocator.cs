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
using Veldrid.SceneGraph.Math.IsoSurface;

namespace Veldrid.SceneGraph.NodeKits.DirectVolumeRendering
{
    public interface ILevoyCabralLocator : ILocator, IVoxelVolume
    {
        public double MinDist { get; }
        public double MaxDist { get; }
        public void UpdateDistances(Vector3 eyePoint);
        Vector3 TexGen(Vector3 point);
        
        public double XMin { get; }
        public double XMax { get; }
        
        public double YMin { get; }
        public double YMax { get; }
        
        public double ZMin { get; }
        public double ZMax { get; }
    }
    
    public class LevoyCabralLocator : Locator, ILevoyCabralLocator
    {
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

        private IVoxelVolume _source;
        
        public double MinDist { get; private set; }
        public double MaxDist { get; private set; }

        private Vector3 _lastEyePoint = Vector3.Zero;
        
        public static ILevoyCabralLocator Create(IVoxelVolume voxelVolume)
        {
            return new LevoyCabralLocator(voxelVolume);
        }
        
        protected LevoyCabralLocator(IVoxelVolume voxelVolume) 
        {
            _source = voxelVolume;

            var xdim = _source.XValues.GetLength(0)-1;
            var ydim = _source.XValues.GetLength(1)-1;
            var zdim = _source.XValues.GetLength(2)-1;
            
            Values = new double[2, 2, 2];
            XValues = new double[2, 2, 2];
            YValues = new double[2, 2, 2];
            ZValues = new double[2, 2, 2];

            XMin = _source.XValues[0, 0, 0];
            XMax = _source.XValues[xdim, 0, 0];

            YMin = _source.YValues[0, 0, 0];
            YMax = _source.YValues[0, ydim, 0];

            ZMin = _source.ZValues[0, 0, 0];
            ZMax = _source.ZValues[0, 0, zdim];
            
            UpdateDistances(Vector3.Zero);
            
            SetTransformAsExtents(
                (float) XMin, 
                (float) XMax, 
                (float) YMin, 
                (float) YMax, 
                (float) ZMin, 
                (float) ZMax);
        }
        
        public Vector3 TexGen(Vector3 point)
        {
            return point;
            
            return new Vector3(
                (float)((point.X - XMin) / (XMax - XMin)),
                (float)((point.Y - YMin) / (YMax - YMin)),
                (float)((point.Z - ZMin) / (ZMax - ZMin))
            );
        }

        public override void SetTransform(Matrix4x4 transform)
        {
            _transform = transform;
            if (Matrix4x4.Invert(_transform, out var inverse))
            {
                _inverse = inverse;
            }

            XMin = Transform.M41;
            YMin = Transform.M42;
            ZMin = Transform.M43;

            XMax = Transform.M11 + XMin;
            YMax = Transform.M22 + YMin;
            ZMax = Transform.M33 + ZMin;
            
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
            {
                for (var y = 0; y < 2; ++y)
                {
                    for (var x = 0; x < 2; ++x)
                    {
                        var pos = new Vector3(
                            (float) XValues[x, y, z],
                            (float) YValues[x, y, z],
                            (float) ZValues[x, y, z]);
                        
                        var dist = System.Math.Abs((pos - eyePoint).Length());

                        if (dist < MinDist) MinDist = dist;
                        if (dist > MaxDist) MaxDist = dist;
                        
                        Values[x, y, z] = dist;
                        
                    }
                }
            }
        }
    }
}