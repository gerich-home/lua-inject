//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;

namespace Meflook
{
    public interface IShellViewMetadata
    {
        int Index { get; }
    }

    [MetadataAttribute]
    public class ShellViewMetadataAttribute : Attribute
    {
        public int Index { get; private set; }

        public ShellViewMetadataAttribute(int index)
        {
            this.Index = index;
        }
    }
}
