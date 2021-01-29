//
// This file is part of IMAGEFrac (R) and related technologies.
//
// Copyright (c) 2017-2020 Reveal Energy Services.  All Rights Reserved.
//
// LEGAL NOTICE:
// IMAGEFrac contains trade secrets and otherwise confidential information
// owned by Reveal Energy Services. Access to and use of this information is 
// strictly limited and controlled by the Company. This file may not be copied,
// distributed, or otherwise disclosed outside of the Company's facilities 
// except under appropriate precautions to maintain the confidentiality hereof, 
// and may not be used in any way not expressly authorized by the Company.
//

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