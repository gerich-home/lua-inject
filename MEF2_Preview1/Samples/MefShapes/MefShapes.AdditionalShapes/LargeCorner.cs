//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace Microsoft.Samples.MefShapes.Shapes.Library
{
    [GameShape("3 by 3 corner shape", 0)]
    public class LargeCorner : RegularShape
    {
        protected override Cell[,] CreateMatrix(Color color, CellFactory cellFactory)
        {
            Cell[,] matrix = new Cell[4, 4];
            cellFactory.PositionNewCell(color, matrix, Cells, 0, 0);
            cellFactory.PositionNewCell(color, matrix, Cells, 1, 0);
            cellFactory.PositionNewCell(color, matrix, Cells, 2, 0);
            cellFactory.PositionNewCell(color, matrix, Cells, 2, 1);
            cellFactory.PositionNewCell(color, matrix, Cells, 2, 2);
            return matrix;
        }
    }
}
