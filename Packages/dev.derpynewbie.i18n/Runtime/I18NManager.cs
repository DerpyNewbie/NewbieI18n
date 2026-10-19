using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
namespace DerpyNewbie.I18N
{
    public class I18NManager : I18NProvider
    {
        [SerializeField]
        private string currentLanguage = "ja";
        [SerializeField] [TextArea]
        private string localeJson;

        private DataDictionary _textDict;
        private string[] _languages = new string[0];
        private string[] _languageNames = new string[0];

        private void Start()
        {
            EnsureLoaded();
            OnLanguageChanged(VRCPlayerApi.GetCurrentLanguage());
        }

        public override void OnLanguageChanged(string language)
        {
            currentLanguage = language;
            RefreshViews();
        }

        public void Reload()
        {
            Debug.Log("[I18NManager] Loading locale json");

            if (!VRCJson.TryDeserializeFromJson(localeJson, out var result))
            {
                Debug.Log($"[I18NManager] Failed to deserialize locale json: {result.Error}");
                return;
            }

            var resultDict = result.DataDictionary;
            _languages = ToStringArray(resultDict["languages"].DataList);
            _textDict = resultDict["texts"].DataDictionary;
            _languageNames = resultDict.TryGetValue("languageNames", TokenType.DataList, out var names) ? ToStringArray(names.DataList) : _languages;
        }

        private void EnsureLoaded()
        {
            if (_textDict != null)
            {
                return;
            }

            Reload();
        }

        public override string GetString(string key, string language)
        {
            EnsureLoaded();

            if (!_textDict.TryGetValue(key, TokenType.DataDictionary, out var entry)) return key;
            if (entry.DataDictionary.TryGetValue(language, TokenType.String, out var value)) return value.String;
            // 訳がない場合は先頭の言語にフォールバック
            if (entry.DataDictionary.TryGetValue(_languages[0], TokenType.String, out value)) return value.String;
            return key;
        }

        public override string GetCurrentLanguage()
        {
            return currentLanguage;
        }

        public override string[] GetAvailableLanguages()
        {
            EnsureLoaded();
            return _languages;
        }

        public override string GetLanguageName(string language)
        {
            EnsureLoaded();
            for (var i = 0; i < _languages.Length && i < _languageNames.Length; i++)
                if (_languages[i] == language)
                    return _languageNames[i];

            return language;
        }

        public override bool SetCurrentLanguage(string language)
        {
            OnLanguageChanged(language);
            return true;
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void OnValidate()
        {
            // localeJson が書き換えられたら次回アクセス時に読み直す
            _textDict = null;
            Reload();
        }
#endif

        private static string[] ToStringArray(DataList list)
        {
            var strArr = new string[list.Count];
            for (var i = 0; i < list.Count; i++)
                strArr[i] = list[i].String;

            return strArr;
        }
    }
}
