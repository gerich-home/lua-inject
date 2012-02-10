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
    public class DesignerMetadataAttribute : Attribute
    {
        public string Language { get; set; }
        public string ItemType { get; set; }
        public string FileExtension { get; set; }
    }

    public interface IDesignerMetadataView
    {
        string Language { get; }
        string ItemType { get; }
        string FileExtension { get; }
    }
}
