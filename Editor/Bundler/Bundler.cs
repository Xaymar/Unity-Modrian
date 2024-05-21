using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.xaymar.guardian {
public class Bundler : EditorWindow
{
    string _pathToSelf;
    string _pathToUI;

    TextField _pathElement;
    Button _pathButtonElement;
    Button _exportButtonElement;
    DropdownField _optCompression;
    Toggle _optDeterministic;

    string[] _assetBundles;

    public Bundler()
    {
    }

    public void OnEnable()
    {
        _pathToSelf = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
        _pathToUI = Path.Combine(Path.GetDirectoryName(_pathToSelf), "Bundler.uxml");
        Debug.Log(_pathToSelf);
        Debug.Log(_pathToUI);

        rootVisualElement.RegisterCallback<GeometryChangedEvent>(geometryChanged);
    }

    public void CreateGUI()
    {
        Debug.Log("CreateGUI");
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
        _pathElement.value = Path.GetRelativePath(Application.dataPath, "AssetBundles");
        _pathButtonElement = root.Query<Button>("pathButton");
        _pathButtonElement.clicked += pathButtonClicked;
        _exportButtonElement = root.Query<Button>("export");
        _exportButtonElement.clicked += exportButtonClicked;
        _optCompression = root.Query<DropdownField>("optCompression");
        _optDeterministic = root.Query<Toggle>("optDeterministic");
    }

    private void geometryChanged(GeometryChangedEvent evt)
    {
        var tgt = rootVisualElement.Q<VisualElement>("root");

        // Set up minimum size.
        minSize = new Vector2(tgt.resolvedStyle.minWidth.value, tgt.resolvedStyle.minHeight.value);

    } 

    private void pathButtonClicked()
    { 
        var path = EditorUtility.OpenFolderPanel("Select Directory", _pathElement.value, "");
        if (path != null)
        {
            _pathElement.value = Path.GetRelativePath(Application.dataPath, path);
        }
    }

    private void exportButtonClicked()
    {
        var definitions = new List<AssetBundleBuild>();

        // Build a list of definitions.
        var bundles = AssetDatabase.GetAllAssetBundleNames();
        foreach (var bundle in bundles)
        {
            Debug.Log(string.Format("Indexing bundle {0}", bundle));
            var ab = new AssetBundleBuild();
            ab.assetBundleName = bundle;
            ab.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
            definitions.Add(ab);
        }

        // Build the asset bundles.
        Debug.Log("Building Asset Bundles...");
        var fullPath = Path.GetFullPath(_pathElement.value, Application.dataPath);
        if (!System.IO.Directory.Exists(fullPath))
        {
            System.IO.Directory.CreateDirectory(fullPath);
        }

        BuildAssetBundleOptions opts = BuildAssetBundleOptions.AssetBundleStripUnityVersion;
        switch (_optCompression.value)
        {
            case "Uncompressed":
                opts |= BuildAssetBundleOptions.UncompressedAssetBundle;
                break;
            case "Chunked":
                opts |= BuildAssetBundleOptions.ChunkBasedCompression;
                break;
            case "Compressed":
                break;
        }
        if (_optDeterministic.value)
            opts |= BuildAssetBundleOptions.DeterministicAssetBundle;

        BuildPipeline.BuildAssetBundles(
            fullPath, definitions.ToArray(), opts, BuildTarget.StandaloneWindows);
    }


    [MenuItem("Guardian/Bundler")]
    public static void onMenuGuardianBundler()
    {
        Bundler wnd = GetWindow<Bundler>();
        wnd.titleContent = new GUIContent("Bundler");
    }
}
}