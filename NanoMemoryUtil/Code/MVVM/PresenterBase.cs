using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarInTheNorthTrainer.Code.MVVM
{
    public abstract class PresenterBase<TView, TViewModel>
        where TView : IView
        where TViewModel : INotifyPropertyChanged
    {
        public TView View { get; set; }
        public TViewModel ViewModel { get; set; }

        protected PresenterBase(TView view, TViewModel vm)
        {
            View = view;
            ViewModel = vm;

            view.OnLoaded += OnViewReady;
            view.OnUnloaded += OnViewFinished;
        }

        public abstract void OnViewReady();
        public abstract void OnViewFinished();
    }
}
