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
    [GameShape("3 by 1 stick shape", 0)]
    public class TripleStick : RegularShape
    {
        protected override Cell[,] CreateMatrix(Color color, CellFactory cellFactory)
        {
            Cell[,] matrix = new Cell[3, 3];
            cellFactory.PositionNewCell(color, matrix, Cells, 0, 0);
            cellFactory.PositionNewCell(color, matrix, Cells, 0, 1);
            cellFactory.PositionNewCell(color, matrix, Cells, 0, 2);
            return matrix;
        }
    }
}
