
using System.Collections.Generic;

namespace Veldrid.SceneGraph.InputAdapter
{
    public class PointerData
    {
        public IObject Object;
        public float X, XMin, XMax;
        public float Y, YMin, YMax;

        public PointerData(IObject obj, float x, float xMin, float xMax, float y, float yMin, float yMax)
        {
            Object = obj;
            X = x;
            XMin = xMin;
            XMax = xMax;
            Y = y;
            YMin = yMin;
            YMax = yMax;
        }

        public float GetXNormalized()
        {
            return (X - XMin) / (XMax - XMin) * 2.0f - 1.0f;
        }

        public float GetYNormalized()
        {
            return (Y - YMin) / (YMax - YMin) * 2.0f - 1.0f;
        }
    }

    public class PointerDataList : List<PointerData>
    {
    }
}