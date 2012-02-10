//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.ComponentModel.Composition;

namespace Microsoft.Samples.MefShapes.Shapes
{
    public interface IShapeMetadata
    {
        ShapeType ShapeType { get; }

        string ShapeDescription { get; }
        
        /// <summary>
        /// This value can be used in shape frequency algorithms
        /// </summary>
        int ShapePriority { get; }
    }
}
