using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
namespace DerpyNewbie.I18N
{
    public class I18NManager : I18NProvider
    {
        [SerializeField]
        private string localeJson;
        [SerializeField]
        private string currentLanguage = "ja";

        private DataDictionary _textDict;
        private string[] _languages = new string[0];

        private void Start()
        {
            currentLanguage = VRCPlayerApi.GetCurrentLanguage();
            EnsureLoaded();
        }

        public override void OnLanguageChanged(string language)
        {
            currentLanguage = language;
            RefreshViews();
        }

        public void Reload()
        {
            Debug.Log("[I18NManager]Loading locale json");

            if (!VRCJson.TryDeserializeFromJson(localeJson, out var result))
            {
                Debug.Log($"[I18NManager] Failed to deserialize locale json: {result.Error}");
                return;
            }

            var resultDict = result.DataDictionary;
            _languages = ToStringArray(resultDict["languages"].DataList);
            _textDict = resultDict["texts"].DataDictionary;
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
            return _languages;
        }

        public override bool SetCurrentLanguage(string language)
        {
            OnLanguageChanged(language);
            return true;
        }

        private static string[] ToStringArray(DataList list)
        {
            var strArr = new string[list.Count];
            for (var i = 0; i < list.Count; i++)
                strArr[i] = list[i].String;

            return strArr;
        }
    }
}
