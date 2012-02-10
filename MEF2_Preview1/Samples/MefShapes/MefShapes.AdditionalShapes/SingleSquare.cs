//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.Samples.MefShapes.Shapes.Library;
using Microsoft.Samples.MefShapes.Shapes;

namespace Microsoft.Samples.MefShapes.AdditionalShapes
{
    [GameShape("1 by 1 square shape", 0)]
    public class SingleSquare : RegularShape
    {
        protected override Cell[,] CreateMatrix(Color color, CellFactory cellFactory)
        {
            Cell[,] matrix = new Cell[1, 1];
            cellFactory.PositionNewCell(color, matrix, Cells, 0, 0);
            return matrix;
        }
    }
}
