//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace Contracts
{
    [MetadataAttribute]
    public class ToolboxItemMetadataAttribute : Attribute
    {
        public string Category { get; set; }
    }

    public interface IToolboxItemMetadataView
    {
        string Category { get; }
    }
}
