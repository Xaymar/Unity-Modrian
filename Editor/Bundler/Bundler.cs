using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.Xaymar.Modrian
{
    public class Bundler
    {
        public const string KeyExportPath = "com.Xaymar.Modrian.Bundler.Path";
        public const string KeyCompression = "com.Xaymar.Modrian.Bundler.Compression";
        public const string KeyDeterministic = "com.Xaymar.Modrian.Bundler.Deterministic";

        public static string getExportPath()
        {

            {
                string cmd = null;
                var args = System.Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].ToLower() == $"-{KeyExportPath}")
                    {
                        if ((i + 1) < args.Length)
                        {
                            cmd = args[i + 1]; break;
                        }
                        else
                        {
                            Debug.LogError("Ignoring invalid command line option.");
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(cmd))
                {
                    return cmd;
                }
            }

            {
                string env = Environment.GetEnvironmentVariable(KeyExportPath);

                if (!string.IsNullOrEmpty(env))
                {
                    return env;
                }
            }

            {
                string ep = EditorPrefs.GetString(KeyExportPath);

                if (!string.IsNullOrEmpty(ep))
                {
                    return ep;
                }
            }

            return "../BepInEx/Plugins/${bundleName}/${bundleName}.assetBundle";
        }

        public static string getCompressionType()
        {
            {
                string cmd = null;
                var args = System.Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].ToLower() == $"-{KeyCompression}")
                    {
                        if ((i + 1) < args.Length)
                        {
                            cmd = args[i + 1]; break;
                        }
                        else
                        {
                            Debug.LogError("Ignoring invalid command line option.");
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(cmd))
                {
                    return cmd;
                }
            }

            {
                string env = Environment.GetEnvironmentVariable(KeyCompression);

                if (!string.IsNullOrEmpty(env))
                {
                    return env;
                }
            }

            {
                string ep = EditorPrefs.GetString(KeyCompression);

                if (!string.IsNullOrEmpty(ep))
                {
                    return ep;
                }
            }

            return "Chunked";

        }

        public static bool getDeterministic()
        {
            bool result = false;

            {
                string cmd = null;
                var args = System.Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].ToLower() == $"-{KeyDeterministic}")
                    {
                        if ((i + 1) < args.Length)
                        {
                            cmd = args[i + 1]; break;
                        }
                        else
                        {
                            Debug.LogError("Ignoring invalid command line option.");
                            break;
                        }
                    }
                }

                if (bool.TryParse(cmd, out result))
                {
                    return result;
                }
            }

            {
                string env = Environment.GetEnvironmentVariable(KeyDeterministic);
                if (bool.TryParse(env, out result))
                {
                    return result;
                }
            }

            if (EditorPrefs.HasKey(KeyDeterministic))
            {
                return EditorPrefs.GetBool(KeyDeterministic);
            }

            return false;
        }

        public static void build()
        {
            var definitions = new List<AssetBundleBuild>();

            // Build a list of definitions.
            string exportPath = getExportPath();
            var bundles = AssetDatabase.GetAllAssetBundleNames();
            foreach (var bundle in bundles)
            {
                Debug.Log(string.Format("Indexing bundle {0}", bundle));
                var ab = new AssetBundleBuild();
                ab.assetBundleName = $"{exportPath.Replace("${bundleName}", bundle)}";
                ab.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
                definitions.Add(ab);
            }

            // Build the asset bundles.
            Debug.Log("Building Asset Bundles...");

            BuildAssetBundleOptions opts = BuildAssetBundleOptions.AssetBundleStripUnityVersion;
            switch (getCompressionType().ToLower())
            {
                case "uncompressed":
                    opts |= BuildAssetBundleOptions.UncompressedAssetBundle;
                    break;
                default:
                case "chunked":
                    opts |= BuildAssetBundleOptions.ChunkBasedCompression;
                    break;
                case "compressed":
                    break;
            }
#if (! UNITY_5_OR_NEWER)
            if (getDeterministic())
                opts |= BuildAssetBundleOptions.DeterministicAssetBundle;
#endif

            BuildPipeline.BuildAssetBundles(
                Application.dataPath, definitions.ToArray(), opts, EditorUserBuildSettings.activeBuildTarget);
        }

    }
}