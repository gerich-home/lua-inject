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
using System.Drawing.Design;
using System.ComponentModel.Design;
using ToolboxLibrary;
using System.Collections;
using Contracts;
using System.ComponentModel.Composition;
using System.Collections.ObjectModel;

namespace ToolboxLibrary
{
    [Export(typeof(IToolbox))]
    public partial class Toolbox : UserControl, IToolboxService, IToolbox, IPartImportsSatisfiedNotification 
    {
        [Import]
        private ToolboxUIManager toolboxUIManager = null;
        private Collection<ToolboxTab> toolboxTabCollection = null;
        private Button[] tabPageArray = null;
        private int selectedIndex = 0;
        private IDesignerHost designerHost = null;
        private ListBox toolsListBox = null;

        public Toolbox()
        {
            InitializeComponent();
        }

        public void InitializeToolbox()
        {
            toolboxUIManager.PopulateUI(this);
            AddEventHandlers();
            PrintToolbox();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Collection<ToolboxTab> Tabs
        {
            get
            {
                return toolboxTabCollection;
            }
            set
            {
                toolboxTabCollection = value;
            }
        }

        private void PrintToolbox()
        {
            try
            {
                for (int i = 0; i < Tabs.Count; i++)
                {
                    Console.WriteLine(Tabs[i].Name);
                    for (int j = 0; j < Tabs[i].ToolboxItems.Count; j++)
                        Console.WriteLine("\t" + Tabs[i].ToolboxItems[j].Type.ToString());
                    Console.WriteLine(" ");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        internal Button[] TabPageArray
        {
            get
            {
                return tabPageArray;
            }
            set
            {
                tabPageArray = value;
            }
        }
        internal ListBox ToolsListBox
        {
            get
            {
                return toolsListBox;
            }
            set
            {
                toolsListBox = value;
            }
        }

        private void AddEventHandlers()
        {
            ToolsListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.list_KeyDown);
            ToolsListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.list_MouseDown);
            ToolsListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.list_DrawItem);
        }

        private void list_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            try
            {
                ListBox lbSender = sender as ListBox;
                if (lbSender == null)
                    return;

                // If this tool is the currently selected tool, draw it with a highlight.
                if (selectedIndex == e.Index)
                {
                    e.Graphics.FillRectangle(Brushes.LightSlateGray, e.Bounds);
                }

                System.Drawing.Design.ToolboxItem tbi = lbSender.Items[e.Index] as System.Drawing.Design.ToolboxItem;
                Rectangle BitmapBounds = new Rectangle(e.Bounds.Location.X, e.Bounds.Location.Y + e.Bounds.Height / 2 - tbi.Bitmap.Height / 2, tbi.Bitmap.Width, tbi.Bitmap.Height);
                Rectangle StringBounds = new Rectangle(e.Bounds.Location.X + BitmapBounds.Width + 5, e.Bounds.Location.Y, e.Bounds.Width - BitmapBounds.Width, e.Bounds.Height);

                StringFormat format = new StringFormat();

                format.LineAlignment = StringAlignment.Center;
                format.Alignment = StringAlignment.Near;
                e.Graphics.DrawImage(tbi.Bitmap, BitmapBounds);
                e.Graphics.DrawString(tbi.DisplayName, new Font("Tahoma", 11, FontStyle.Regular, GraphicsUnit.World), Brushes.Black, StringBounds, format);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private void list_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                ListBox lbSender = sender as ListBox;
                Rectangle lastSelectedBounds = lbSender.GetItemRectangle(0);
                try
                {
                    lastSelectedBounds = lbSender.GetItemRectangle(selectedIndex);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

                selectedIndex = lbSender.IndexFromPoint(e.X, e.Y); // change our selection
                lbSender.SelectedIndex = selectedIndex;
                lbSender.Invalidate(lastSelectedBounds); // clear highlight from last selection
                lbSender.Invalidate(lbSender.GetItemRectangle(selectedIndex)); // highlight new one

                if (selectedIndex != 0)
                {
                    if (e.Clicks == 2)
                    {
                        IDesignerHost idh = (IDesignerHost)this.DesignerHost.GetService(typeof(IDesignerHost));
                        IToolboxUser tbu = idh.GetDesigner(idh.RootComponent as IComponent) as IToolboxUser;

                        if (tbu != null)
                        {
                            tbu.ToolPicked((System.Drawing.Design.ToolboxItem)(lbSender.Items[selectedIndex]));
                        }
                    }
                    else if (e.Clicks < 2)
                    {
                        System.Drawing.Design.ToolboxItem tbi = lbSender.Items[selectedIndex] as System.Drawing.Design.ToolboxItem;
                        IToolboxService tbs = this;

                        // The IToolboxService serializes ToolboxItems by packaging them in DataObjects.
                        DataObject d = tbs.SerializeToolboxItem(tbi) as DataObject;

                        try
                        {
                            lbSender.DoDragDrop(d, DragDropEffects.Copy);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private void list_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            try
            {
                ListBox lbSender = sender as ListBox;
                Rectangle lastSelectedBounds = lbSender.GetItemRectangle(0);
                try
                {
                    lastSelectedBounds = lbSender.GetItemRectangle(selectedIndex);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }

                switch (e.KeyCode)
                {
                    case Keys.Up: if (selectedIndex > 0)
                        {
                            selectedIndex--; // change selection
                            lbSender.SelectedIndex = selectedIndex;
                            lbSender.Invalidate(lastSelectedBounds); // clear old highlight
                            lbSender.Invalidate(lbSender.GetItemRectangle(selectedIndex)); // add new one
                        }
                        break;

                    case Keys.Down: if (selectedIndex + 1 < lbSender.Items.Count)
                        {
                            selectedIndex++; // change selection
                            lbSender.SelectedIndex = selectedIndex;
                            lbSender.Invalidate(lastSelectedBounds); // clear old highlight
                            lbSender.Invalidate(lbSender.GetItemRectangle(selectedIndex)); // add new one
                        }
                        break;

                    case Keys.Enter:
                        if (DesignerHost == null)
                            MessageBox.Show("idh Null");

                        IToolboxUser tbu = DesignerHost.GetDesigner(DesignerHost.RootComponent as IComponent) as IToolboxUser;

                        if (tbu != null)
                        {
                            // Enter means place the tool with default location and default size.
                            tbu.ToolPicked((System.Drawing.Design.ToolboxItem)(lbSender.Items[selectedIndex]));
                            lbSender.Invalidate(lastSelectedBounds); // clear old highlight
                            lbSender.Invalidate(lbSender.GetItemRectangle(selectedIndex)); // add new one
                        }

                        break;

                    default:
                        {
                            Console.WriteLine("Error: Not able to add");
                            break;
                        }
                } // switch
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region IToolboxService Members

        // We only implement what is really essential for ToolboxService

        public System.Drawing.Design.ToolboxItem GetSelectedToolboxItem(IDesignerHost host)
        {
            ListBox list = this.ToolsListBox;
            System.Drawing.Design.ToolboxItem tbi = (System.Drawing.Design.ToolboxItem)list.Items[selectedIndex];
            if (tbi.DisplayName != "<Pointer>")
                return tbi;
            else
                return null;
        }

        public System.Drawing.Design.ToolboxItem GetSelectedToolboxItem()
        {
            return this.GetSelectedToolboxItem(null);
        }

        public void AddToolboxItem(System.Drawing.Design.ToolboxItem toolboxItem, string category)
        {
        }

        public void AddToolboxItem(System.Drawing.Design.ToolboxItem toolboxItem)
        {
        }

        public bool IsToolboxItem(object serializedObject, IDesignerHost host)
        {
            return false;
        }

        public bool IsToolboxItem(object serializedObject)
        {
            return false;
        }

        public void SetSelectedToolboxItem(System.Drawing.Design.ToolboxItem toolboxItem)
        {
        }

        public void SelectedToolboxItemUsed()
        {
            ListBox list = this.ToolsListBox;

            list.Invalidate(list.GetItemRectangle(selectedIndex));
            selectedIndex = 0;
            list.SelectedIndex = 0;
            list.Invalidate(list.GetItemRectangle(selectedIndex));
        }

        public CategoryNameCollection CategoryNames
        {
            get
            {
                return null;
            }
        }

        void IToolboxService.Refresh()
        {
        }

        public void AddLinkedToolboxItem(System.Drawing.Design.ToolboxItem toolboxItem, string category, IDesignerHost host)
        {
        }

        public void AddLinkedToolboxItem(System.Drawing.Design.ToolboxItem toolboxItem, IDesignerHost host)
        {
        }

        public bool IsSupported(object serializedObject, ICollection filterAttributes)
        {
            return false;
        }

        public bool IsSupported(object serializedObject, IDesignerHost host)
        {
            return false;
        }

        public string SelectedCategory
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public System.Drawing.Design.ToolboxItem DeserializeToolboxItem(object serializedObject, IDesignerHost host)
        {
            return (System.Drawing.Design.ToolboxItem)((DataObject)serializedObject).GetData(typeof(System.Drawing.Design.ToolboxItem));
        }

        public System.Drawing.Design.ToolboxItem DeserializeToolboxItem(object serializedObject)
        {
            return this.DeserializeToolboxItem(serializedObject, this.DesignerHost);
        }

        public System.Drawing.Design.ToolboxItemCollection GetToolboxItems(string category, IDesignerHost host)
        {
            return null;
        }

        public System.Drawing.Design.ToolboxItemCollection GetToolboxItems(string category)
        {
            return null;
        }

        public System.Drawing.Design.ToolboxItemCollection GetToolboxItems(IDesignerHost host)
        {
            return null;
        }

        public System.Drawing.Design.ToolboxItemCollection GetToolboxItems()
        {
            return null;
        }

        public void AddCreator(ToolboxItemCreatorCallback creator, string format, IDesignerHost host)
        {
        }

        public void AddCreator(ToolboxItemCreatorCallback creator, string format)
        {
        }

        public bool SetCursor()
        {
            return false;
        }

        public void RemoveToolboxItem(System.Drawing.Design.ToolboxItem toolboxItem, string category)
        {
        }

        public void RemoveToolboxItem(System.Drawing.Design.ToolboxItem toolboxItem)
        {
        }

        public object SerializeToolboxItem(System.Drawing.Design.ToolboxItem toolboxItem)
        {
            return new DataObject(toolboxItem);
        }

        public void RemoveCreator(string format, IDesignerHost host)
        {
        }

        public void RemoveCreator(string format)
        {
        }

        #endregion


        #region IToolbox Members


        public IToolboxService ToolboxService
        {
            get { return this; }
        }

        public IDesignerHost DesignerHost
        {
            set
            {
                designerHost = value;
            }
            get
            {
                return designerHost;
            }
        }

        public void RefreshControls()
        {
            toolboxUIManager.PopulateData();
            toolboxUIManager.PopulateUI(this);
            AddEventHandlers();
        }

        public UserControl View
        {
            get
            {
                return this;
            }
        }


        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            InitializeToolbox();
        }

        #endregion
    }
}
