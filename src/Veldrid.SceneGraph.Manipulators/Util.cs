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

using System.Linq;
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public class Util
    {
        internal static NodePath ComputeNodePathToRoot(INode node)
        {
            var result = new NodePath();
            
            var nodePaths = node.GetParentalNodePaths();
            if (!nodePaths.Any()) return result;
            
            result = nodePaths.First();
            if (nodePaths.Count > 1)
            {
                // TODO: Log this as degenerate case.
            }

            return result;
        }


    }
}