//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Microsoft.Samples.XFileExplorer;

namespace Microsoft.Samples.XFileExplorerComponents
{
    /// <summary>
    /// Interaction logic for SizeView.xaml
    /// </summary>
    [Export("Microsoft.Samples.XFileExplorer.FileExplorerViewContract", typeof(UserControl))]
    [ExportMetadata("Name", "Size Pane")]
    [ExportMetadata("Docking", Dock.Right)]
    [ExportMetadata("DockId", 1)]
    [ExportMetadata("Hidden", false)]
    public partial class SizeView : UserControl
    {
        [Import]
        public INavigationService Navigation = null;

        int TOP_SIZE_ITEMS_COUNT = 10;
        double PIE_RADIUS = 200;

        // For scaling the pie view to fit into content
        double originanlSize = 1.0;
        double absScaleFactor = 1.0;

        // For obtaining item sizes with multiple threads
        int _jobsCount = 0;
        List<ItemSize> _jobsItemList = null;
        bool _jobCancelling = false;

        private static Brush[] PieColors = new Brush[] {
            Brushes.OrangeRed, Brushes.Orange, Brushes.SlateBlue, Brushes.MediumVioletRed, Brushes.DarkOliveGreen, Brushes.LightSalmon, 
			Brushes.MediumAquamarine, Brushes.PaleVioletRed, Brushes.DarkCyan, Brushes.Goldenrod, Brushes.Green  };

        public SizeView()
        {
            InitializeComponent();
            BtnSize.Tag = "Ready";
        }

        private void BtnSize_Click(object sender, RoutedEventArgs e)
        {
            // Clear any previous pie graph and its legend
            PieGraph.Children.Clear();
#pragma warning disable 0618
            PieGraph.BitmapEffect = BitmapEffect; //This need to be removed. Otherwise, PieGraph stays on UI.
#pragma warning restore 0618
            LegendList.Items.Clear();
            LegendList.Visibility = Visibility.Hidden;

            if ((string)BtnSize.Tag == "Working")
            {
                _jobCancelling = true;
                return;
            }

            if (!Directory.Exists(Navigation.CurrentPath))
            {
                SizeMessage.Text = "No folder selectd";
                return;
            }

            try
            {
                SizeMessage.Text = "Working...";
                BtnSize.Tag = "Working";
                _jobCancelling = false;

                DirectoryInfo di = new DirectoryInfo(Navigation.CurrentPath);
                FileSystemInfo[] fsis = di.GetFileSystemInfos();

                _jobsCount = fsis.Count();
                _jobsItemList = new List<ItemSize>();

                if (_jobsCount == 0)
                {
                    SizeMessage.Text = "This folder is empty.";
                    BtnSize.Tag = "Ready";
                    return;
                }

                foreach (var item in fsis)
                {
                    BackgroundWorker bgw = new BackgroundWorker();
                    bgw.DoWork += new DoWorkEventHandler(BGW_DoWork);
                    bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_RunWorkerCompleted);
                    bgw.RunWorkerAsync(item);
                }
            }
            catch (Exception ex) //Ignore folder/file access exceptions
            {
                SizeMessage.Text = ex.Message;
                BtnSize.Tag = "Ready";
            } 
        }

        private void BGW_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            FileSystemInfo fsi = e.Argument as FileSystemInfo;

