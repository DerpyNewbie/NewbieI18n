using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
namespace DerpyNewbie.I18N.View
{
    /// <summary>
    /// Swaps the texture of RawImage with the one provided by <see cref="I18NProvider.GetTexture(string, string)"/>.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class I18NRawImage : I18NView
    {
        [SerializeField]
        private RawImage rawImage;
        [SerializeField]
        protected string key;

        public override void OnI18NRefresh()
        {
            if (rawImage == null)
            {
                Debug.LogError($"[I18NRawImage-{name}] RawImage is null!", this);
                return;
            }

            if (provider == null)
            {
                Debug.LogError($"[I18NRawImage-{name}] Provider is null!", this);
                return;
            }

            var texture = provider.GetTexture(key);
            if (texture == null)
            {
                Debug.LogWarning($"[I18NRawImage-{name}] No texture found for key {key}", this);
                return;
            }

            rawImage.texture = texture;
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override void OnValidate()
        {
            if (rawImage == null) rawImage = GetComponent<RawImage>();
            base.OnValidate();
        }
#endif
    }
}
