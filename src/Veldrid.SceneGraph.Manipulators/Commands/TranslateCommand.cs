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

namespace Veldrid.SceneGraph.Manipulators.Commands
{
    public interface ITranslateCommand : IMotionCommand
    {
        Vector3 Translation { get; set; }
    }
    
    public abstract class TranslateCommand : MotionCommand
    {
        public Vector3 Translation { get; set; }
        
        public override Matrix4x4 GetMotionMatrix()
        {
            return Matrix4x4.CreateTranslation(Translation);
        }
        
        protected void SetInverseProperties(ITranslateCommand inverse)
        {
            inverse.Translation = -Translation;
            inverse.SetLocalToWorldAndWorldToLocal(GetLocalToWorld(), GetWorldToLocal());
            inverse.Stage = Stage;
        }
    }
}