            ItemSize item = new ItemSize();
            item.Name = fsi.Name;
            item.IsFolder = Directory.Exists(fsi.FullName);
            item.Size = GetItemSize(fsi.FullName);
            _jobsItemList.Add(item);
        }

        private void BGW_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (--_jobsCount == 0) //The last job
            {
                if (!_jobCancelling)
                {
                    List<ItemSize> sortedItemList = new List<ItemSize>();

                    double totalSize = 0;
                    ItemSize otherItems = new ItemSize() { Name = "Others", Size = 0, IsFolder = false };
                    foreach (var item in _jobsItemList.OrderBy(i => i.Size).Reverse())
                    {
                        if (sortedItemList.Count < TOP_SIZE_ITEMS_COUNT)
                            sortedItemList.Add(item);
                        else
                            otherItems.Size += item.Size;

                        totalSize += item.Size;
                    }

                    if (_jobsItemList.Count > TOP_SIZE_ITEMS_COUNT)
                        sortedItemList.Add(otherItems);

                    if (totalSize != 0)
                    {
                        foreach (var item in sortedItemList)
                        {
                            item.Percentage = item.Size / totalSize;
                        }
                    }

                    ShowSortedItemsInGraph(sortedItemList);
                    ShowSortedItemsInText(sortedItemList);
                }
                else
                {
                    SizeMessage.Text = "The task was cancelled.";
                }

                BtnSize.Tag = "Ready";
            }
        }

        private void ShowSortedItemsInText(List<ItemSize> SortedItemList)
        {
            SizeMessage.Text = "";

            for (int i = 0; i < SortedItemList.Count; i++)
            {
                var item = SortedItemList[i];

                ListBoxItem legend = new ListBoxItem();

                //A colored rectangle used as an icon of the legend
                Rectangle legendIcon = new Rectangle();
                legendIcon.Width = legendIcon.Height = 10;
                legendIcon.Fill = PieColors[i % PieColors.Length];
                legendIcon.Margin = new Thickness(0, 0, 2, 0);
                //Legend text
                TextBlock legendMsg = new TextBlock();
                legendMsg.Text = (item.IsFolder) ? "[" + item.Name + "]" : item.Name;
                //Assembly tooltip icon and text into a panel
                StackPanel legendPanel = new StackPanel();
                legendPanel.Orientation = Orientation.Horizontal;
                legendPanel.Children.Add(legendIcon);
                legendPanel.Children.Add(legendMsg);
                legend.Content = legendPanel;

                LegendList.Items.Add(legend);
            }
            
            LegendList.Visibility = Visibility.Visible;
        }

        private void ShowSortedItemsInGraph(List<ItemSize> SortedItemList)
        {
            bool hasBoundary = true;
            Brush boundaryBrush = Brushes.Black;
            double boundaryThickness = 0.5;
            Point pieCenter = new Point(PIE_RADIUS, PIE_RADIUS);
            Point startPoint = new Point(PIE_RADIUS, 0);
            
            //Set bitmap effect for the pie graph
#pragma warning disable 0618
            PieGraph.BitmapEffect = new BevelBitmapEffect();
#pragma warning restore 0618

            //Move the center of the pie view to the origin of the canvas (canvas origin is located at the center of the view)
            ScaleTransform scale = new ScaleTransform(absScaleFactor, absScaleFactor);
            TransformGroup trasfm = new TransformGroup();
            trasfm.Children.Add(new TranslateTransform(-PIE_RADIUS, -PIE_RADIUS));
            trasfm.Children.Add(scale);
            PieGraph.RenderTransform = trasfm;

            for (int i = 0; i < SortedItemList.Count; i++)
            {
                var item = SortedItemList[i];
                double angle = 360 * item.Percentage;
                if (angle >= 360) angle = 360 - 1e-5;
                RotateTransform rotate = new RotateTransform(angle, pieCenter.X, pieCenter.Y);
                Point endPoint = rotate.Transform(startPoint);

                //Draw a fan shape
                PathFigure fig = new PathFigure();
                fig.StartPoint = pieCenter;
                fig.IsClosed = hasBoundary;
                fig.Segments.Add(new LineSegment(startPoint, hasBoundary));
                fig.Segments.Add(new ArcSegment(endPoint, new Size(PIE_RADIUS, PIE_RADIUS), 0, angle > 180, SweepDirection.Clockwise, hasBoundary));
                fig.Segments[0].IsSmoothJoin = fig.Segments[1].IsSmoothJoin = true;
                PathGeometry geo = new PathGeometry();
                geo.Figures.Add(fig);

                //Set color and boundary line for current fan shape
                System.Windows.Shapes.Path fan = new System.Windows.Shapes.Path();
                fan.Fill = PieColors[i % PieColors.Length];
                fan.Stroke = boundaryBrush;
                fan.StrokeThickness = boundaryThickness;
                fan.Data = geo;

                //Add tooptip for current fan shape
                ToolTip tip = new ToolTip();
                //A colored rectangle used as an icon of the tooltip
                Rectangle tipIcon = new Rectangle();
                tipIcon.Width = tipIcon.Height = 40;
                tipIcon.Fill = fan.Fill;
                tipIcon.Margin = new Thickness(0, 0, 4, 0);
                //Tooltip message
                TextBox tipMsg = new TextBox();
                string name = (item.IsFolder) ? "[" + item.Name + "]" : item.Name;
                tipMsg.Text = String.Format("{0}\n{1} Bytes\n{2}%", name, item.Size.ToString("n0"), (item.Percentage * 100).ToString("n2"));
                //Assembly tooltip icon and message into a panel and set it to the fan shape tooltip
                StackPanel tipPanel = new StackPanel();
                tipPanel.Orientation = Orientation.Horizontal;
                tipPanel.Children.Add(tipIcon);
                tipPanel.Children.Add(tipMsg);
                tip.Content = tipPanel;
                fan.ToolTip = tip;

                //Add a trigger to current fan so that it is highlighted when mouse is over it
                Trigger trigger = new Trigger();
                trigger.Property = System.Windows.Shapes.Path.IsMouseOverProperty;
                trigger.Value = true;
                trigger.Setters.Add(new Setter(System.Windows.Shapes.Path.OpacityProperty, 0.6));
                Style style = new Style();
                style.Triggers.Add(trigger);
                fan.Style = style;

                //Add current fan to the graph
                PieGraph.Children.Add(fan);
                startPoint = endPoint;
            }
        }

        private long GetItemSize(string ItemPath)
        {
            if (_jobCancelling) return 0;

            if (File.Exists(ItemPath))
            {
                FileInfo fi = new FileInfo(ItemPath);
                return fi.Length;
            }

            long size = 0;
            try
            {
                DirectoryInfo di = new DirectoryInfo(ItemPath);
                FileSystemInfo[] fsi = di.GetFileSystemInfos();
                foreach (var item in fsi)
                {
                    size += GetItemSize(item.FullName);
                }
            }
            catch (Exception) { } //Ignore folder/file access exceptions

            return size;
        }

        private void PieGraph_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newSize = e.NewSize.Width > e.NewSize.Height ? e.NewSize.Height : e.NewSize.Width;
            double oldSize = e.PreviousSize.Width > e.PreviousSize.Height ? e.PreviousSize.Height : e.PreviousSize.Width;

            if (oldSize == 0)
                originanlSize = newSize;
            else
                absScaleFactor = newSize / originanlSize;
            
            if (PieGraph.Children.Count > 0)
            {
                double scaleFactor = newSize / oldSize;
                ScaleTransform scale = new ScaleTransform(scaleFactor, scaleFactor);
                TransformGroup trasfm = new TransformGroup();
                trasfm.Children.Add(PieGraph.RenderTransform);
                trasfm.Children.Add(scale);
                PieGraph.RenderTransform = trasfm;
            }
        }
    }

    internal class ItemSize
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public bool IsFolder { get; set; }
        public double Percentage { get; set; }
    }
}
