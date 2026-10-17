using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;

namespace DerpyNewbie.I18N.Editor
{
    /// <summary>
    /// Google スプレッドシート(または CSV ファイル)から JSON を生成し、
    /// シーン内の I18NManager.localeJson に書き込む(JSON ファイル出力は任意)。
    ///
    /// シートの形式:
    ///   | key        | ja       | en       | #memo      |
    ///   | @name      | 日本語   | English  |            |  ← Dropdown に表示する言語名(省略可)
    ///   | # コメント行 |          |          |            |  ← key が # で始まる行は無視
    ///   | menu.title | メニュー  | Menu     | タイトル用  |  ← ヘッダーが # で始まる列は無視
    /// </summary>
    public class I18NSheetExporter : EditorWindow
    {
        private const string NameRowKey = "@name";

        private string _sheetUrl = "";
        private bool _writeFile;
        private string _outputPath = "Assets/I18N/locale.json";

        private UnityWebRequest _request;
        private string _status = "";
        private MessageType _statusType = MessageType.None;

        [MenuItem("Tools/DerpyNewbie/I18N/Sheet Exporter")]
        private static void Open()
        {
            GetWindow<I18NSheetExporter>("I18N Exporter");
        }

        private static string PrefKey(string name)
        {
            return "I18NSheetExporter." + PlayerSettings.productGUID + "." + name;
        }

        private void OnEnable()
        {
            _sheetUrl = EditorPrefs.GetString(PrefKey("sheetUrl"), _sheetUrl);
            _writeFile = EditorPrefs.GetBool(PrefKey("writeFile"), _writeFile);
            _outputPath = EditorPrefs.GetString(PrefKey("outputPath"), _outputPath);
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(PrefKey("sheetUrl"), _sheetUrl);
            EditorPrefs.SetBool(PrefKey("writeFile"), _writeFile);
            EditorPrefs.SetString(PrefKey("outputPath"), _outputPath);
        }

        private void OnHierarchyChange()
        {
            Repaint();
        }

        private static I18NManager[] FindManagers()
        {
            return FindObjectsByType<I18NManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("入力", EditorStyles.boldLabel);
            _sheetUrl = EditorGUILayout.TextField("スプレッドシート URL", _sheetUrl);
            EditorGUILayout.HelpBox("共有設定を「リンクを知っている全員が閲覧可」にしてください。\nURL はシートのタブを開いた状態でアドレスバーからコピー(#gid= 付き)。", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("出力", EditorStyles.boldLabel);
            var managers = FindManagers();
            if (managers.Length == 0)
            {
                EditorGUILayout.HelpBox("シーンに I18NManager が見つかりません。", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("書き込み先(シーン内の I18NManager)");
                using (new EditorGUI.DisabledScope(true))
                {
                    foreach (var manager in managers)
                    {
                        EditorGUILayout.ObjectField(manager, typeof(I18NManager), true);
                    }
                }
            }

            _writeFile = EditorGUILayout.Toggle("JSON ファイルも書き出す", _writeFile);
            using (new EditorGUI.DisabledScope(!_writeFile))
            using (new EditorGUILayout.HorizontalScope())
            {
                _outputPath = EditorGUILayout.TextField("JSON 出力先", _outputPath);
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    var picked = EditorUtility.SaveFilePanelInProject("JSON 出力先", "locale", "json", "");
                    if (!string.IsNullOrEmpty(picked)) _outputPath = picked;
                }
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(_request != null))
            {
                if (GUILayout.Button(_request != null ? "取得中..." : "スプレッドシートから書き出し", GUILayout.Height(28)))
                {
                    FetchSheet();
                }
                if (GUILayout.Button("ローカル CSV から書き出し"))
                {
                    var path = EditorUtility.OpenFilePanel("CSV を選択", "", "csv");
                    if (!string.IsNullOrEmpty(path)) Export(File.ReadAllText(path, Encoding.UTF8));
                }
            }

            if (GUILayout.Button("全てのテキストを更新する"))
            {
                UpdateAllViews();
            }

            if (!string.IsNullOrEmpty(_status))
            {
                EditorGUILayout.HelpBox(_status, _statusType);
            }
        }

        private void UpdateAllViews()
        {
            var managers = FindManagers();
            foreach (var manager in managers)
            {
                manager.Reload();
            }

            var views = FindObjectsByType<I18NView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var view in views)
            {
                view.OnI18NRefresh();
            }

            _status = $"Updated {views.Length} views";
            _statusType = MessageType.Info;
        }

