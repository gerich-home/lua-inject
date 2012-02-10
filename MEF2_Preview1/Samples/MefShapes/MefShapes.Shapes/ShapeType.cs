//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.ComponentModel.Composition;

namespace Microsoft.Samples.MefShapes.Shapes
{
    public enum ShapeType
    {
        /// <summary>
        /// Game shape, e.g. stick or square or corner
        /// </summary>
        GameShape,

        /// <summary>
        /// Box shape, i.e. the the box where the shapes get dropped to
        /// </summary>
        BoxShape,
    }
}
