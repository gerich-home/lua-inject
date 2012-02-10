//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace Microsoft.Samples.MefShapes.Shapes
{
    public abstract class CellFactory
    {
        #region CellFactory Members

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y"), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        public void PositionNewCell(Color color, Cell[,] matrix, Collection<Cell> cells, int x, int y)
        {
            Cell cell = CreateCell(color);
            cell.X = x;
            cell.Y = y;
            matrix[x, y] = cell;
            cells.Add(cell);
        }

        #endregion

        public abstract Cell CreateCell(Color color);
    }
}
