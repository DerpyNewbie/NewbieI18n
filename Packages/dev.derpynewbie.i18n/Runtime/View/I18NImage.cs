using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
namespace DerpyNewbie.I18N.View
{
    /// <summary>
    /// Swaps the sprite of Image with the one provided by <see cref="I18NProvider.GetSprite(string, string)"/>.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class I18NImage : I18NView
    {
        [SerializeField]
        private Image image;
        [SerializeField]
        protected string key;

        public override void OnI18NRefresh()
        {
            if (image == null)
            {
                Debug.LogError($"[I18NImage-{name}] Image is null!", this);
                return;
            }

            if (provider == null)
            {
                Debug.LogError($"[I18NImage-{name}] Provider is null!", this);
                return;
            }

            var sprite = provider.GetSprite(key);
            if (sprite == null)
            {
                Debug.LogWarning($"[I18NImage-{name}] No sprite found for key {key}", this);
                return;
            }

            image.sprite = sprite;
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override void OnValidate()
        {
            if (image == null) image = GetComponent<Image>();
            base.OnValidate();
        }
#endif
    }
}
