//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using Microsoft.Samples.MefShapes.Shapes;

namespace Microsoft.Samples.MefShapes
{
    [Export(typeof(IAccelerationStrategy))]
    public class DefaultAccelerationStrategy : IAccelerationStrategy
    {
        public DefaultAccelerationStrategy()
        {
            this.Acceleration = 1.1;
        }

        public double Acceleration { get; private set; }
    }
}
