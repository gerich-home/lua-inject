//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;
using Meflook;

namespace MeflookSample
{
    /// <summary>
    /// Interaction logic for MessagePane.xaml
    /// </summary>
    [Export("MeflookView", typeof(UserControl)), ShellViewMetadata(3)]
    public partial class MessagePane : System.Windows.Controls.UserControl
    {
        [Import(typeof(IEmailService))]
        private IEmailService emailService = null;

        [Import(typeof(ISelectionService))]
        private ISelectionService selectionService = null;

        public MessagePane()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //Display message
            DisplayMessage("Inbox", 0);
            selectionService.CurrentIndexChanged += new Meflook.CurrentIndexChangedHandler(selectionService_CurrentIndexChanged);
        }

        private void selectionService_CurrentIndexChanged()
        {
            DisplayMessage(selectionService.CurrentFolder, selectionService.CurrentIndex);
        }

        private void DisplayMessage(string folder, int index)
        {
            this.DataContext = emailService.GetEmailMetaData(folder, index);
            string xaml = emailService.GetEmail(folder, index);
            using (StringReader stringReader = new StringReader(xaml))
            {
                using (XmlTextReader xmlTextReader = new XmlTextReader(stringReader))
                {
                    FlowDocument flowDocument = (FlowDocument)XamlReader.Load(xmlTextReader);
                    this.messageContent.Document = flowDocument;
                }
            }
        }

        public UserControl View
        {
            get { return this; }
        }
    }
}
