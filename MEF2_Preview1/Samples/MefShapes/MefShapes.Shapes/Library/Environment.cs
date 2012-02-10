//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace Microsoft.Samples.MefShapes.Shapes.Library
{
    [BoxShape("Square box shape")]
    public class Environment : Shape
    {
        public override void OnImportsSatisfied()
        {
            if (!IsInitialized)
            {
                this.IsInitialized = true;
                this.Matrix = new Cell[Dimensions.BoxWidth, Dimensions.BoxHeight];
                Initialize(Dimensions.BoxWidth, Dimensions.BoxHeight, false);
                this.CellWidth = Dimensions.CellWidth;
                this.CellHeight = Dimensions.CellHeight;
                EnvironmentShape = this;
            }
        }

        private void Initialize(int width, int height, bool drawTop)
        {
            Color color = Colors.Black;
            for (int y = 0; y < height; y++)
            {
                CellFactory.PositionNewCell(color, Matrix, Cells, 0, y);
                CellFactory.PositionNewCell(color, Matrix, Cells, Matrix.GetLength(0) - 1, y);
            }

            for (int x = 1; x < width - 1; x++)
            {
                CellFactory.PositionNewCell(color, Matrix, Cells, x, Matrix.GetLength(1) - 1);
            }

            if (drawTop)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    CellFactory.PositionNewCell(color, Matrix, Cells, x, 0);
                }
            }
        }
    }
}
