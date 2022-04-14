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
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.Text;
using Color = SixLabors.ImageSharp.Color;

namespace Examples.Common
{
    public class TextRenderingExampleScene
    {
        public static IGroup Build()
        {
            var root = Group.Create();

            var text = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor";

            // Left Justified Text 
            {
                var leftJustifiedXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(0f, 60f, 0f));
                var leftJustifiedText = TextNode.Create(text, 20f, Color.White, Color.Red, VerticalAlignment.Top,
                    HorizontalAlignment.Left, 4, 0.75f);
                leftJustifiedText.AutoRotateToScreen = true;
                leftJustifiedText.CharacterSizeMode = CharacterSizeModes.ObjectCoords;
                var leftJustifiedGeode = Geode.Create();
                leftJustifiedGeode.AddDrawable(leftJustifiedText);
                leftJustifiedXForm.AddChild(leftJustifiedGeode);
                root.AddChild(leftJustifiedXForm);
            }

            // Center Justified Text 
            {
                var centerJustifiedXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(0f, 0f, 0f));
                var centerJustifiedText = TextNode.Create(text, 40f, Color.Yellow, Color.Blue,
                    VerticalAlignment.Center, HorizontalAlignment.Center, 4, 1.5f);
                centerJustifiedText.AutoRotateToScreen = false;
                centerJustifiedText.CharacterSizeMode = CharacterSizeModes.ObjectCoords;
                var centerJustifiedGeode = Geode.Create();
                centerJustifiedGeode.AddDrawable(centerJustifiedText);
                centerJustifiedXForm.AddChild(centerJustifiedGeode);
                root.AddChild(centerJustifiedXForm);
            }

            // Right Justified, Screen coordinate scaled Text 
            {
                var rightJustifiedXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(0f, -60f, 0f));
                var rightJustifiedText = TextNode.Create(text, 20f, Color.Black, Color.Green,
                    VerticalAlignment.Bottom, HorizontalAlignment.Right, 4, 3);
                rightJustifiedText.AutoRotateToScreen = true;
                rightJustifiedText.CharacterSizeMode = CharacterSizeModes.ScreenCoords;
                var rightJustifiedGeode = Geode.Create();
                rightJustifiedGeode.AddDrawable(rightJustifiedText);
                rightJustifiedXForm.AddChild(rightJustifiedGeode);
                root.AddChild(rightJustifiedXForm);
            }

            return root;
        }
    }
}