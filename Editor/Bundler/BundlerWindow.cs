using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.Xaymar.Modrian;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;

namespace com.Xaymar.Modrian
{
    public class BundlerWindow : EditorWindow
    {
        [MenuItem("Modrian/Bundler")]
        public static void onMenuModrianBundler()
        {
            BundlerWindow wnd = GetWindow<BundlerWindow>();
            wnd.titleContent = new GUIContent("Bundler");
        }

        string _pathToSelf;
        string _pathToUI;

        TextField _pathElement;
        Button _pathButtonElement;
        Button _exportButtonElement;
        DropdownField _optCompression;
        Toggle _optDeterministic;

        public void OnEnable()
        {
            _pathToSelf = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            _pathToUI = Path.Combine(Path.GetDirectoryName(_pathToSelf), "Bundler.uxml");
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(geometryChanged);
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;

            // Load the necessary UI elements.
            var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_pathToUI);
            if (!uiAsset)
            {
                Debug.Log(string.Format("Failed to load '{0}', how did this happen?", _pathToUI));
                throw new System.IO.FileNotFoundException(_pathToUI);
            }

            // Instantiate them for our usage.
            var ui = uiAsset.Instantiate();
            root.Add(ui);

            // Bind them to the necessary functionality.
            _pathElement = root.Query<TextField>("path");
            _pathElement.RegisterCallback<ChangeEvent<string>>(pathChanged);
            _pathButtonElement = root.Query<Button>("pathButton");
            _pathButtonElement.clicked += pathButtonClicked;
            _exportButtonElement = root.Query<Button>("export");
            _exportButtonElement.clicked += exportButtonClicked;
            _optCompression = root.Query<DropdownField>("optCompression");
            _optCompression.RegisterCallback<ChangeEvent<string>>(optCompressionChanged);
            _optDeterministic = root.Query<Toggle>("optDeterministic");
            _optDeterministic.RegisterCallback<ChangeEvent<bool>>(optDeterministicChanged);

            // Reload stored options.
            _pathElement.value = Bundler.getExportPath();
            _optCompression.value = Bundler.getCompressionType();
            _optDeterministic.value = Bundler.getDeterministic();
        }

        private void geometryChanged(GeometryChangedEvent evt)
        {
            var tgt = rootVisualElement.Q<VisualElement>("root");

            // Set up minimum size.
            minSize = new Vector2(tgt.resolvedStyle.minWidth.value, tgt.resolvedStyle.minHeight.value);
        }

        private void pathChanged(ChangeEvent<string> evt)
        {
            EditorPrefs.SetString(Bundler.KeyExportPath, evt.newValue);
        }

        private void pathButtonClicked()
        {
            var path = EditorUtility.OpenFolderPanel("Select Directory", _pathElement.value, "");
            if (path != null)
            {
                _pathElement.value = Path.GetRelativePath(Application.dataPath, path);
            }
        }

        private void optCompressionChanged(ChangeEvent<string> evt)
        {
            EditorPrefs.SetString(Bundler.KeyCompression, evt.newValue);
        }

        private void optDeterministicChanged(ChangeEvent<bool> evt)
        {
            EditorPrefs.SetBool(Bundler.KeyDeterministic, evt.newValue);
        }

        private void exportButtonClicked()
        {
            Bundler.build();
        }
    }
}
