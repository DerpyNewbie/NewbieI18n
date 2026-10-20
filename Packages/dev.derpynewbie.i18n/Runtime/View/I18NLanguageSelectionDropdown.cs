using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System.Linq;
using UnityEngine.Events;
#endif
namespace DerpyNewbie.I18N.View
{
    /// <summary>
    /// Language selection using a Dropdown.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class I18NLanguageSelectionDropdown : I18NLanguageSelectionDropdownBase
    {
        [SerializeField]
        private Dropdown dropdown;

        protected override bool HasDropdown() => dropdown != null;

        protected override int GetDropdownValue() => dropdown.value;

        protected override void SetDropdownValueWithoutNotify(int index)
        {
            dropdown.SetValueWithoutNotify(index);
            dropdown.RefreshShownValue();
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override void FindDropdown() => dropdown = GetComponent<Dropdown>();

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
