//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.ComponentModel.Composition;

namespace Microsoft.Samples.MefShapes.Shapes
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class BoxShapeAttribute : ExportAttribute, IShapeMetadata
    {
        public const string BoxShapeContractName = "Microsoft.Samples.MefShapes.Shapes.EnvironmentShapeContract";

        public BoxShapeAttribute(string shapeDescription)
            : base(BoxShapeContractName, typeof(IShape))
        {
            this.ShapeType = ShapeType.BoxShape;
            this.ShapeDescription = shapeDescription;
            this.ShapePriority = 0;
        }

        public ShapeType ShapeType { get; private set; }
        public string ShapeDescription { get; private set; }
        public int ShapePriority { get; private set; }
    }
}
