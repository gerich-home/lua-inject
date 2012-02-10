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
    public class CommandMetadataAttribute : Attribute
    {
        public string Language { get; set; }
        public string ItemType { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
    }

    public interface ICommandMetadataView
    {
        string Language { get; }
        string ItemType { get; }
        string Category { get; }
        string Name { get; }
    }
}
