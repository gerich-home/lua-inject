//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using Contracts;

namespace Shell
{
    [Export]
    public partial class MainShell : Form, IPartImportsSatisfiedNotification
    {
        [Import]
        private IToolbox toolbox = null;

        [Import]
        private IOutputWindow outputWindow = null;

        [Import]
        private IPropertyGrid propertyGrid = null;

        [Import]
        private ISolutionExplorer solutionExplorer = null;

        [Import]
        private FileNewDialog fileNewDialog = null;

        [Import("RefreshExtensions")]
        private Action refresh = null;

        private Dictionary<string, int> designerCounts = new Dictionary<string, int>();
        private List<MenuItem> menuItems = new List<MenuItem>();

        public MainShell()
        {
            InitializeComponent();
        }

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            toolbox.View.Parent = splitContainer1.Panel1;
            toolbox.View.Dock = DockStyle.Fill;
            outputWindow.View.Parent = splitContainer3.Panel2;
            outputWindow.View.Dock = DockStyle.Fill;
            solutionExplorer.View.Parent = splitContainer4.Panel1;
            solutionExplorer.View.Dock = DockStyle.Fill;
            propertyGrid.View.Parent = splitContainer4.Panel2;
            propertyGrid.View.Dock = DockStyle.Fill;
        }

        #endregion

        private void refreshMenuItem_Click(object sender, EventArgs e)
        {
            refresh();
            toolbox.RefreshControls();
	        if(this.tabControl1.TabPages==null || this.tabControl1.TabPages.Count<=0)
		        return;
            var host = this.tabControl1.SelectedTab.Controls[0] as HostControl;
            if (host != null)
            {
                HostSurface hostSurface = host.HostSurface;
                SetupMenus(hostSurface);
            }
        }

        private void newMenuItem_Click(object sender, EventArgs e)
        {
            if(fileNewDialog.ShowDialog()==DialogResult.OK)
                AddDesigner();
        }

        private void AddDesigner()
        {
            Lazy<HostSurfaceFactory, IDesignerMetadataView> exportSurfaceFactory = fileNewDialog.GetHostFactory();
            HostControl hc = new HostControl();
            if (designerCounts.ContainsKey(exportSurfaceFactory.Metadata.ItemType))
                designerCounts[exportSurfaceFactory.Metadata.ItemType]++;
            else
                designerCounts.Add(exportSurfaceFactory.Metadata.ItemType, 1);
            string siteName = exportSurfaceFactory.Metadata.ItemType + designerCounts[exportSurfaceFactory.Metadata.ItemType].ToString();
            HostSurface hostSurface = exportSurfaceFactory.Value.CreateNew(siteName);
            hc.InitializeHost(hostSurface);
            string fileName = siteName + "." + exportSurfaceFactory.Metadata.FileExtension;
            TabPage tabpage = new TabPage(fileName + " - Design");
            tabpage.Tag = exportSurfaceFactory.Metadata.Language;
            hc.Parent = tabpage;
            hc.Dock = DockStyle.Fill;
            this.tabControl1.TabPages.Add(tabpage);
            this.tabControl1.SelectedIndex = this.tabControl1.TabPages.Count - 1;
            this.outputWindow.Writeline("Opened new host.");
            this.toolbox.DesignerHost = hostSurface.DesignerHost;
            this.solutionExplorer.AddFileNode(fileName);
            SetupMenus(hostSurface);
        }

        private void SetupMenus(HostSurface hostSurface)
        {
            foreach (MenuItem mi in menuItems)
                mi.Dispose();
            menuItems.Clear();
            if (hostSurface == null)
                return;
            IEnumerable<Lazy<Action<HostSurface>, ICommandMetadataView>> commands = hostSurface.HostSurfaceFactory.GetCommands();
            foreach (var command in commands)
            {
                bool found = false;
                MenuItem topMenu = null;
                foreach (MenuItem mi in this.mainMenu1.MenuItems)
                {
                    if (mi.Text == command.Metadata.Category)
                    {
                        found = true;
                        topMenu = mi;
                    }
                }
                if(!found)
                {
                    topMenu = new MenuItem(command.Metadata.Category);
                    this.mainMenu1.MenuItems.Add(topMenu);
                    menuItems.Add(topMenu);
                }
                MenuItem realItem = new MenuItem(command.Metadata.Name);
                topMenu.MenuItems.Add(realItem);
                menuItems.Add(realItem);
                realItem.Tag = new CommandAction(command.Value, hostSurface);
                realItem.Click += new EventHandler(realItem_Click);
            }
        }

        void realItem_Click(object sender, EventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            CommandAction ca = mi.Tag as CommandAction;
            ca.Action(ca.HostSurface);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl1.SelectedTab.Controls[0] is HostControl)
            {
                // Clicked on designer tab
                HostSurface hostSurface = (this.tabControl1.SelectedTab.Controls[0] as HostControl).HostSurface;
                this.toolbox.DesignerHost = hostSurface.DesignerHost;
                SetupMenus(hostSurface);
                this.propertyGrid.SetSelectedObjects(new object[] {hostSurface.DesignerHost.RootComponent});
            }
            else
            {
                // Clicked on Code tab
                HostSurface hostSurface = tabControl1.SelectedTab.Tag as HostSurface;
                CodeEditorControl cec = tabControl1.SelectedTab.Controls[0] as CodeEditorControl;
                cec.ShowText(hostSurface.GetCode(), cec.Language);
                SetupMenus(null);
                this.propertyGrid.SetSelectedObjects(null);
            }
        }

        private void viewCodeMenuItem_Click(object sender, EventArgs e)
        {
            if (!(this.tabControl1.SelectedTab.Controls[0] is HostControl))
                return;
            HostSurface hostSurface = (this.tabControl1.SelectedTab.Controls[0] as HostControl).HostSurface;
            foreach (TabPage tp in this.tabControl1.TabPages)
            {
                if(hostSurface.Equals(tp.Tag as HostSurface))
                {
                    this.tabControl1.SelectedTab = tp;
                    return;
                }
            }
            string code = hostSurface.GetCode();
            string language = this.tabControl1.SelectedTab.Tag.ToString();
            if (String.IsNullOrEmpty(code))
                return;
            string tabName = this.tabControl1.SelectedTab.Text.Replace("Design", "Code");
            TabPage tabpage = new TabPage(tabName);
            CodeEditorControl cec = new CodeEditorControl();
            cec.Parent = tabpage;
            cec.Dock = DockStyle.Fill;
            cec.ShowText(code, language);
            tabpage.Tag = hostSurface;
            this.tabControl1.TabPages.Add(tabpage);
            this.tabControl1.SelectedTab = tabpage;
        }

        private class CommandAction
        {
            public Action<HostSurface> Action;
            public HostSurface HostSurface;
            public CommandAction(Action<HostSurface> action, HostSurface hostSurface)
            {
                Action = action;
                HostSurface = hostSurface;
            }
        }

    }
}
