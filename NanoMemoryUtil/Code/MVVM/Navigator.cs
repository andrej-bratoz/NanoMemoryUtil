using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WarInTheNorthTrainer.UI;

namespace WarInTheNorthTrainer.Code.MVVM
{
    public class Navigator
    {
        public static void NavigateToMainView()
        {
            var view = new MemView();
            var vm = new MemViewModel();
            var presenter = new MemPresenter(view, vm);
            view.DataContext = vm;
            Application.Current.MainWindow.Content = presenter.View;
        }
    }
}
