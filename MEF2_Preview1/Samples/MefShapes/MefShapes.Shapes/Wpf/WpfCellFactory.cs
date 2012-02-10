//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using Microsoft.Samples.MefShapes.Shapes;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace Microsoft.Samples.MefShapes
{
    [Export(typeof(CellFactory))]
    public class WpfCellFactory : CellFactory
    {
        [Import]
        internal IDimensions dimensions = null;

        public WpfCellFactory() : base() { }

        public override Cell CreateCell(Color color)
        {
            return new WpfCell(color, dimensions.CellWidth, dimensions.CellHeight);
        }
    }
}
