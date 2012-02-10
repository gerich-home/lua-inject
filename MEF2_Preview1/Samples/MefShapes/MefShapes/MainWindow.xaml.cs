//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Microsoft.Samples.MefShapes.Shapes;

namespace Microsoft.Samples.MefShapes
{
    [Export(typeof(MainWindow))]
    public partial class MainWindow : Window
    {
        private static readonly DependencyProperty MefShapesGameProperty = DependencyProperty.Register("MefShapesGame", typeof(IMefShapesGame), typeof(MainWindow));

        [Import]
        public IMefShapesGame MefShapesGame
        {
            get { return (IMefShapesGame)GetValue(MefShapesGameProperty); }
            set { SetValue(MefShapesGameProperty, value); }
        }

        [Import]
        private AggregateCatalog Catalog { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += delegate { MefShapesGame.StartGame(this.Dispatcher, 300); };
        }

        private void mainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (MefShapesGame.IsRunning)
            {
                switch (e.Key)
                {
                    case Key.Right:
                        MefShapesGame.ActiveShape.Move(Direction.Right);
                        break;
                    case Key.Left:
                        MefShapesGame.ActiveShape.Move(Direction.Left);
                        break;
                    case Key.Down:
                        MefShapesGame.ActiveShape.Move(Direction.Down);
                        break;
                    case Key.Up:
                        MefShapesGame.ActiveShape.Move(Direction.Up);
                        break;
                    case Key.Space:
                        MefShapesGame.ActiveShape.Rotate(Direction.Right);
                        break;
                    case Key.Return:
                        MefShapesGame.StopGame();
                        break;
                    default:
                        break;

                }
            }
            else
            {
                if (e.Key == Key.Return || e.Key == Key.Space)
                    MefShapesGame.StartGame(this.Dispatcher, 300);
            }
        }

        private void CommandBindingClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CommandBindingOpenAssembly_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            dialog.Filter = "Assemblies (*.exe;*.dll)|*.exe;*.dll";
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Catalog.Catalogs.Add(new AssemblyCatalog(dialog.FileName));
            }
        }

        private void CommandBindingHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Use <ENTER> to Pause/Resume, arrow keys to move and <SPACE> to rotate shapes." +
                " Please post your questions or comments on http://forums.msdn.microsoft.com/en-US/MEFramework/threads/",
                "MefShapes", MessageBoxButton.OK);
        }
    }
}
