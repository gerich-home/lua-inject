//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.ComponentModel.Composition;

namespace Microsoft.Samples.MefShapes.Shapes
{
    public  interface IDimensions
    {
        int BoxWidth { get; }
        int BoxHeight { get; }
        double CellWidth { get; }
        double CellHeight { get; }
        int StartShapeX { get; }
        int StartShapeY { get; }
        int StartBoxX { get; }
        int StartBoxY { get; }
        int StartShapeSelectionBoxX { get; }
        int StartShapeSelectionBoxY { get; }
        int ShapeSelectionBoxWidth { get; }
        int ShapeSelectionBoxHeight { get; }
    }
}
