//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;

namespace Microsoft.Samples.XFileExplorer
{
    [Export(typeof(INavigationService))]
    public class NavigationService : INavigationService
    {
        public event CurrentPathChangedHandler CurrentPathChanged;
        public event SelectedItemChangedHandler SelectedItemChanged;

        private string _currentPath = "";
        private string _selectedItem = "";

        /// <summary>
        /// Current opened folder
        /// </summary>
        public string CurrentPath
        {
            get
            {
                return _currentPath;
            }
            set
            {
                if (value.ToLower() != _currentPath.ToLower())
                {
                    if (Directory.Exists(value))
                    {
                        _currentPath = value;
                        if (CurrentPathChanged != null)
                            CurrentPathChanged();
                        SelectedItem = "";
                    }
                    else
                    {
                        MessageBox.Show("The specified directory does not exist.");
                    }
                }
            }
        }

        /// <summary>
        /// Current selected item, which could be either a folder or a file
        /// </summary>
        public string SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (value.ToLower() != _selectedItem.ToLower())
                {
                    _selectedItem = value;
                    if (SelectedItemChanged != null)
                        SelectedItemChanged();
                }
            }
        }

        /// <summary>
        /// Determine whether the current selected item is a folder or a file
        /// </summary>
        /// <returns>bool</returns>
        public bool IsFolder()
        {
            string item = Path.Combine(_currentPath, _selectedItem);
            return Directory.Exists(item);
        }
    }
}
