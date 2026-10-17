using DerpyNewbie.Common;
using UdonSharp;
using UnityEngine;
namespace DerpyNewbie.I18N
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class I18NView : UdonSharpBehaviour
    {
        [SerializeField] [NewbieInject]
        protected I18NProvider provider;
        [SerializeField]
        protected string key;
        [SerializeField]
        protected string stringPreview;

        private void OnEnable()
        {
            provider.RegisterI18NView(this);
        }

        private void OnDisable()
        {
            provider.UnregisterI18NView(this);
        }

        public virtual void OnI18NRefresh()
        {

        }

#if UNITY_EDITOR && !UDONSHARP_COMPILER
        protected virtual void OnValidate()
        {
            if (provider == null)
            {
                stringPreview = "<PROVIDER_NOT_FOUND>";
                return;
            }

            var str = provider.GetString(key);
            stringPreview = str ?? "<INVALID_KEY>";

            OnI18NRefresh();
        }
#endif
    }
}
