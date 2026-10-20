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
        [Tooltip("Sprites looked up by name. The translated value of a key is used as the sprite name.")]
        [SerializeField]
        private Sprite[] sprites = new Sprite[0];
        [Tooltip("Textures looked up by name. The translated value of a key is used as the texture name.")]
        [SerializeField]
        private Texture[] textures = new Texture[0];

        private DataDictionary _textDict;
        // Asset name -> index into sprites / textures
        private DataDictionary _spriteIndex;
        private DataDictionary _textureIndex;
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

            _spriteIndex = BuildSpriteIndex(sprites);
            _textureIndex = BuildTextureIndex(textures);

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
            // Fall back to the first language when there is no translation
            if (entry.DataDictionary.TryGetValue(_languages[0], TokenType.String, out value)) return value.String;
            return key;
        }

        public override Sprite GetSprite(string key, string language)
        {
            EnsureLoaded();

            if (!_spriteIndex.TryGetValue(GetString(key, language), TokenType.Int, out var index)) return null;
            return sprites[index.Int];
        }

        public override Texture GetTexture(string key, string language)
        {
            EnsureLoaded();

            if (!_textureIndex.TryGetValue(GetString(key, language), TokenType.Int, out var index)) return null;
            return textures[index.Int];
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
            // Reload on next access when localeJson has been changed
            _textDict = null;
            Reload();
            WarnDuplicateNames(sprites, "sprite");
            WarnDuplicateNames(textures, "texture");
        }

        private void WarnDuplicateNames(Object[] assets, string label)
        {
            if (assets == null) return;

            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var asset in assets)
            {
                if (asset != null && !seen.Add(asset.name))
                    Debug.LogWarning($"[I18NManager] Duplicate {label} name \"{asset.name}\". Only the first one is used.", this);
            }
        }
#endif

        // Udon arrays are not covariant, so sprites and textures are indexed by separate methods
        private static DataDictionary BuildSpriteIndex(Sprite[] assets)
        {
            var index = new DataDictionary();
            if (assets == null) return index;

            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                // The first asset wins when names are duplicated
                if (asset == null || index.ContainsKey(asset.name)) continue;
                index.Add(asset.name, i);
            }

            return index;
        }

        private static DataDictionary BuildTextureIndex(Texture[] assets)
        {
            var index = new DataDictionary();
            if (assets == null) return index;

            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                // The first asset wins when names are duplicated
                if (asset == null || index.ContainsKey(asset.name)) continue;
                index.Add(asset.name, i);
            }

            return index;
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
