using DerpyNewbie.Common;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
namespace DerpyNewbie.I18N.View
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class I18NText : I18NView
    {
        [SerializeField]
        private Text text;
        [SerializeField]
        protected string key;

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

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override void OnValidate()
        {
            base.OnValidate();
            if (text == null)
            {
                text = GetComponentInChildren<Text>();
            }
        }
#endif
    }
}
