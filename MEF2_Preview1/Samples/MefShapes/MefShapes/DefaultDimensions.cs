//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using Microsoft.Samples.MefShapes.Shapes;

namespace Microsoft.Samples.MefShapes
{
    [Export(typeof(IDimensions))]
    public class DefaultDimensions : IDimensions
    {
        public int BoxWidth { get { return 12; } }
        public int BoxHeight { get { return 25; } }
        public double CellWidth { get { return 30.00; } }
        public double CellHeight { get { return 30.00; } }
        public int StartShapeX { get { return 2; } }
        public int StartShapeY { get { return 0; } }
        public int StartBoxX { get { return 0; } }
        public int StartBoxY { get { return 0; } }
        public int StartShapeSelectionBoxX { get { return 13; } }
        public int StartShapeSelectionBoxY { get { return 1; } }
        public int ShapeSelectionBoxWidth { get { return 13; } }
        public int ShapeSelectionBoxHeight { get { return 23; } }
    }
}
