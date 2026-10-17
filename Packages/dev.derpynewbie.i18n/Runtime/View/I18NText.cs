using DerpyNewbie.Common;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
namespace DerpyNewbie.I18N.View
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class I18NText : I18NView
    {
        [SerializeField] [NewbieInject(SearchScope.Children)]
        private Text text;

        public override void OnI18NRefresh()
        {
            if (text == null)
            {
                Debug.LogError($"[I18nText-{name}] Text is null!", this);
                return;
            }

            if (provider == null)
            {
                Debug.LogError($"[I18nText-{name}] Provider is null!", this);
                return;
            }

            text.text = provider.GetString(key);
        }
    }
}
