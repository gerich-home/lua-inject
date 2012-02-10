//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using Microsoft.Samples.XFileExplorer;

namespace Microsoft.Samples.XFileExplorerFavorites
{
    [Export("Microsoft.Samples.XFileExplorer.CustomMenuItemContract", typeof(MenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AddToFavsMenuItem : MenuItem
    {
        [Import]
        public INavigationService NavigationService { get; set; }

        [Import]
        public CompositionContainer Container { get; set; }

        public AddToFavsMenuItem()
        {
            Header = "_Add To Favorites";
            Click += delegate { AddToFavorites(); };
        }

        private void AddToFavorites()
        {
            Debug.Assert(NavigationService != null);
            Debug.Assert(Container != null);

            string fullPath = System.IO.Path.Combine(NavigationService.CurrentPath, NavigationService.SelectedItem);

            IFavoriteItem favItem = null;
            if (Directory.Exists(fullPath))
            {
                favItem = new FavoriteDirectoryItem(fullPath);
            }
            else if (File.Exists(fullPath))
            {
                favItem = new FavoriteFileItem(fullPath);
            }
            if (favItem != null)
            {
                CompositionBatch batch = new CompositionBatch();
                batch.AddPart(favItem);
                Container.Compose(batch);
            }
        }
    }
}
