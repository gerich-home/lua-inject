//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.Samples.MefShapes.Shapes.Library;

namespace Microsoft.Samples.MefShapes.Shapes
{
    public interface IShape
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        Cell[,] Matrix { get; }   
 
        ObservableCollection<Cell> Cells { get; }
        
        void Move(Direction direction);
        
        void Rotate(Direction direction);
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X")]
        int X { get; }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y")]
        int Y { get; }

        event EventHandler<ReachedBottomEventArgs> ReachedBottom;
    }
}