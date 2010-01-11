﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Resources;
using System.Xml.Linq;
using System.Reflection;
using Ranet.AgOlap.Controls.General.ItemControls;

namespace Ranet.AgOlap.Controls.Combo
{
    public partial class XapItemComboBox : UserControl
    {
        public XapItemComboBox()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            ItemsComboBox.Items.Clear();

            DownloadXap();
        }

        private void DownloadXap()
        {
            WebClient downloader = new WebClient();
            downloader.OpenReadCompleted += new OpenReadCompletedEventHandler(downloader_OpenReadCompleted);
            downloader.OpenReadAsync(Application.Current.Host.Source);
        }

        void downloader_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                string appManifest = new StreamReader(Application.GetResourceStream(new StreamResourceInfo(e.Result, null), new Uri("AppManifest.xaml", UriKind.Relative)).Stream).ReadToEnd();

                XElement deploymentRoot = XDocument.Parse(appManifest).Root;
                List<XElement> deploymentParts = (from assemblyParts in deploymentRoot.Elements().Elements()
                                                  select assemblyParts).ToList();

                foreach (XElement xElement in deploymentParts)
                {
                    string source = xElement.Attribute("Source").Value;
                    AssemblyPart asmPart = new AssemblyPart();
                    StreamResourceInfo streamInfo = Application.GetResourceStream(new StreamResourceInfo(e.Result, "application/binary"), new Uri(source, UriKind.Relative));
                    var asm = asmPart.Load(streamInfo.Stream);
                    if (asm != null && asm.ManifestModule != null)
                    {
                        ItemsComboBox.Items.Add(new ItemControlBase(false) { Text = asm.ManifestModule.ToString(), Tag = asm });
                    }
                }

                if (ItemsComboBox.Items.Count > 0)
                {
                    ItemsComboBox.SelectedIndex = 0;
                }
            }
            catch 
            {
            }
        }

        public ItemControlBase CurrentObject
        {
            get
            {
                var item = ItemsComboBox.SelectedItem as ItemControlBase;
                if (item != null)
                {
                    return item;
                }
                return null;
            }
        }
    }
}