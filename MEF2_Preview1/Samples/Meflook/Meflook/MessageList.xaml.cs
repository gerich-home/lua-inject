//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Meflook;

namespace MeflookSample
{
    /// <summary>
    /// Interaction logic for MessageList.xaml
    /// </summary>
    [Export("MeflookView", typeof(UserControl)), ShellViewMetadata(2)]
    public partial class MessageList : System.Windows.Controls.UserControl
    {
        private int previousSelectedIndex = 0;
        private int currentSelectedIndex = 0;
        private string folder = "Inbox";

        [Import(typeof(IEmailService))]
        private IEmailService emailService = null;

        [Import(typeof(ISelectionService))]
        private ISelectionService selectionService = null;

        public MessageList()
        {
            InitializeComponent();
        }

        private void selectionService_CurrentFolderChanged()
        {
            previousSelectedIndex = 0;
            currentSelectedIndex = 0;
            folder = selectionService.CurrentFolder;
            this.DataContext = emailService.GetEmailList(folder);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            previousSelectedIndex = currentSelectedIndex;
            currentSelectedIndex = messageListView.SelectedIndex == -1 ? 0 : messageListView.SelectedIndex;
            selectionService.CurrentIndex = currentSelectedIndex;
            messageListView.SelectedIndex = currentSelectedIndex;
            if(messageListView.SelectedIndex>0)
                emailService.MarkAsRead(folder, previousSelectedIndex);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = emailService.GetEmailList(folder);
            selectionService.CurrentFolderChanged += new Meflook.CurrentFolderChangedHandler(selectionService_CurrentFolderChanged);
        }

        public UserControl View
        {
            get { return this; }
        }
    }
}
