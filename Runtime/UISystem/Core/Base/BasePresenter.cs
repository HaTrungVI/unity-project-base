namespace ProjectBase.UI.Core
{
    public abstract class BasePresenter<TView> : IPresenter where TView : IView
    {
        protected readonly TView View;

        protected BasePresenter(TView view)
        {
            View = view;
        }

        public virtual void Initialize(object parameter = null) { }

        public virtual void Dispose() { }
    }
}
