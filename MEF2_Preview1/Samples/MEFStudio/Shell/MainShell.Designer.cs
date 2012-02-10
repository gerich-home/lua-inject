//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Shell
{
    partial class MainShell
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
            this.components = new System.ComponentModel.Container();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.fileMenuItem = new System.Windows.Forms.MenuItem();
            this.newMenuItem = new System.Windows.Forms.MenuItem();
            this.openMenuItem = new System.Windows.Forms.MenuItem();
            this.refreshMenuItem = new System.Windows.Forms.MenuItem();
            this.exitMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.viewCodeMenuItem = new System.Windows.Forms.MenuItem();
            this.toolsMenuItem = new System.Windows.Forms.MenuItem();
            this.openMenuItem1 = new System.Windows.Forms.MenuItem();
            this.hMenuItem = new System.Windows.Forms.MenuItem();
            this.abMenuItem = new System.Windows.Forms.MenuItem();
            this.StatusBar = new System.Windows.Forms.StatusBar();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fileMenuItem,
            this.menuItem1,
            this.toolsMenuItem,
            this.hMenuItem});
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.Index = 0;
            this.fileMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.newMenuItem,
            this.openMenuItem,
            this.refreshMenuItem,
            this.exitMenuItem});
            this.fileMenuItem.Text = "&File";
            // 
            // newMenuItem
            // 
            this.newMenuItem.Index = 0;
            this.newMenuItem.Text = "&New";
            this.newMenuItem.Click += new System.EventHandler(this.newMenuItem_Click);
            // 
            // openMenuItem
            // 
            this.openMenuItem.Index = 1;
            this.openMenuItem.Text = "&Open";
            // 
            // refreshMenuItem
            // 
            this.refreshMenuItem.Index = 2;
            this.refreshMenuItem.Text = "Re&fresh";
            this.refreshMenuItem.Click += new System.EventHandler(this.refreshMenuItem_Click);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Index = 3;
            this.exitMenuItem.Text = "E&xit";
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 1;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.viewCodeMenuItem});
            this.menuItem1.Text = "&View";
            // 
            // viewCodeMenuItem
            // 
            this.viewCodeMenuItem.Index = 0;
            this.viewCodeMenuItem.Text = "C&ode";
            this.viewCodeMenuItem.Click += new System.EventHandler(this.viewCodeMenuItem_Click);
            // 
            // toolsMenuItem
            // 
            this.toolsMenuItem.Index = 2;
            this.toolsMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.openMenuItem1});
            this.toolsMenuItem.Text = "&Tools";
            // 
            // openMenuItem1
            // 
            this.openMenuItem1.Index = 0;
            this.openMenuItem1.Text = "&Options...";
            // 
            // hMenuItem
            // 
            this.hMenuItem.Index = 3;
            this.hMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.abMenuItem});
            this.hMenuItem.Text = "&Help";
            // 
            // abMenuItem
            // 
            this.abMenuItem.Index = 0;
            this.abMenuItem.Text = "&About";
            // 
            // StatusBar
            // 
            this.StatusBar.Location = new System.Drawing.Point(0, 381);
            this.StatusBar.Name = "StatusBar";
            this.StatusBar.Size = new System.Drawing.Size(576, 22);
            this.StatusBar.TabIndex = 2;
            this.StatusBar.Text = "Ready";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(576, 381);
            this.splitContainer1.SplitterDistance = 103;
            this.splitContainer1.TabIndex = 3;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer2.Size = new System.Drawing.Size(469, 381);
            this.splitContainer2.SplitterDistance = 352;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.tabControl1);
            this.splitContainer3.Size = new System.Drawing.Size(352, 381);
            this.splitContainer3.SplitterDistance = 289;
            this.splitContainer3.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(352, 289);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainer4.Size = new System.Drawing.Size(113, 381);
            this.splitContainer4.SplitterDistance = 179;
            this.splitContainer4.TabIndex = 0;
            // 
            // MainShell
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 403);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.StatusBar);
            this.Menu = this.mainMenu1;
            this.Name = "MainShell";
            this.Text = "MEF Studio";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem fileMenuItem;
        private System.Windows.Forms.MenuItem newMenuItem;
        private System.Windows.Forms.MenuItem openMenuItem;
        private System.Windows.Forms.MenuItem refreshMenuItem;
        private System.Windows.Forms.MenuItem toolsMenuItem;
        private System.Windows.Forms.MenuItem openMenuItem1;
        private System.Windows.Forms.MenuItem hMenuItem;
        private System.Windows.Forms.MenuItem abMenuItem;
        public System.Windows.Forms.StatusBar StatusBar;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.MenuItem exitMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem viewCodeMenuItem;
    }
}

