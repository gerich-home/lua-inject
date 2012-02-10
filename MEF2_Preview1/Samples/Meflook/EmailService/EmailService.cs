//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using HTMLConverter;
using Meflook;

namespace MeflookSample
{
    [Export(typeof(IEmailService))]
    public class EmailService : IEmailService
    {
        private XmlDataProvider xdp;
        private DataSet baseMessages;
        private Dictionary<string, DataView> emailDictionary;

        public EmailService()
        {
            InitializeDummyMessages();
            InitializeDummyFolders();
            emailDictionary = new Dictionary<string, DataView>();
        }

        private void InitializeDummyMessages()
        {
            if (baseMessages != null)
                return;
            baseMessages = new DataSet();
            baseMessages.ReadXml(Application.GetResourceStream(new Uri(@"pack://application:,,,/EmailService;component/Data/Inbox.xml", UriKind.Absolute)).Stream);
        }

        private void InitializeDummyFolders()
        {
            if (xdp != null)
                return;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Application.GetResourceStream(new Uri(@"pack://application:,,,/EmailService;component/Data/FolderList.xml", UriKind.Absolute)).Stream);
            xdp = new XmlDataProvider();
            xdp.Document = xmlDoc;
            xdp.XPath = "FolderList";
        }

        public XmlDataProvider GetFoldersDataProvider()
        {
            return xdp;
        }

        public DataView GetEmailList(string folder)
        {
            // Check if the dictionary already contains the messages
            if (emailDictionary.ContainsKey(folder))
                return emailDictionary[folder];

            // Email list does not exist. Create the list.
            DataSet ds = CreateEmailList(folder);
            DataView dv = ds.Tables[0].DefaultView;
            dv.Sort = "SentDate DESC";
            emailDictionary.Add(folder, dv);
            return dv;
        }

        public void MarkAsRead(string folder, int index)
        {
            if (!emailDictionary.ContainsKey(folder))
                return;

            if (!Boolean.Parse(emailDictionary[folder][index]["Read"].ToString()))
            {
                emailDictionary[folder][index]["Read"] = true;
                DecrementUnreadEmailCount(folder);
            }
        }

        private DataSet CreateEmailList(string folder)
        {
            Random random = new Random();
            int startIndex;
            int endIndex;

            if (folder == "Inbox" || folder == "Acropolis")
                startIndex = 0;
            else
                startIndex = random.Next(1, 10);

            endIndex = random.Next(21, 40);

            DataSet ds = baseMessages.Clone();
            for (int i = startIndex; i < endIndex; i++)
            {
                DataRow row = ds.Tables[0].NewRow();
                for (int j = 0; j < ds.Tables[0].Columns.Count; j++)
                {
                    row[j] = baseMessages.Tables[0].Rows[i][j];
                }
                ds.Tables[0].Rows.Add(row);
            }

            FixEmailMetaData(ds, GetUnreadEmailCount(folder));
            return ds;
        }

        private void FixEmailMetaData(DataSet ds, int unreadCount)
        {
            int emailCount = ds.Tables[0].Rows.Count;
            DateTime baseTime = DateTime.Now;
            for (int i = 0; i < emailCount; i++)
            {
                ds.Tables[0].Rows[i]["SentDate"] = baseTime.Subtract(new TimeSpan(3,18,46));
                if (i < unreadCount)
                    ds.Tables[0].Rows[i]["Read"] = false;
                else
                    ds.Tables[0].Rows[i]["Read"] = true;
            }
        }

        private int GetUnreadEmailCount(string folder)
        {
            XmlNodeList nodeList = xdp.Document.GetElementsByTagName("Name");
            for (int i = 0; i < nodeList.Count; i++)
                if (nodeList[i].InnerText == folder)
                    return Int32.Parse(nodeList[i].NextSibling.InnerText.ToString());
            return 0;
        }

        private int DecrementUnreadEmailCount(string folder)
        {
            XmlNodeList nodeList = xdp.Document.GetElementsByTagName("Name");
            for (int i = 0; i < nodeList.Count; i++)
            {
                if (nodeList[i].InnerText == folder)
                {
                    int currentValue = Int32.Parse(nodeList[i].NextSibling.InnerText.ToString());
                    currentValue = (currentValue == 0) ? 0 : currentValue - 1;
                    nodeList[i].NextSibling.InnerText = currentValue.ToString();
                    return currentValue;
                }
            }
            return 0;
        }

        public DataRowView GetEmailMetaData(string folder, int index)
        {
            return GetEmailList(folder)[index];
        }

        public string GetEmail(string folder, int index)
        {
            string file = GetEmailMetaData(folder, index)["Path"].ToString();
            Stream stream = Application.GetResourceStream(new Uri(String.Concat(@"pack://application:,,,/EmailService;component/Data/", file), UriKind.Absolute)).Stream;
            StreamReader reader = new StreamReader(stream);
            string emailHtml = reader.ReadToEnd();
            reader.Close();
            return HtmlToXamlConverter.ConvertHtmlToXaml(emailHtml, true, HTMLConverter.EmptyParagraphTreatment.RemoveAll);
        }
    }
}
