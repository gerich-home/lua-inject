//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace ToolWindows
{
    partial class OutputWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.RichTextBox = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // RichTextBox
            // 
            this.RichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RichTextBox.Location = new System.Drawing.Point(0, 16);
            this.RichTextBox.Name = "RichTextBox";
            this.RichTextBox.Size = new System.Drawing.Size(367, 191);
            this.RichTextBox.TabIndex = 4;
            this.RichTextBox.Text = "";
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(367, 16);
            this.label1.TabIndex = 3;
            this.label1.Text = "Output";
            // 
            // OutputWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.RichTextBox);
            this.Controls.Add(this.label1);
            this.Name = "OutputWindow";
            this.Size = new System.Drawing.Size(367, 207);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox RichTextBox;
        private System.Windows.Forms.Label label1;
    }
}
