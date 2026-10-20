using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System.Collections.Generic;
using UnityEditor;
#endif
namespace DerpyNewbie.I18N.View
{
    /// <summary>
    /// Translates the options of a TMP_Dropdown.
    /// At runtime, the options are rebuilt with VRCTMPDropdownExtension.AddOptions(OptionData[]).
    /// TMP_Dropdown.options cannot be read from Udon, so each option's image is cached in the editor.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class I18NTMPDropdown : I18NView
    {
        [SerializeField]
        private TMP_Dropdown dropdown;
        [SerializeField]
        protected string[] keys;

        [SerializeField] [HideInInspector]
        private Sprite[] images;

        public override void OnI18NRefresh()
        {
            if (dropdown == null)
            {
                Debug.LogError($"[I18NTMPDropdown-{name}] Dropdown is null!", this);
                return;
            }

            if (provider == null)
            {
                Debug.LogError($"[I18NTMPDropdown-{name}] Provider is null!", this);
                return;
            }

            var keyCount = keys != null ? keys.Length : 0;
            var options = new TMP_Dropdown.OptionData[keyCount];
            for (var i = 0; i < keyCount; i++)
            {
                var text = provider.GetString(keys[i]);
                var image = images != null && i < images.Length ? images[i] : null;
                options[i] = new TMP_Dropdown.OptionData(text != null ? text : keys[i], image);
            }

            var value = dropdown.value;
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.SetValueWithoutNotify(value);
            dropdown.RefreshShownValue();
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override void OnValidate()
        {
            if (dropdown == null) dropdown = GetComponent<TMP_Dropdown>();
            if (dropdown == null) return;

            CacheImages();

            if (provider == null) provider = FindObjectOfType<I18NProvider>();
            if (provider == null) return;

            ApplyEditorPreview();
        }

        private void CacheImages()
        {
            var keyCount = keys != null ? keys.Length : 0;
            var options = dropdown.options;
            var cached = new Sprite[keyCount];
            for (var i = 0; i < keyCount; i++)
                cached[i] = i < options.Count ? options[i].image : null;

            images = cached;
        }

        // In the editor, use List to set the options to the current language's text
        private void ApplyEditorPreview()
        {
            var keyCount = keys != null ? keys.Length : 0;
            var options = new List<TMP_Dropdown.OptionData>(keyCount);
            for (var i = 0; i < keyCount; i++)
                options.Add(new TMP_Dropdown.OptionData(provider.GetString(keys[i]) ?? keys[i], images[i]));

            if (IsSameOptions(options)) return;

            var value = dropdown.value;
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.SetValueWithoutNotify(value);
            dropdown.RefreshShownValue();

            EditorUtility.SetDirty(dropdown);
            PrefabUtility.RecordPrefabInstancePropertyModifications(dropdown);
        }

        private bool IsSameOptions(List<TMP_Dropdown.OptionData> source)
        {
            var options = dropdown.options;
            if (options.Count != source.Count) return false;
            for (var i = 0; i < source.Count; i++)
                if (options[i].text != source[i].text || options[i].image != source[i].image)
                    return false;

            return true;
        }
#endif
    }
}
