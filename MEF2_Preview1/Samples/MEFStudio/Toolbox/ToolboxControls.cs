//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Contracts;

namespace Toolbox
{
    public class ToolboxControls
    {
        private const string WinForms = "Windows Forms";
        private const string Components = "Components";
        private const string Data = "Data";
        private const string UserControlTab = "UserControl";

        [Export]
        [ToolboxItemMetadata(Category=WinForms)]
        private static Type t1 = typeof(System.Windows.Forms.PropertyGrid);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t2 = typeof(System.Windows.Forms.Label);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t3 = typeof(System.Windows.Forms.LinkLabel);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t4 = typeof(System.Windows.Forms.Button);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t5 = typeof(System.Windows.Forms.TextBox);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t6 = typeof(System.Windows.Forms.RadioButton);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t7 = typeof(System.Windows.Forms.CheckBox);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t8 = typeof(System.Windows.Forms.Panel);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t9 = typeof(System.Windows.Forms.Menu);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t10 = typeof(System.Windows.Forms.GroupBox);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t11 = typeof(System.Windows.Forms.ComboBox);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t12 = typeof(System.Windows.Forms.ListBox);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t13 = typeof(System.Windows.Forms.ListView);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t14 = typeof(System.Windows.Forms.DataGrid);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t15 = typeof(System.Windows.Forms.ToolStrip);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t16 = typeof(System.Windows.Forms.MenuStrip);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t17 = typeof(System.Windows.Forms.StatusStrip);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t18 = typeof(System.Windows.Forms.DataGridView);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t19 = typeof(System.Windows.Forms.TreeView);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t20 = typeof(System.Windows.Forms.RichTextBox);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t21 = typeof(System.Windows.Forms.SplitContainer);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t22 = typeof(System.Windows.Forms.DateTimePicker);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t23 = typeof(System.Windows.Forms.MonthCalendar);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t24 = typeof(System.Windows.Forms.HScrollBar);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t25 = typeof(System.Windows.Forms.VScrollBar);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t26 = typeof(System.Windows.Forms.TrackBar);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t27 = typeof(System.Windows.Forms.PageSetupDialog);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t28 = typeof(System.Windows.Forms.PrintPreviewDialog);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t29 = typeof(System.Windows.Forms.FileDialog);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t30 = typeof(System.Windows.Forms.OpenFileDialog);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t31 = typeof(System.Windows.Forms.ErrorProvider);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t32 = typeof(System.Windows.Forms.Timer);

        [Export]
        [ToolboxItemMetadata(Category = WinForms)]
        private static Type t33 = typeof(System.Windows.Forms.ToolTip);

        [Export]
        [ToolboxItemMetadata(Category = Components)]
        private static Type t34 = typeof(System.IO.FileSystemWatcher);

        [Export]
        [ToolboxItemMetadata(Category = Components)]
        private static Type t35 = typeof(System.Diagnostics.Process);

        [Export]
        [ToolboxItemMetadata(Category = Components)]
        private static Type t36 = typeof(System.Timers.Timer);

        [Export]
        [ToolboxItemMetadata(Category = Data)]
        private static Type t37 = typeof(System.Data.OleDb.OleDbCommandBuilder);

        [Export]
        [ToolboxItemMetadata(Category = Data)]
        private static Type t38 = typeof(System.Data.OleDb.OleDbConnection);

        [Export]
        [ToolboxItemMetadata(Category = Data)]
        private static Type t39 = typeof(System.Data.SqlClient.SqlCommandBuilder);

        [Export]
        [ToolboxItemMetadata(Category = Data)]
        private static Type t40 = typeof(System.Data.SqlClient.SqlConnection);

        [Export]
        [ToolboxItemMetadata(Category = UserControlTab)]
        private static Type t41 = typeof(System.Windows.Forms.UserControl);

    }
}
