//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;

namespace Microsoft.Samples.MefShapes.Shapes
{
    public class ReachedBottomEventArgs : EventArgs
    {
        public ReachedBottomEventArgs(int layersRemoved)
        {
            this.LayersRemoved = layersRemoved;
        }

        public int LayersRemoved { get; private set; }
    }
}
