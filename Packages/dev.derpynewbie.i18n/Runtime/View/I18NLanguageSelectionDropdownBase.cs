using DerpyNewbie.Common;
using JetBrains.Annotations;
using System.Collections.Generic;
using UdonSharp;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor.Events;
using UnityEngine.Events;
using VRC.Udon;
#endif
namespace DerpyNewbie.I18N.View
{
    /// <summary>
    /// Shared language selection logic for Dropdown / TMP_Dropdown.
    /// The dropdown options and the SendCustomEvent("OnValueChanged") listener on OnValueChanged
    /// are set up automatically in the editor.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class I18NLanguageSelectionDropdownBase : I18NView
    {
        protected abstract bool HasDropdown();
        protected abstract int GetDropdownValue();
        protected abstract void SetDropdownValueWithoutNotify(int index);

        public override void OnI18NRefresh()
        {
            SyncDropdownValue();
        }

        public void SyncDropdownValue()
        {
            if (provider == null || !HasDropdown()) return;

            var index = provider.GetAvailableLanguages().FindString(provider.GetCurrentLanguage());
            SetDropdownValueWithoutNotify(index);
        }

        // Used from UnityEvent (dropdown)
        [UsedImplicitly]
        public void OnValueChanged()
        {
            if (provider == null || !HasDropdown())
            {
                Debug.LogError($"[I18NLanguageSelectionDropdown-{name}] Provider or Dropdown is null!", this);
                return;
            }

            var languages = provider.GetAvailableLanguages();
            var index = GetDropdownValue();
            if (index < 0 || index >= languages.Length) return;

            var language = languages[index];
            if (language == provider.GetCurrentLanguage()) return;

            provider.SetCurrentLanguage(language);
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected abstract void FindDropdown();
        protected abstract List<string> GetOptionTexts();
        protected abstract void SetOptionTexts(List<string> names);
        protected abstract Object GetDropdownObject();
        protected abstract UnityEvent<int> GetOnValueChangedEvent();

        protected override void OnValidate()
        {
            base.OnValidate();

            if (!HasDropdown()) FindDropdown();
            if (!HasDropdown()) return;

            EnsureOnValueChangedListener();

            if (provider == null) return;

            var languages = provider.GetAvailableLanguages();
            var names = new List<string>(languages.Length);
            foreach (var language in languages) names.Add(provider.GetLanguageName(language));

            if (IsSameOptions(names)) return;

            SetOptionTexts(names);

            var index = languages.FindString(provider.GetCurrentLanguage());
            SetDropdownValueWithoutNotify(index >= 0 ? index : 0);

            var dropdownObject = GetDropdownObject();
            EditorUtility.SetDirty(dropdownObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(dropdownObject);
        }

        private void EnsureOnValueChangedListener()
        {
            var udon = new SerializedObject(this).FindProperty("_udonSharpBackingUdonBehaviour")
                ?.objectReferenceValue as UdonBehaviour;
            if (udon == null) return;

            var onValueChanged = GetOnValueChangedEvent();
            for (var i = 0; i < onValueChanged.GetPersistentEventCount(); i++)
                if (onValueChanged.GetPersistentTarget(i) == udon &&
                    onValueChanged.GetPersistentMethodName(i) == nameof(UdonBehaviour.SendCustomEvent))
                    return;

            var dropdownObject = GetDropdownObject();
            Undo.RecordObject(dropdownObject, "Add I18N Language Selection Listener");
            UnityEventTools.AddStringPersistentListener(onValueChanged, udon.SendCustomEvent, nameof(OnValueChanged));
            EditorUtility.SetDirty(dropdownObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(dropdownObject);
        }

        private bool IsSameOptions(List<string> names)
        {
            var options = GetOptionTexts();
            if (options.Count != names.Count) return false;
            for (var i = 0; i < names.Count; i++)
                if (options[i] != names[i])
                    return false;

            return true;
        }
#endif
    }
}
