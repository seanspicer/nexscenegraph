//
// Copyright 2018-2019 Sean Spicer 
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
                var leftJustifiedXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(0f, 4f, 0f));
                var leftJustifiedText = TextNode.Create(text, Rgba32.White, Rgba32.Red, VerticalAlignment.Top, HorizontalAlignment.Left, 4, 0.75f);
                var leftJustifiedGeode = Geode.Create();
                leftJustifiedGeode.AddDrawable(leftJustifiedText);
                leftJustifiedXForm.AddChild(leftJustifiedGeode);
                root.AddChild(leftJustifiedXForm);  
            }
            
            // Center Justified Text 
            {
                var centerJustifiedXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(0f, 0f, 0f));
                var centerJustifiedText = TextNode.Create(text, Rgba32.Yellow, Rgba32.Blue, VerticalAlignment.Center, HorizontalAlignment.Center, 4, 1.5f);
                var centerJustifiedGeode = Geode.Create();
                centerJustifiedGeode.AddDrawable(centerJustifiedText);
                centerJustifiedXForm.AddChild(centerJustifiedGeode);
                root.AddChild(centerJustifiedXForm);  
            }
            
            // Right Justified Text 
            {
                var rightJustifiedXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(0f, -4f, 0f));
                var rightJustifiedText = TextNode.Create(text, Rgba32.Black, Rgba32.Green, VerticalAlignment.Bottom, HorizontalAlignment.Right, 4, 3);
                var rightJustifiedGeode = Geode.Create();
                rightJustifiedGeode.AddDrawable(rightJustifiedText);
                rightJustifiedXForm.AddChild(rightJustifiedGeode);
                root.AddChild(rightJustifiedXForm);  
            }
            
            return root;
        }
    }
}