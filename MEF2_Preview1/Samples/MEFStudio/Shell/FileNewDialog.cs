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
    public partial class FileNewDialog : Form, IPartImportsSatisfiedNotification
    {
        [ImportMany(AllowRecomposition=true)]
        private Lazy<HostSurfaceFactory, IDesignerMetadataView>[] designerFactories = null;
        private Dictionary<string, List<string>> items = new Dictionary<string, List<string>>();

        public FileNewDialog()
        {
            InitializeComponent();
        }


        private void OKButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        public Lazy<HostSurfaceFactory, IDesignerMetadataView> GetHostFactory()
        {
            IEnumerable<Lazy<HostSurfaceFactory, IDesignerMetadataView>> factories = designerFactories.Where(d => d.Metadata.Language == languageListView.SelectedItems[0].Text && itemListView.SelectedItems[0].Text.Contains(d.Metadata.ItemType));
            if (factories != null && factories.Count() > 0)
                return factories.First();
            return null;
        }

        private void languageListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (languageListView.SelectedItems == null || languageListView.SelectedItems.Count <= 0)
                return;
            itemListView.Items.Clear();
            foreach (string itemType in items[languageListView.SelectedItems[0].Text])
                itemListView.Items.Add(itemType);
            if(itemListView.Items!=null && itemListView.Items.Count>0)
                itemListView.Items[0].Selected = true;
        }

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            items.Clear();
            languageListView.Items.Clear();
            itemListView.Items.Clear();
            foreach (var designer in designerFactories)
            {
                if (items.ContainsKey(designer.Metadata.Language))
                    items[designer.Metadata.Language].Add(designer.Metadata.ItemType + "." + designer.Metadata.FileExtension);
                else
                    items.Add(designer.Metadata.Language, new List<string> { designer.Metadata.ItemType + "." + designer.Metadata.FileExtension });
            }
            foreach (string key in items.Keys)
                languageListView.Items.Add(key);
            languageListView.Items[0].Selected = true;
            foreach (string itemType in items[languageListView.Items[0].Text])
                itemListView.Items.Add(itemType);
            itemListView.Items[0].Selected = true;
        }

        #endregion
    }
}
