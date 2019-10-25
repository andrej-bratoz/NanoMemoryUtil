using System.Windows;
using NanoMemUtil.UI;

namespace NanoMemUtil.Code.MVVM
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
