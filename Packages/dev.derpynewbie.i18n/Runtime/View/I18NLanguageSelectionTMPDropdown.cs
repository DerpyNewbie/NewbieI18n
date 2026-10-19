using System.Collections.Generic;
using TMPro;
using UdonSharp;
using UnityEngine;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System.Linq;
using UnityEngine.Events;
#endif
namespace DerpyNewbie.I18N.View
{
    /// <summary>
    /// TMP_Dropdown を使う言語選択。
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class I18NLanguageSelectionTMPDropdown : I18NLanguageSelectionDropdownBase
    {
        [SerializeField]
        private TMP_Dropdown dropdown;

        protected override bool HasDropdown() => dropdown != null;

        protected override int GetDropdownValue() => dropdown.value;

        protected override void SetDropdownValueWithoutNotify(int index)
        {
            dropdown.SetValueWithoutNotify(index);
            dropdown.RefreshShownValue();
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override void FindDropdown() => dropdown = GetComponent<TMP_Dropdown>();

        protected override List<string> GetOptionTexts()
        {
            var texts = new List<string>(dropdown.options.Count);
            texts.AddRange(dropdown.options.Select(option => option.text));
            return texts;
        }

        protected override void SetOptionTexts(List<string> names)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(names);
        }

        protected override Object GetDropdownObject() => dropdown;

        protected override UnityEvent<int> GetOnValueChangedEvent() => dropdown.onValueChanged;
#endif
    }
}
