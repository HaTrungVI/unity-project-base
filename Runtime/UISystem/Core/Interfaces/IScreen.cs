namespace ProjectBase.UI.Core
{
    public interface IScreen : IView
    {
        string ScreenId { get; }
        void OnNavigatedTo(object parameter = null);
        void OnNavigatedFrom();
    }
}