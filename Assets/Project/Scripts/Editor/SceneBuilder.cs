using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Labyrinthino.Audio;
using Labyrinthino.Core;
using Labyrinthino.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Labyrinthino.Editor
{
    /// <summary>
    /// Generates the whole game scene, imports the art and configures the player settings.
    /// Run it from the menu Labyrinthino / Build Scene.
    /// </summary>
    public static partial class SceneBuilder
    {
        private const string ProjectRoot = "Assets/Project";
        private const string ScenePath = ProjectRoot + "/Scenes/Main.unity";
        private const float ContentWidth = 1080f;
        private const float ContentHeight = 1920f;

        private static Font _titleFont;
        private static Font _bodyFont;
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Rebuilds the scene from scratch and saves it.
        /// </summary>
        [MenuItem("Labyrinthino/Build Scene")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            RemoveTemplateAssets();
            ConfigureImports();
            _titleFont = AssetDatabase.LoadAssetAtPath<Font>(ProjectRoot + "/Art/Fonts/Lobster-Regular.ttf");
            _bodyFont = AssetDatabase.LoadAssetAtPath<Font>(ProjectRoot + "/Art/Fonts/Nunito-Regular.ttf");
            SpriteCache.Clear();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            CreateEventSystem();
            AudioController audio = CreateAudio();
            Canvas canvas = CreateCanvas();
            RectTransform canvasRect = (RectTransform)canvas.transform;
            CreateBackground(canvasRect);

            GameObject rootObject = new GameObject("GameRoot");
            GameRoot root = rootObject.AddComponent<GameRoot>();
            ScreenEntry[] entries = BuildAllScreens(canvasRect, root, audio, out MiniGameRefs refs);

            ScreenNavigator navigator = rootObject.AddComponent<ScreenNavigator>();
            WireScreens(navigator, entries);
            Wire(root, "_navigator", navigator);
            Wire(root, "_audio", audio);
            Wire(root, "_loading", refs.Loading);
            Wire(root, "_map", refs.Map);
            Wire(root, "_details", refs.Details);
            Wire(root, "_settings", refs.Settings);
            Wire(root, "_victory", refs.Victory);
            Wire(root, "_defeat", refs.Defeat);
            Wire(root, "_games", refs.Games);

            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Project/Scenes"));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            ConfigurePlayerSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("Labyrinthino scene built: " + ScenePath);
        }

        private static void RemoveTemplateAssets()
        {
            foreach (string path in new[] { "Assets/Readme.asset", "Assets/TutorialInfo", "Assets/Scenes" })
            {
                if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        private static void ConfigureImports()
        {
            Dictionary<string, Vector4> borders = new Dictionary<string, Vector4>
            {
                { "ui_panel", new Vector4(44, 44, 44, 44) },
                { "ui_card", new Vector4(44, 44, 44, 44) },
                { "ui_button", new Vector4(44, 56, 44, 44) },
                { "ui_cell", new Vector4(26, 26, 26, 26) },
                { "ui_bar", new Vector4(22, 22, 22, 22) }
            };

            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ProjectRoot + "/Art" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
                {
                    continue;
                }

                string name = Path.GetFileNameWithoutExtension(path);
                if (name == "app_icon")
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.maxTextureSize = 512;
                }
                else
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.maxTextureSize = 2048;
                    importer.spriteBorder = borders.TryGetValue(name, out Vector4 border) ? border : Vector4.zero;
                }

                importer.SaveAndReimport();
            }
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.productName = "Labyrinthino";
            PlayerSettings.companyName = "Labyrinthino Studio";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.labyrinthino.game");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.defaultScreenWidth = 540;
            PlayerSettings.defaultScreenHeight = 960;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(ProjectRoot + "/Art/Branding/app_icon.png");
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Application);

            Sprite logo = LoadSprite("Branding/logo_labyrinthino");
            PlayerSettings.SplashScreen.logos = new[] { PlayerSettings.SplashScreenLogo.Create(2f, logo) };
            PlayerSettings.SplashScreen.drawMode = PlayerSettings.SplashScreen.DrawMode.AllSequential;
            PlayerSettings.SplashScreen.backgroundColor = new Color32(30, 16, 70, 255);
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(30, 16, 70, 255);
            camera.orthographic = true;
            cameraObject.AddComponent<AudioListener>();
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            InputSystemUIInputModule module = eventSystem.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        private static AudioController CreateAudio()
        {
            GameObject audioObject = new GameObject("Audio");
            AudioController controller = audioObject.AddComponent<AudioController>();
            AudioSource music = NewSource("Music", audioObject.transform);
            AudioSource sound = NewSource("Sound", audioObject.transform);
            string[] soundNames = { "click", "tap", "correct", "error", "victory", "defeat", "step" };
            AudioClip[] clips = new AudioClip[soundNames.Length];
            for (int i = 0; i < clips.Length; i++)
            {
                clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(ProjectRoot + "/Audio/Sfx/sfx_" + soundNames[i] + ".wav");
            }

            Wire(controller, "_musicSource", music);
            Wire(controller, "_soundSource", sound);
            Wire(controller, "_musicClip", AssetDatabase.LoadAssetAtPath<AudioClip>(ProjectRoot + "/Audio/Music/music_main_theme.wav"));
            Wire(controller, "_soundClips", clips);
            return controller;
        }

        private static AudioSource NewSource(string name, Transform parent)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform));
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ContentWidth, ContentHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void CreateBackground(RectTransform canvas)
        {
            RectTransform rect = NewRect("Background", canvas);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(ContentWidth, ContentHeight);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = LoadSprite("Sprites/Backgrounds/bg_main");
            image.raycastTarget = false;
            AspectRatioFitter fitter = rect.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = ContentWidth / ContentHeight;
        }

        private static void WireScreens(ScreenNavigator navigator, ScreenEntry[] entries)
        {
            SerializedObject serialized = new SerializedObject(navigator);
            SerializedProperty list = serialized.FindProperty("_screens");
            list.arraySize = entries.Length;
            for (int i = 0; i < entries.Length; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("Id").enumValueIndex = (int)entries[i].Id;
                element.FindPropertyRelative("Group").objectReferenceValue = entries[i].Group;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite LoadSprite(string relativePath)
        {
            if (SpriteCache.TryGetValue(relativePath, out Sprite cached))
            {
                return cached;
            }

            string path = ProjectRoot + "/Art/" + relativePath + ".png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new FileNotFoundException("Missing sprite " + path);
            }

            SpriteCache[relativePath] = sprite;
            return sprite;
        }

        private static Sprite Ui(string name)
        {
            return LoadSprite("Sprites/UI/" + name);
        }

        private static Sprite Icon(string name)
        {
            return LoadSprite("Sprites/Icons/" + name);
        }

        private static void Wire(Object target, string field, object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
            {
                throw new MissingFieldException(target.GetType().Name + "." + field);
            }

            switch (value)
            {
                case Object unityObject:
                    property.objectReferenceValue = unityObject;
                    break;
                case float number:
                    property.floatValue = number;
                    break;
                case bool flag:
                    property.boolValue = flag;
                    break;
                case IList list:
                    property.arraySize = list.Count;
                    for (int i = 0; i < list.Count; i++)
                    {
                        property.GetArrayElementAtIndex(i).objectReferenceValue = (Object)list[i];
                    }

                    break;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
