//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Data;
using System.Windows.Data;

namespace Meflook
{
    public interface IEmailService
    {
        XmlDataProvider GetFoldersDataProvider();
        DataView GetEmailList(string folder);
        void MarkAsRead(string folder, int index);
        DataRowView GetEmailMetaData(string folder, int index);
        string GetEmail(string folder, int index);
    }
}
