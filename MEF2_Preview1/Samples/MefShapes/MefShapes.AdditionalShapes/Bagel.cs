//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using Microsoft.Samples.MefShapes.Shapes;
using Microsoft.Samples.MefShapes.Shapes.Library;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace Microsoft.Samples.MefShapes.AdditionalShapes
{
    [GameShape("Shape with a hole", 0)]
    public class Bagel : RegularShape
    {
        protected override Cell[,] CreateMatrix(Color color, CellFactory cellFactory)
        {
            Cell[,] matrix = new Cell[3, 3];
            cellFactory.PositionNewCell(color, matrix, Cells, 0, 0);
            cellFactory.PositionNewCell(color, matrix, Cells, 0, 1);
            cellFactory.PositionNewCell(color, matrix, Cells, 0, 2);
            cellFactory.PositionNewCell(color, matrix, Cells, 2, 0);
            cellFactory.PositionNewCell(color, matrix, Cells, 2, 1);
            cellFactory.PositionNewCell(color, matrix, Cells, 2, 2);
            cellFactory.PositionNewCell(color, matrix, Cells, 1, 0);
            cellFactory.PositionNewCell(color, matrix, Cells, 1, 2);
            return matrix;
        }
    }
}
