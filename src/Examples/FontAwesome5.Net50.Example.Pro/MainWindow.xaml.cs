using FontAwesome5.Net40.Example.ViewModels;
using System.Windows;

namespace FontAwesome5.Net50.Example.Pro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();
            InitializeComponent();
        }
    }
}
