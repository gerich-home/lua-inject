//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Microsoft.Samples.XFileExplorerComponents
{
    public abstract class PreviewControl : UserControl
    {
        public abstract void UpdatePreview();
    }

    public interface IPreviewMetadata
    {
        IList<string> Format { get; }
    }
}
