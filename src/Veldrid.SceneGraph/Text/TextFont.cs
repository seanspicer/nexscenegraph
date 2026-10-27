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
    /// <summary>
    ///     Describes the font a <see cref="ITextNode" /> is rendered with. <see cref="Size" /> is in points
    ///     (at 72 DPI, i.e. pixels before <see cref="ITextNode.FontResolution" /> is applied). If the family is not
    ///     installed the platform's default font is used.
    /// </summary>
    public sealed record TextFont(string Family, float Size, bool Bold = false, bool Italic = false);

    public enum HorizontalAlignment
    {
        Left,
        Right,
        Center
    }

    public enum VerticalAlignment
    {
        Top,
        Bottom,
        Center
    }
}
