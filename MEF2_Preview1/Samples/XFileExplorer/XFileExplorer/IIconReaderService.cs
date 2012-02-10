//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace Microsoft.Samples.XFileExplorer
{
    public interface IIconReaderService
    {
        ImageSource GetFileIconImage(string filePath, bool small);
        ImageSource GetFolderIconImage(bool closed, bool small);
        ImageSource GetFolderIconImage(string folderPath, bool closed, bool small);
    }
}
