using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using UdonSharp;
using UnityEditor;
using UnityEngine;
namespace DerpyNewbie.I18N.View
{
    /// <summary>
    /// Dropdown の OnValueChanged に SendCustomEvent("OnValueChanged") を登録して使う。
    /// Dropdown の選択肢はエディタ上で I18NProvider の言語一覧から自動生成される。
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class I18NLanguageSelectionDropdown : I18NView
    {
        [SerializeField]
        private TMP_Dropdown dropdown;

        public override void OnI18NRefresh()
        {
            SyncDropdownValue();
        }

        public void SyncDropdownValue()
        {
            if (provider == null || dropdown == null) return;

            var index = IndexOfCurrentLanguage(provider.GetAvailableLanguages());
            dropdown.SetValueWithoutNotify(index);
            dropdown.RefreshShownValue();
        }

        // Used from UnityEvent (dropdown)
        [UsedImplicitly]
        public void OnValueChanged()
        {
            if (provider == null || dropdown == null)
            {
                Debug.LogError($"[I18NLanguageSelectionDropdown-{name}] Provider or Dropdown is null!", this);
                return;
            }

            var languages = provider.GetAvailableLanguages();
            var index = dropdown.value;
            if (index < 0 || index >= languages.Length) return;

            var language = languages[index];
            if (language == provider.GetCurrentLanguage()) return;

            provider.SetCurrentLanguage(language);
        }

        private int IndexOfCurrentLanguage(string[] languages)
        {
            var current = provider.GetCurrentLanguage();
            for (var i = 0; i < languages.Length; i++)
                if (languages[i] == current)
                    return i;

            return -1;
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override void OnValidate()
        {
            base.OnValidate();

            if (dropdown == null) dropdown = GetComponent<TMP_Dropdown>();
            if (provider == null || dropdown == null) return;

            var languages = provider.GetAvailableLanguages();
            var names = new List<string>(languages.Length);
            foreach (var language in languages) names.Add(provider.GetLanguageName(language));

            if (IsSameOptions(names)) return;

            dropdown.ClearOptions();
            dropdown.AddOptions(names);

            var index = IndexOfCurrentLanguage(languages);
            dropdown.SetValueWithoutNotify(index >= 0 ? index : 0);

            EditorUtility.SetDirty(dropdown);
            PrefabUtility.RecordPrefabInstancePropertyModifications(dropdown);
        }

        private bool IsSameOptions(List<string> names)
        {
            var options = dropdown.options;
            if (options.Count != names.Count) return false;
            for (var i = 0; i < names.Count; i++)
                if (options[i].text != names[i])
                    return false;

            return true;
        }
#endif
    }
}
