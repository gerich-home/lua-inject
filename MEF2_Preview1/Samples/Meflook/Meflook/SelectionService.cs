using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace Meflook
{
    [Export(typeof(ISelectionService))]
    public class SelectionService : ISelectionService
    {
        public event CurrentFolderChangedHandler CurrentFolderChanged;
        public event CurrentIndexChangedHandler CurrentIndexChanged;

        private string currentFolder = "Inbox";
        private int currentIndex = 0;

        public string CurrentFolder
        {
            get
            {
                return currentFolder;
            }
            set
            {
                if (value != currentFolder)
                {
                    currentFolder = value;
                    if (CurrentFolderChanged != null)
                        CurrentFolderChanged();
                }
            }
        }

        public int CurrentIndex
        {
            get
            {
                return currentIndex;
            }
            set
            {
                if (value != currentIndex)
                {
                    currentIndex = value;
                    if (CurrentIndexChanged != null)
                        CurrentIndexChanged();
                }
            }
        }
    }
}
