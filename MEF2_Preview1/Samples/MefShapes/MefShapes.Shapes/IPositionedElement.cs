//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;

namespace Microsoft.Samples.MefShapes.Shapes
{
    public interface IPositionedElement
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X")]
        int X { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y")]
        int Y { get; set; }
        
        double Top { get; }

        double Left { get; }
        
        double CellHeight { get; }
        
        double CellWidth { get; }
    }
}
