//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Windows.Media;

namespace Microsoft.Samples.MefShapes.Shapes.Library
{
    public abstract class RegularShape : Shape
    {
        private static Random rand = new Random((int)DateTime.Now.Ticks);
        private Color shapeColor;

        protected RegularShape()
        {
            byte[] colorBuffer = new byte[3];
            rand.NextBytes(colorBuffer);
            shapeColor = Color.FromRgb(colorBuffer[0], colorBuffer[1], colorBuffer[2]);
        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            this.Matrix = CreateMatrix(shapeColor, CellFactory);
        }

        protected abstract Cell[,] CreateMatrix(Color color, CellFactory cellFactory);
    }
}