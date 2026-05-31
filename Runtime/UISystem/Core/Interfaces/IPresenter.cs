namespace ProjectBase.UI.Core
{
    public interface IPresenter : System.IDisposable
    {
        void Initialize(object parameter = null);
    }
}
