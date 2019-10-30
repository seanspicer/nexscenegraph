using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;
using Path = Veldrid.SceneGraph.Util.Shape.Path;

namespace Examples.Common
{
    public class PathExampleScene
    {
        public static IGroup Build()
        {
            var root = Group.Create();

            var pathData = GetPathData();
            
            var path = Path.Create(new Vector3[]
            {
                new Vector3(0.0f,  1.0f, 0.0f),
                new Vector3(0.0f,  0.0f, 0.0f), 
                new Vector3(1.0f,  -1.0f, 0.0f),
                new Vector3(2.0f, -2.0f, 2.0f), 
            });

            var start = 70;
            //path = Path.Create(pathData.ToList().GetRange(start, 3).ToArray());
            path = Path.Create(pathData);

            var hints = TessellationHints.Create();
            hints.SetDetailRatio(4f);
            hints.SetRadius(.1f);
            var pathDrawable = ShapeDrawable<Position3Texture2Color3Normal3>.Create(path, hints);
            
            var redMaterial = PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    new Vector3(1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    5f),
                PhongHeadlight.Create(PhongLightParameters.Create(
                    new Vector3(0.1f, 0.1f, 0.1f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    1f,
                    0)),
                true);

            var pathGeode = Geode.Create();
            pathGeode.AddDrawable(pathDrawable);
            pathGeode.PipelineState = redMaterial.CreatePipelineState();
            pathGeode.PipelineState.RasterizerStateDescription 
                = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);

            root.AddChild(pathGeode);
            return root;
        }

        static Vector3[] GetPathData()
        {
            var dataString = File.ReadAllText(@"C:\Users\sean.spicer.RES\Downloads\badPath.json");
            var json = JObject.Parse(dataString);

            var vecArray = json["Vector3"] as JArray;

            if (null == vecArray)
            {
                return new Vector3[0];
            }

            var result = new Vector3[vecArray.Count];
            var idx = 0;
            foreach (var elt in vecArray)
            {
                if (elt is JObject vecData)
                {
                    var x = vecData["X"].ToObject<double>();
                    var y = vecData["Y"].ToObject<double>();
                    var z = vecData["Z"].ToObject<double>();

                    var vec = new Vector3((float)x,(float)y,(float)z );

                    result[idx] = vec;
                }

                    
                ++idx;
            }

            return result;
        }
    }
}