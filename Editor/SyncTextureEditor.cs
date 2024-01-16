using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UdonSharpEditor;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace net.narazaka.vrchat.sync_texture.editor
{
    [CustomEditor(typeof(SyncTexture2D), true)]
    [CanEditMultipleObjects]
    public class SyncTextureEditor : Editor
    {
        // cf. https://docs.unity3d.com/ScriptReference/Texture2D.SetPixel.html
        static List<TextureFormat> AllowedFormats = new List<TextureFormat>
        {
            TextureFormat.Alpha8,
            TextureFormat.ARGB32,
            TextureFormat.ARGB4444,
            TextureFormat.BGRA32,
            TextureFormat.R16,
            TextureFormat.R8,
            TextureFormat.RFloat,
            TextureFormat.RG16,
            TextureFormat.RG32,
            TextureFormat.RGB24,
            TextureFormat.RGB48,
            TextureFormat.RGB565,
            TextureFormat.RGB9e5Float,
            TextureFormat.RGBA32,
            TextureFormat.RGBA4444,
            TextureFormat.RGBA64,
            TextureFormat.RGBAFloat,
            TextureFormat.RGBAHalf,
            TextureFormat.RGFloat,
            TextureFormat.RGHalf,
            TextureFormat.RHalf,
        };
        SerializedProperty Source;
        SerializedProperty Target;
        SerializedProperty ColorEncoder;
        SerializedProperty GetPixelsBulkCount;
        SerializedProperty BulkCount;
        SerializedProperty SyncInterval;
        SerializedProperty ShowProgress;
        SerializedProperty CallbackListeners;
        SerializedProperty PrepareCallbackAsync;
        SerializedProperty SyncEnabled;
        bool ShowColorEncoders;
        bool ShowCalllbackHelp;

        void OnEnable()
        {
            Source = serializedObject.FindProperty("Source");
            Target = serializedObject.FindProperty("Target");
            ColorEncoder = serializedObject.FindProperty("ColorEncoder");
            GetPixelsBulkCount = serializedObject.FindProperty("GetPixelsBulkCount");
            BulkCount = serializedObject.FindProperty("BulkCount");
            SyncInterval = serializedObject.FindProperty("SyncInterval");
            ShowProgress = serializedObject.FindProperty("ShowProgress");
            CallbackListeners = serializedObject.FindProperty("CallbackListeners");
            PrepareCallbackAsync = serializedObject.FindProperty("PrepareCallbackAsync");
            SyncEnabled = serializedObject.FindProperty("SyncEnabled");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.Update();
            EditorGUILayout.PropertyField(Source);
            EditorGUILayout.PropertyField(Target);
            if (Source.objectReferenceValue == null || Target.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Source and Target must be set", MessageType.Error);
            }
            if (Source.objectReferenceValue != null && Target.objectReferenceValue != null)
            {
                var source = (Texture)Source.objectReferenceValue;
                var target = (Texture2D)Target.objectReferenceValue;
                if (source.width != target.width || source.height != target.height)
                {
                    EditorGUILayout.HelpBox("Source and Target must be same size", MessageType.Error);
                }
            }
            CheckTexture2DReadable(Source);
            CheckTexture2DReadable(Target);
            CheckTexture2DWritable(Target);
            EditorGUILayout.PropertyField(ColorEncoder);
            if (ShowColorEncoders = EditorGUILayout.Foldout(ShowColorEncoders, $"Select {ColorEncoder.displayName}"))
            {
                var targetObject = serializedObject.targetObject;
                var field = targetObject.GetType().GetField("ColorEncoder");
                if (field != null)
                {
                    Assembly.GetAssembly(field.DeclaringType).GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(field.FieldType)).ToList().ForEach(t =>
                    {
                        if (GUILayout.Button(t.Name))
                        {
                            var existObject = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(o => o.GetComponentInChildren(t));
                            if (existObject == null)
                            {
                                var obj = new GameObject(t.Name);
                                obj.AddComponent(t);
                                ColorEncoder.objectReferenceValue = obj.GetComponent(t);
                            }
                            else
                            {
                                ColorEncoder.objectReferenceValue = existObject.GetComponent(t);
                            }
                        }
                    });
                }
            }
            if (ColorEncoder.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("ColorEncoder must be set", MessageType.Error);
            }
            bool useAsyncGPUReadback;
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                useAsyncGPUReadback = EditorGUILayout.Toggle("Use AsyncGPUReadback", GetPixelsBulkCount.intValue == 0);
                if (change.changed)
                {
                    GetPixelsBulkCount.intValue = useAsyncGPUReadback ? 0 : 8;
                }
            }
            if (!useAsyncGPUReadback && Source.objectReferenceValue != null && Source.objectReferenceValue is RenderTexture)
            {
                EditorGUILayout.HelpBox("RenderTexture source must use AsyncGPUReadback", MessageType.Error);
            }
            if (useAsyncGPUReadback)
            {
                EditorGUILayout.HelpBox("AsyncGPUReadback can use raw texture formats such as RGBA32bit so Source texture format should not be a compressed format such as DXT1", MessageType.Info);
            }
            if (!useAsyncGPUReadback)
            {
                EditorGUILayout.PropertyField(GetPixelsBulkCount);
                EditorGUILayout.HelpBox("GetPixelsBulkCount affects sender performance", MessageType.Info);
            }
            EditorGUILayout.PropertyField(BulkCount);
            if (ColorEncoder.objectReferenceValue != null)
            {
                var colorEncoder = ColorEncoder.objectReferenceValue;
                int packUnitLength = 1;
                int bytes = 0;
                switch (colorEncoder)
                {
                    case ColorEncoder8 c:
                        packUnitLength = c.PackUnitLength;
                        bytes = 1;
                        break;
                    case ColorEncoder16 c:
                        packUnitLength = c.PackUnitLength;
                        bytes = 2;
                        break;
                }

                if (BulkCount.intValue % packUnitLength != 0)
                {
                    EditorGUILayout.HelpBox($"BulkCount must be multiple of {packUnitLength}", MessageType.Error);
                }
                var SyncBytesPerSecond = BulkCount.intValue * bytes / SyncInterval.floatValue;
                var specRate = SyncBytesPerSecond / (11f * 1024);
                EditorGUILayout.HelpBox($"sync {SyncBytesPerSecond} bytes/sec : {specRate * 100} % of network spec", MessageType.Info);
                if (Source.objectReferenceValue != null)
                {
                    var sourceTexture = (Texture)Source.objectReferenceValue;
                    var pixelCount = sourceTexture.width * sourceTexture.height;
                    var bulkPixelCount = BulkCount.intValue / packUnitLength;
                    var seconds = SyncInterval.floatValue * pixelCount / bulkPixelCount;
                    EditorGUILayout.HelpBox($"total sync time will be {seconds} seconds", MessageType.Info);
                }
                if (specRate > 0.7f)
                {
                    EditorGUILayout.HelpBox("sync size is too big! reduce BulkCount or increase SyncInterval", MessageType.Warning);
                }
            }
            if (BulkCount.intValue < 1)
            {
                EditorGUILayout.HelpBox("BulkCount must be positive", MessageType.Error);
            }
            EditorGUILayout.PropertyField(SyncInterval);
            if (SyncInterval.floatValue < 0f)
            {
                EditorGUILayout.HelpBox("SyncInterval must be positive", MessageType.Error);
            }
            EditorGUILayout.PropertyField(ShowProgress);
            EditorGUILayout.PropertyField(CallbackListeners);
            EditorGUILayout.PropertyField(PrepareCallbackAsync);
            ShowCalllbackHelp = EditorGUILayout.Foldout(ShowCalllbackHelp, "Callback Help");
            if (ShowCalllbackHelp)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.HelpBox("CallbackListener callbacks:", MessageType.Info);
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnPreSync));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnSyncStart));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnSync));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnSyncComplete));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnSyncCanceled));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnReceiveStart));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnReceive));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnReceiveComplete));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnReceiveCanceled));
                    EditorGUILayout.HelpBox("PrepareCallbackListener callbacks:", MessageType.Info);
                    EditorGUILayout.TextField(nameof(SyncTexturePrepareCallbackListener.OnPrepare));
                    EditorGUILayout.TextField(nameof(SyncTexturePrepareCallbackListener.OnPrepareCancel));
                    EditorGUILayout.HelpBox("async PrepareCallbackListener should call this:", MessageType.Info);
                    EditorGUILayout.TextField(nameof(SyncTexture.OnPrepared));
                }
            }
            EditorGUILayout.PropertyField(SyncEnabled);
            if (!SyncEnabled.boolValue)
            {
                EditorGUILayout.HelpBox("SyncEnabled is false. Set true at runtime.", MessageType.Warning);
            }
            serializedObject.ApplyModifiedProperties();
        }

        void CheckTexture2DReadable(SerializedProperty property)
        {
            if (property.objectReferenceValue == null) return;
            var texture = (Texture)property.objectReferenceValue;
            if (!texture.isReadable)
            {
                EditorGUILayout.HelpBox($"{property.displayName} must be Read/Write Enabled", MessageType.Error);
            }
        }

        void CheckTexture2DWritable(SerializedProperty property)
        {
            if (property.objectReferenceValue == null) return;
            var texture = (Texture2D)property.objectReferenceValue;
            if (!AllowedFormats.Contains(texture.format))
            {
                EditorGUILayout.HelpBox($"{property.displayName} format is not supported", MessageType.Error);
                EditorGUILayout.HelpBox($"Supported formats are " + string.Join(", ", AllowedFormats), MessageType.Info);
            }
        }
    }
}
