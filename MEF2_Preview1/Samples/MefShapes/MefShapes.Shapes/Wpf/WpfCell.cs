//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Samples.MefShapes
{
    public class WpfCell : Cell
    {
        private static readonly  DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(WpfCell));
       
        public WpfCell(Color color, double width, double height)
        {
            this.Color = color;
            this.CellWidth = width;
            this.CellHeight = height;
        }

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
    }
}
