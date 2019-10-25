using System.ComponentModel;

namespace NanoMemUtil.Code.MVVM
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
