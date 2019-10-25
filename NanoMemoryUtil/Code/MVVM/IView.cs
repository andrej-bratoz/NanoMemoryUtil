using System;

namespace NanoMemUtil.Code.MVVM
{
    public interface IView
    {
        event Action OnLoaded;
        event Action OnUnloaded;
        void ExecuteOnUIThread(Action a);
    }
}