        // ---------- 取得 ----------
        private void FetchSheet()
        {
            var m = Regex.Match(_sheetUrl, @"/spreadsheets/d/([a-zA-Z0-9_-]+)");
            if (!m.Success)
            {
                SetStatus("URL からスプレッドシート ID を読み取れませんでした。", MessageType.Error);
                return;
            }
            var gidMatch = Regex.Match(_sheetUrl, @"[#?&]gid=(\d+)");
            var gid = gidMatch.Success ? gidMatch.Groups[1].Value : "0";

            var url = "https://docs.google.com/spreadsheets/d/" + m.Groups[1].Value + "/export?format=csv&gid=" + gid;
            _request = UnityWebRequest.Get(url);
            _request.SendWebRequest();
            EditorApplication.update += PollRequest;
            SetStatus("取得中...", MessageType.None);
        }

        private void PollRequest()
        {
            if (_request == null || !_request.isDone) return;
            EditorApplication.update -= PollRequest;

            try
            {
                var contentType = _request.GetResponseHeader("Content-Type") ?? "";
                if (_request.result != UnityWebRequest.Result.Success)
                {
                    SetStatus("取得に失敗しました: " + _request.error, MessageType.Error);
                }
                else if (contentType.Contains("text/html"))
                {
                    // 非公開シートだとログインページの HTML が返ってくる
                    SetStatus("CSV ではなく HTML が返されました。シートの共有設定を確認してください。", MessageType.Error);
                }
                else
                {
                    Export(_request.downloadHandler.text);
                }
            }
            finally
            {
                _request.Dispose();
                _request = null;
                Repaint();
            }
        }

