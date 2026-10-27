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


namespace Veldrid.SceneGraph.Text
{
    public enum CharacterSizeModes
    {
        ObjectCoords,
        ScreenCoords
    }

    public interface ITextNode : IGeometry<VertexPositionTexture>
    {
        string Text { get; set; }

        int Padding { get; set; }

        float FontResolution { get; set; }

        RgbaByte TextColor { get; set; }
        
        RgbaByte OutlineColor { get; set; }
        float OutlineStrokeWidth { get; set; }

        RgbaByte BackgroundColor { get; set; }

        VerticalAlignment VerticalAlignment { get; set; }

        HorizontalAlignment HorizontalAlignment { get; set; }
        
        TextFont Font { get; }

        bool AutoRotateToScreen { get; set; }

        CharacterSizeModes CharacterSizeMode { get; set; }
    }
}