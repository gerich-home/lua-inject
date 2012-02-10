//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace ToolboxLibrary
{
    partial class Toolbox
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
            this.toolboxTitleButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // toolboxTitleButton
            // 
            this.toolboxTitleButton.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.toolboxTitleButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolboxTitleButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.toolboxTitleButton.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.toolboxTitleButton.Location = new System.Drawing.Point(0, 0);
            this.toolboxTitleButton.Name = "toolboxTitleButton";
            this.toolboxTitleButton.Size = new System.Drawing.Size(207, 20);
            this.toolboxTitleButton.TabIndex = 2;
            this.toolboxTitleButton.Text = "Toolbox";
            this.toolboxTitleButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolboxTitleButton.UseVisualStyleBackColor = false;
            // 
            // Toolbox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolboxTitleButton);
            this.Name = "Toolbox";
            this.Size = new System.Drawing.Size(207, 372);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button toolboxTitleButton;
    }
}
