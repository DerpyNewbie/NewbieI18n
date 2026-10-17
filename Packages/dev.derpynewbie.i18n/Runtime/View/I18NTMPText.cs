using DerpyNewbie.Common;
using TMPro;
using UnityEngine;
namespace DerpyNewbie.I18N.View
{
    public class I18NTMPText : I18NView
    {
        [SerializeField] [NewbieInject(SearchScope.Children)]
        private TMP_Text text;

        public override void OnI18NRefresh()
        {
            if (text == null)
            {
                Debug.LogError("[I18nTMPText] Text is null!", this);
                return;
            }

            if (provider == null)
            {
                Debug.LogError("[I18nTMPText] Provider is null!", this);
                return;
            }

            text.text = provider.GetString(key);
        }
    }
}
