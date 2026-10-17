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

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected virtual void OnValidate()
        {
            if (provider == null) provider = FindObjectOfType<I18NProvider>();
            if (provider == null) return;

            OnI18NRefresh();
        }
#endif
    }
}
