//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Editor.SourceEditing;

namespace Shell
{
    public partial class CodeEditorControl : UserControl
    {
        TextControl editor = new TextControl();
        string language = null;

        public CodeEditorControl()
        {
            InitializeComponent();
            editor.Dock = DockStyle.Fill;
            this.Controls.Add(editor);
        }

        public void ShowText(string text, string language)
        {
            this.language = language;
            if (language == "C#")
                editor.Language = Editor.CodePackage.Code.SupportedLanguages.CSharp;
            else if (language == "VB")
                editor.Language = Editor.CodePackage.Code.SupportedLanguages.VisualBasic;
            else if (language == "XML")
                editor.Language = Editor.CodePackage.Code.SupportedLanguages.HTML;
            else
                editor.Language = Editor.CodePackage.Code.SupportedLanguages.None;
            editor.Text = text;
        }

        public string Language
        {
            get
            {
                return language;
            }
        }
    }
}