        // ---------- 変換 ----------
        private void Export(string csv)
        {
            var managers = FindManagers();
            if (managers.Length == 0 && !_writeFile)
            {
                SetStatus("書き込み先がありません。シーンに I18NManager を置くか、JSON ファイル出力を有効にしてください。", MessageType.Error);
                return;
            }
            if (_writeFile && !_outputPath.StartsWith("Assets/"))
            {
                SetStatus("出力先は Assets/ 以下にしてください。", MessageType.Error);
                return;
            }

            var rows = ParseCsv(csv);
            var warnings = new List<string>();
            var json = BuildJson(rows, out var languageNames, warnings, out var keyCount);
            if (json == null)
            {
                SetStatus(string.Join("\n", warnings), MessageType.Error);
                return;
            }

            foreach (var manager in managers)
            {
                ApplyToManager(manager, json);
            }

            if (_writeFile)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_outputPath) ?? throw new InvalidOperationException());
                File.WriteAllText(_outputPath, json, new UTF8Encoding(false));
                AssetDatabase.ImportAsset(_outputPath);
            }

            foreach (var w in warnings) Debug.LogWarning("[I18N] " + w);
            var summary = "書き出し完了: I18NManager " + managers.Length + " 件" + (_writeFile ? " + " + _outputPath : "")
                          + "\nキー " + keyCount + " 件 / 言語 " + languageNames.Length + " 件";
            if (warnings.Count > 0) summary += "\n警告 " + warnings.Count + " 件(Console を確認)";
            Debug.Log("[I18N] " + summary);
            SetStatus(summary, warnings.Count > 0 ? MessageType.Warning : MessageType.Info);
        }

        private static string BuildJson(List<List<string>> rows, out string[] languageNames, List<string> warnings, out int keyCount)
        {
            languageNames = new string[0];
            keyCount = 0;
            if (rows.Count == 0)
            {
                warnings.Add("シートが空です。");
                return null;
            }

            // ヘッダー行から言語列を拾う(空 or # 始まりの列は無視)
            var langCols = new List<int>();
            var langCodes = new List<string>();
            var header = rows[0];
            for (var c = 1; c < header.Count; c++)
            {
                var code = header[c].Trim();
                if (code.Length == 0 || code.StartsWith("#")) continue;
                langCols.Add(c);
                langCodes.Add(code);
            }
            if (langCols.Count == 0)
            {
                warnings.Add("ヘッダー行に言語コードの列がありません(例: key, ja, en)。");
                return null;
            }

            languageNames = langCodes.ToArray();
            var seen = new HashSet<string>();
            var texts = new StringBuilder();

            for (var r = 1; r < rows.Count; r++)
            {
                var row = rows[r];
                var key = row.Count > 0 ? row[0].Trim() : "";
                if (key.Length == 0 || key.StartsWith("#")) continue;

                if (key == NameRowKey)
                {
                    for (var i = 0; i < langCols.Count; i++)
                    {
                        var name = Cell(row, langCols[i]);
                        if (name.Length > 0) languageNames[i] = name;
                    }
                    continue;
                }

                if (!seen.Add(key))
                {
                    warnings.Add((r + 1) + " 行目: キー \"" + key + "\" が重複しています(後の行は無視)。");
                    continue;
                }

                var entry = new StringBuilder();
                var missing = new List<string>();
                for (var i = 0; i < langCols.Count; i++)
                {
                    var value = Cell(row, langCols[i]);
                    if (value.Length == 0)
                    {
                        missing.Add(langCodes[i]);
                        continue;
                    }
                    if (entry.Length > 0) entry.Append(", ");
                    entry.Append(Quote(langCodes[i])).Append(": ").Append(Quote(value));
                }
                if (missing.Count > 0)
                {
                    warnings.Add("\"" + key + "\" の訳がありません: " + string.Join(", ", missing));
                }

                if (keyCount > 0) texts.Append(",\n");
                texts.Append("    ").Append(Quote(key)).Append(": { ").Append(entry).Append(" }");
                keyCount++;
            }

            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"languages\": ").Append(QuoteArray(langCodes)).Append(",\n");
            sb.Append("  \"languageNames\": ").Append(QuoteArray(languageNames)).Append(",\n");
            sb.Append("  \"texts\": {\n").Append(texts).Append("\n  }\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        private static void ApplyToManager(I18NManager manager, string json)
        {
            var so = new SerializedObject(manager);
            var jsonProp = so.FindProperty("localeJson");
            if (jsonProp == null)
            {
                Debug.LogError("[I18N] I18NManager に localeJson (string) フィールドがありません。", manager);
                return;
            }
            jsonProp.stringValue = json;
            // ApplyModifiedProperties が Undo 記録・Dirty 化・Prefab オーバーライド記録まで行う
            if (so.ApplyModifiedProperties())
            {
                UdonSharpEditorUtility.CopyProxyToUdon(manager);
                if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            }
        }

        // ---------- ユーティリティ ----------

        private static string Cell(List<string> row, int index)
        {
            return index < row.Count ? row[index] : "";
        }

        /// <summary>RFC 4180 形式の CSV をパースする(セル内の改行・カンマ・"" に対応)。</summary>
        private static List<List<string>> ParseCsv(string text)
        {
            text = text.TrimStart('﻿').Replace("\r\n", "\n").Replace('\r', '\n');

            var rows = new List<List<string>>();
            var row = new List<string>();
            var cell = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (inQuotes)
                {
                    if (c != '"') cell.Append(c);
                    else if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        cell.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }

                    continue;
                }

                switch (c)
                {
                    case '"':
                        inQuotes = true;
                        break;
                    case ',':
                        row.Add(cell.ToString());
                        cell.Clear();
                        break;
                    case '\n':
                        row.Add(cell.ToString());
                        cell.Clear();
                        rows.Add(row);
                        row = new List<string>();
                        break;
                    default:
                        cell.Append(c);
                        break;
                }
            }

            if (cell.Length <= 0 && row.Count <= 0)
            {
                return rows;
            }

            row.Add(cell.ToString());
            rows.Add(row);
            return rows;
        }

        private static string Quote(string s)
        {
            var sb = new StringBuilder("\"");
            foreach (var c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            return sb.Append('"').ToString();
        }

        private static string QuoteArray(IList<string> items)
        {
            var quoted = new List<string>();
            foreach (var s in items) quoted.Add(Quote(s));
            return "[" + string.Join(", ", quoted) + "]";
        }

        private void SetStatus(string message, MessageType type)
        {
            _status = message;
            _statusType = type;
            Repaint();
        }
    }
}
