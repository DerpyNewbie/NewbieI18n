using UdonSharp;
using VRC.SDK3.Data;
namespace DerpyNewbie.I18N
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class I18NProvider : UdonSharpBehaviour
    {
        private readonly DataList _i18NViews = new DataList();

        public abstract string GetString(string key, string language);
        public abstract string GetCurrentLanguage();
        public abstract string[] GetAvailableLanguages();
        public abstract bool SetCurrentLanguage(string language);

        public void RefreshViews()
        {
            foreach (var token in _i18NViews.ToArray())
            {
                if (token.TokenType != TokenType.Reference || token.Reference == null)
                {
                    continue;
                }

                ((I18NView)token.Reference).OnI18NRefresh();
            }
        }

        public void RegisterI18NView(I18NView view)
        {
            _i18NViews.Add(view);
            view.OnI18NRefresh();
        }

        public void UnregisterI18NView(I18NView view)
        {
            _i18NViews.Remove(view);
        }
    }

    public static class I18NProviderExtensions
    {
        public static string GetString(this I18NProvider provider, string key)
        {
            return provider.GetString(key, provider.GetCurrentLanguage());
        }
    }
}
