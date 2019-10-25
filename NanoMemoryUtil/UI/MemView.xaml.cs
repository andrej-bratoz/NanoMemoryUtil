using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NanoMemUtil.Code.MVVM;

namespace NanoMemUtil.UI
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
