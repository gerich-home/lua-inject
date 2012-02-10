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
    [GameShape("Diagonal shape", 0)]
    public class Diagonal : RegularShape
    {
        protected override Cell[,] CreateMatrix(Color color, CellFactory cellFactory)
        {
            Cell[,] matrix = new Cell[3, 3];
            cellFactory.PositionNewCell(color, matrix, Cells, 0, 0);
            cellFactory.PositionNewCell(color, matrix, Cells, 1, 1);
            cellFactory.PositionNewCell(color, matrix, Cells, 2, 2);
            return matrix;
        }
    }
}
