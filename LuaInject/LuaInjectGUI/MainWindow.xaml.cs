using System.Windows;
using LuaInject;

namespace LuaInjectGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var a = new Sheduler();

            a.Start("LuaInjectGUI.exe", target.Text, module.Text);
        }
    }
}
