//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.MefShapes.Shapes.Library
{
    public class PictureShape : RegularShape
    {
        private string Picture;
        public PictureShape(string picture)
        {
            Picture = picture;
        }

        protected override Cell[,] CreateMatrix(System.Windows.Media.Color color, CellFactory cellFactory)
        {
            string[] lines = Picture.Split('/');
            int height = lines.Length;
            int width = lines[0].Length;
            Cell[,] matrix = new Cell[width, height];
            for (int row = 0; row < height; row++)
            {
                if (lines[row].Length != width)
                {
                    throw new FormatException(string.Format("Each line must be the same length ({0})", width));
                }
                for (int col = 0; col < width; col++)
                {
                    char ch = lines[row][col];
                    if (ch == '1')
                    {
                        cellFactory.PositionNewCell(color, matrix, Cells, col, row);
                    }
                }
            }
            return matrix;
        }
    }

    [GameShape("T shape", 0)]
    internal class GameTShape : PictureShape
    {
        public GameTShape() : base("010/111/000") { }
    }

    [GameShape("L shape", 0)]
    internal class GameLShape : PictureShape
    {
        public GameLShape() : base("010/010/011") { }
    }

    [GameShape("J shape", 0)]
    internal class GameJShape : PictureShape
    {
        public GameJShape() : base("010/010/110") { }
    }

    [GameShape("I shape", 0)]
    internal class GameIShape : PictureShape
    {
        public GameIShape() : base("1000/1000/1000/1000") { }
    }

    [GameShape("Square shape", 0)]
    internal class GameSquareShape : PictureShape
    {
        public GameSquareShape() : base("11/11") { }
    }

    [GameShape("S shape", 0)]
    internal class GameSShape : PictureShape
    {
        public GameSShape() : base("011/110/000") { }
    }

    [GameShape("Z shape", 0)]
    internal class GameZShape : PictureShape
    {
        public GameZShape() : base("110/011/000") { }
    }
}
