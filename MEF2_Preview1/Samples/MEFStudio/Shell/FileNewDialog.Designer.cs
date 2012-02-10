//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Shell
{
    partial class FileNewDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.languageListView = new System.Windows.Forms.ListView();
            this.itemListView = new System.Windows.Forms.ListView();
            this.OKButton = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.OKButton);
            this.splitContainer1.Size = new System.Drawing.Size(470, 274);
            this.splitContainer1.SplitterDistance = 228;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.languageListView);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.itemListView);
            this.splitContainer2.Size = new System.Drawing.Size(470, 228);
            this.splitContainer2.SplitterDistance = 156;
            this.splitContainer2.TabIndex = 0;
            // 
            // languageListView
            // 
            this.languageListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.languageListView.Location = new System.Drawing.Point(0, 0);
            this.languageListView.Name = "languageListView";
            this.languageListView.Size = new System.Drawing.Size(156, 228);
            this.languageListView.TabIndex = 0;
            this.languageListView.UseCompatibleStateImageBehavior = false;
            this.languageListView.View = System.Windows.Forms.View.List;
            this.languageListView.SelectedIndexChanged += new System.EventHandler(this.languageListView_SelectedIndexChanged);
            // 
            // itemListView
            // 
            this.itemListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.itemListView.Location = new System.Drawing.Point(0, 0);
            this.itemListView.Name = "itemListView";
            this.itemListView.Size = new System.Drawing.Size(310, 228);
            this.itemListView.TabIndex = 0;
            this.itemListView.UseCompatibleStateImageBehavior = false;
            this.itemListView.View = System.Windows.Forms.View.SmallIcon;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(378, 7);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // FileNewDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 274);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FileNewDialog";
            this.Text = "New File Dialog";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListView languageListView;
        private System.Windows.Forms.ListView itemListView;
        private System.Windows.Forms.Button OKButton;

    }
}