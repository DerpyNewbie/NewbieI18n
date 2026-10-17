using TMPro;
using UnityEngine;
namespace DerpyNewbie.I18N.View
{
    public class I18NTMPText : I18NView
    {
        [SerializeField]
        private TMP_Text text;
        [SerializeField]
        protected string key;

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

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override void OnValidate()
        {
            base.OnValidate();
            if (text == null)
            {
                text = GetComponentInChildren<TMP_Text>();
            }
        }
#endif
    }
}
