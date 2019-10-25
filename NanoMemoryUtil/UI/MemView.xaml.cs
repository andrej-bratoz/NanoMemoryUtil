using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WarInTheNorthTrainer.Code.MVVM;

namespace WarInTheNorthTrainer.UI
{
    /// <summary>
    /// Interaction logic for MemView.xaml
    /// </summary>
    public partial class MemView : UserControl, IView
    {
        public MemView()
        {
            InitializeComponent();
            Loaded += (sender, args) => OnLoaded?.Invoke();
            Unloaded += (sender, args) => OnUnloaded?.Invoke();
        }

        public event Action OnLoaded;
        public event Action OnUnloaded;
        public void ExecuteOnUIThread(Action a)
        {
            Dispatcher.Invoke(a);
        }

        private void ElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var view = _listView.View as GridView;
            foreach (var gridViewColumn in view.Columns)
            {
                gridViewColumn.Width = _listView.ActualWidth / view.Columns.Count;
            }
        }

        private void _listView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
