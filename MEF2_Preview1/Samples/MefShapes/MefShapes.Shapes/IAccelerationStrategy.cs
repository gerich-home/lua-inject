//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.ComponentModel.Composition;

namespace Microsoft.Samples.MefShapes.Shapes
{
    public interface IAccelerationStrategy
    {
        double Acceleration { get; }
    }
}
