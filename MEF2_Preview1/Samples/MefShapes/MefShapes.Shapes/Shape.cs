//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Windows;

namespace Microsoft.Samples.MefShapes.Shapes
{
    public abstract class Shape : PositionedElement, IShape, IPartImportsSatisfiedNotification
    {
        private static DependencyProperty CellsProperty = DependencyProperty.Register("Cells", typeof(ObservableCollection<Cell>), typeof(Shape));
        protected bool IsInitialized { get; set; }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods"), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public Cell[,] Matrix { get; protected set; }

        protected Shape()
        {
            Cells = new ObservableCollection<Cell>();
        }

        public virtual void OnImportsSatisfied()
        {
            if (!IsInitialized)
            {
                IsInitialized = true;
                this.CellWidth = Dimensions.CellWidth;
                this.CellHeight = Dimensions.CellHeight;
                this.X = Dimensions.StartShapeX + EnvironmentShape.X;
                this.Y = Dimensions.StartShapeY + EnvironmentShape.Y;
            }
        }

        [Import]
        public CellFactory CellFactory { get;  set; }

        [Import("Microsoft.Samples.MefShapes.Shapes.EnvironmentShapeContract")]
        public IShape EnvironmentShape { get; set; }

        [Import]
        public IDimensions Dimensions { get; set; }

        public ObservableCollection<Cell> Cells
        {
            get
            {
                return (ObservableCollection<Cell>)GetValue(CellsProperty);
            }
            private set { SetValue(CellsProperty, value); }
        }

        #region IShape Members

        public void Move(Direction direction)
        {
            if (EnvironmentShape != null)
            {
                int newX = X;
                int newY = Y;

                switch (direction)
                {
                    case Direction.Left:
                        newX--;
                        break;

                    case Direction.Down:
                        newY++;
                        break;

                    case Direction.Right:
                        newX++;
                        break;

                    case Direction.Up:
                        newY--;
                        break;
                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, "Move to {0} is not implemented", direction.ToString()));
                }

                if (IsPositionValid(Matrix, newX, newY))
                {
                    X = newX;
                    Y = newY;
                }
                else if (direction == Direction.Down)
                {
                    MergeIntoEnvironment();
                    int layersFilled = RemoveFilledLayers();
                    if (ReachedBottom != null)
                    {
                        ReachedBottom(this, new ReachedBottomEventArgs(layersFilled));
                    }
                }
            }
        }

        public void Rotate(Direction direction)
        {
            if (EnvironmentShape != null)
            {
                if (Matrix.GetLength(0) != Matrix.GetLength(1))
                    throw new NotSupportedException("Rotation of non-rectangular matrices is not supported");

                int length = Matrix.GetLength(0);
                Cell[,] tempMatrix = new Cell[length, length];

                switch (direction)
                {
                    case Direction.Right:
                      
                        foreach (Cell cell in Cells)
                            tempMatrix[length -1- cell.Y, cell.X] = cell;                      
                        break;
                    default:
                        throw new NotImplementedException();
                }
                if (IsPositionValid(tempMatrix, X, Y))
                {
                    this.Matrix = tempMatrix;
                    for (int x = 0; x < length; x++)
                        for (int y = 0; y < length; y++)
                        {
                            Cell cell = Matrix[x, y];
                            if (cell != null)
                            {
                                cell.X = x;
                                cell.Y = y;
                            }
                        }
                }
            }
        }
      
        private void MergeIntoEnvironment()
        {
         while (Cells.Count > 0)
            {
                Cell cell = Cells[0];
                 this.Matrix[cell.X, cell.Y] = null;
                cell.X = cell.X + X;
                cell.Y = cell.Y + Y;
                EnvironmentShape.Matrix[cell.X, cell.Y] = cell;
                EnvironmentShape.Cells.Add(cell);
                this.Cells.RemoveAt(0);
               
            }
        }

        private int RemoveFilledLayers()
        {
            //TODO: this method assumes the shape of the environment

            int layersFilled = 0;

            int boxHeight = EnvironmentShape.Matrix.GetLength(1) - 1;
            int boxWidth = EnvironmentShape.Matrix.GetLength(0) - 1;

            for (int y = boxHeight-1; y >= 0; y--)
            {
                bool layerFilled = true;
                //start from bottom and move up
                for (int x = 1; x < boxWidth; x++)
                {
                    if (EnvironmentShape.Matrix[x, y] == null)
                    {
                        layerFilled = false;
                        break;
                    }
                }
                if (layerFilled)
                {
                    for (int x = 1; x < boxWidth; x++)
                        ((Shape)EnvironmentShape).RemoveCell(x, y);
                    ((Shape)EnvironmentShape).ShiftCellsDown(boxWidth, y - 1);
                    y++;
                    layersFilled++;
                }
            }

            return layersFilled;
        }

        private void RemoveCell(int x, int y)
        {
            Cell cell = Matrix[x, y];
            Cells.Remove(cell);
            Matrix[x, y] = null;
            cell = null;
        }

        private void ShiftCellsDown(int boxWidth, int startingY)
        {
            for (int y = startingY; y >= 0; y--)
            {
                for (int x = 1; x < boxWidth; x++)
                {
                    if (Matrix[x, y] != null)
                    {
                        Cell cell = Matrix[x, y];
                        cell.Y++;
                        Matrix[x, y + 1] = cell;
                        Matrix[x, y] = null;
                    }
                }
            }
        }        

        private bool IsPositionValid(Cell[,] cellMatrix, int xOffset, int yOffset)
        {
            if (xOffset < 0 || yOffset < 0)
                return false;

            for (int x = 0; x < cellMatrix.GetLength(0); x++)
                for (int y = 0; y < cellMatrix.GetLength(1); y++)
                    if (cellMatrix[x, y] != null && EnvironmentShape.Matrix[x + xOffset, y + yOffset] != null)
                        return false;
            return true;
        }

        public event EventHandler<ReachedBottomEventArgs> ReachedBottom;

        #endregion
    }
}
