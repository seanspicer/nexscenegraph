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

using System.Drawing;
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ITranslatePlaneDragger : ICompositeDragger
    {
        public Translate1DDragger Translate1DDragger { get; }
        public Translate2DDragger Translate2DDragger { get; }
        void SetColor(Color color);
    }
    
    public class TranslatePlaneDragger : CompositeDragger, ITranslatePlaneDragger
    {
        public Translate1DDragger Translate1DDragger { get; protected set; }
        public Translate2DDragger Translate2DDragger { get; protected set; }
        protected bool UsingTranslate1DDragger { get; set; }
        
        protected TranslatePlaneDragger(Matrix4x4 matrix) : base(matrix)
        {
        }

        public override void SetupDefaultGeometry()
        {
            throw new System.NotImplementedException();
        }

        public void SetColor(Color color)
        {
            throw new System.NotImplementedException();
        }
    }
}