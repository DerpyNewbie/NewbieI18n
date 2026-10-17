using UdonSharpEditor;
using UnityEditor;

namespace DerpyNewbie.I18N.Editor
{
    [CustomEditor(typeof(I18NView), true)]
    [CanEditMultipleObjects]
    public class I18NViewEditor : UnityEditor.Editor
    {
        private SerializedProperty _provider;
        private SerializedProperty _key;

        private void OnEnable()
        {
            _provider = serializedObject.FindProperty("provider");
            _key = serializedObject.FindProperty("key");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            DrawDefaultInspector();

            if (_key == null || _provider.hasMultipleDifferentValues || _key.hasMultipleDifferentValues) return;

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.TextField("Preview", GetPreview());
        }

        private string GetPreview()
        {
            var provider = _provider.objectReferenceValue as I18NProvider;
            if (provider == null) return "<PROVIDER_NOT_FOUND>";

            return provider.GetString(_key.stringValue) ?? "<INVALID_KEY>";
        }
    }
}
