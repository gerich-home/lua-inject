//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Windows;

namespace Microsoft.Samples.MefShapes.Shapes
{
    public abstract class PositionedElement : DependencyObject, IPositionedElement
    {
        private const int MaxCellsCount = 5;
        private const int CenterCoefficient = 4;

        private static DependencyProperty XProperty = DependencyProperty.Register("X", typeof(int), typeof(PositionedElement));
        private static DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(int), typeof(PositionedElement));
        private static readonly DependencyProperty TopProperty = DependencyProperty.Register("Top", typeof(double), typeof(PositionedElement));
        private static readonly DependencyProperty LeftProperty = DependencyProperty.Register("Left", typeof(double), typeof(PositionedElement));
        private static readonly DependencyProperty HeightProperty = DependencyProperty.Register("CellHeight", typeof(double), typeof(PositionedElement));
        private static readonly DependencyProperty WidthProperty = DependencyProperty.Register("CellWidth", typeof(double), typeof(PositionedElement));

        public double Top
        {
            get { return (double)GetValue(TopProperty); }
            protected set { SetValue(TopProperty, value); }
        }

        public double Left
        {
            get { return (double)GetValue(LeftProperty); }
            protected set { SetValue(LeftProperty, value); }
        }

        public double CellHeight
        {
            get { return (double)GetValue(HeightProperty); }
            protected set { SetValue(HeightProperty, value); }
        }

        public double CellWidth
        {
            get { return (double)GetValue(WidthProperty); }
            protected set { SetValue(WidthProperty, value); }
        }

        public double ShapeWidth { get { return CellWidth * MaxCellsCount; } }

        public double ShapeHeight { get { return CellHeight * MaxCellsCount; } }

        public double ShapeCenterX { get { return ShapeWidth / CenterCoefficient; } }

        public double ShapeCenterY { get { return ShapeHeight / CenterCoefficient; } }

        public virtual int X
        {
            get { return (int)GetValue(XProperty); }
            set
            {
                SetValue(XProperty, value);
                Left = CellWidth * value;
            }
        }

        public virtual int Y
        {
            get { return (int)GetValue(YProperty); }
            set
            {
                SetValue(YProperty, value);
                Top = CellHeight * value;
            }
        }
    }
}
