//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace Microsoft.Samples.XFileExplorer
{
    /// <summary>
    /// The ContractTypeAttribute is optional, but it is recommended
    /// when a type is actually used as a contract.
    /// </summary>
    public interface IFavoriteItem
    {
        ImageSource Icon { get; }
        string ItemName { get; }
        Action Command { get; }
    }
}
