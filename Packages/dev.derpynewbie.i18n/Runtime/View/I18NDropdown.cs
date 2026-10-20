using DerpyNewbie.Common;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon.Serialization.OdinSerializer;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
#endif
namespace DerpyNewbie.I18N.View
{
    /// <summary>
    /// Translates the options of a Dropdown.
    /// Udon cannot modify the contents of a List&lt;OptionData&gt;, so an option list per language
    /// is generated and stored in the editor, and Dropdown.options is swapped wholesale at runtime.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class I18NDropdown : I18NView
    {
        [SerializeField]
        private Dropdown dropdown;
        [SerializeField]
        protected string[] keys;

        [SerializeField] [HideInInspector]
        private string[] languages;
        // List<Dropdown.OptionData> per language. Udon cannot handle arrays of List, so they are stored as object[].
        [OdinSerialize]
        private object[] _languageOptions;

        public override void OnI18NRefresh()
        {
            if (dropdown == null)
            {
                Debug.LogError($"[I18NDropdown-{name}] Dropdown is null!", this);
                return;
            }

            if (provider == null)
            {
                Debug.LogError($"[I18NDropdown-{name}] Provider is null!", this);
                return;
            }

            if (languages == null || _languageOptions == null) return;

            var index = languages.FindString(provider.GetCurrentLanguage());
            if (index < 0 || index >= _languageOptions.Length)
            {
                Debug.LogWarning($"[I18NDropdown-{name}] No options for language {provider.GetCurrentLanguage()}",
                    this);
                return;
            }

            var value = dropdown.value;
            dropdown.options = (List<Dropdown.OptionData>)_languageOptions[index];
            dropdown.SetValueWithoutNotify(value);
            dropdown.RefreshShownValue();
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override void OnValidate()
        {
            if (dropdown == null) dropdown = GetComponent<Dropdown>();
            if (provider == null) provider = FindObjectOfType<I18NProvider>();
            if (dropdown == null || provider == null) return;

            BuildLanguageOptions();
            ApplyEditorPreview();
        }

        private void BuildLanguageOptions()
        {
            var available = provider.GetAvailableLanguages();
            var currentOptions = dropdown.options;
            var keyCount = keys?.Length ?? 0;

            languages = (string[])available.Clone();
            _languageOptions = new object[available.Length];
            for (var i = 0; i < available.Length; i++)
            {
                var options = new List<Dropdown.OptionData>(keyCount);
                for (var j = 0; j < keyCount; j++)
                {
                    var key = keys?[j];
                    var text = provider.GetString(key, available[i]) ?? key;
                    var image = j < currentOptions.Count ? currentOptions[j].image : null;
                    options.Add(new Dropdown.OptionData(text, image));
                }

                _languageOptions[i] = options;
            }
        }

        // In the editor, copy the stored list into the Dropdown instead of sharing it
        private void ApplyEditorPreview()
        {
            var index = languages.FindString(provider.GetCurrentLanguage());
            if (index < 0) index = 0;
            if (index >= _languageOptions.Length) return;

            var source = (List<Dropdown.OptionData>)_languageOptions[index];
            if (IsSameOptions(source)) return;

            var value = dropdown.value;
            dropdown.ClearOptions();
            foreach (var option in source)
                dropdown.options.Add(new Dropdown.OptionData(option.text, option.image));
            dropdown.SetValueWithoutNotify(value);
            dropdown.RefreshShownValue();

            EditorUtility.SetDirty(dropdown);
            PrefabUtility.RecordPrefabInstancePropertyModifications(dropdown);
        }

        private bool IsSameOptions(List<Dropdown.OptionData> source)
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
