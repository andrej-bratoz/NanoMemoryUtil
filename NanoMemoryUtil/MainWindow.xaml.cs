using System.Windows;
using NanoMemUtil.Code.MVVM;

namespace NanoMemUtil
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Navigator.NavigateToMainView();
        }
    }
}
