//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.ComponentModel.Composition;

namespace Microsoft.Samples.MefShapes.Shapes
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class GameShapeAttribute : ExportAttribute, IShapeMetadata
    {
        public GameShapeAttribute(string shapeDescription, int shapePriority)
            : base(typeof(IShape))
        {
            this.ShapeType = ShapeType.GameShape;
            this.ShapeDescription = shapeDescription;
            this.ShapePriority = shapePriority;
        }

        public ShapeType ShapeType { get; private set; }
        public string ShapeDescription { get; private set; }
        public int ShapePriority { get; private set; }
    }
}
