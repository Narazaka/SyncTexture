using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UdonSharpEditor;
using System.Linq;

namespace net.narazaka.vrchat.sync_texture.editor
{
    [CustomEditor(typeof(SyncTexture2D16))]
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
        SerializedProperty SendFormat;
        SerializedProperty GetPixelsBulkCount;
        SerializedProperty BulkCount;
        SerializedProperty SyncInterval;
        SerializedProperty ShowProgress;
        SerializedProperty CallbackListener;
        SerializedProperty PrepareCallbackListener;
        SerializedProperty PrepareCallbackAsync;
        SerializedProperty SyncEnabled;
        bool ShowCalllbackHelp;

        void OnEnable()
        {
            Source = serializedObject.FindProperty("Source");
            Target = serializedObject.FindProperty("Target");
            SendFormat = serializedObject.FindProperty("SendFormat");
            GetPixelsBulkCount = serializedObject.FindProperty("GetPixelsBulkCount");
            BulkCount = serializedObject.FindProperty("BulkCount");
            SyncInterval = serializedObject.FindProperty("SyncInterval");
            ShowProgress = serializedObject.FindProperty("ShowProgress");
            CallbackListener = serializedObject.FindProperty("CallbackListener");
            PrepareCallbackListener = serializedObject.FindProperty("PrepareCallbackListener");
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
                var source = (Texture2D)Source.objectReferenceValue;
                var target = (Texture2D)Target.objectReferenceValue;
                if (source.width != target.width || source.height != target.height)
                {
                    EditorGUILayout.HelpBox("Source and Target must be same size", MessageType.Error);
                }
            }
            CheckTexture2DReadable(Source);
            CheckTexture2DReadable(Target);
            CheckTexture2DWritable(Target);
            EditorGUILayout.PropertyField(SendFormat);
            EditorGUILayout.PropertyField(GetPixelsBulkCount);
            EditorGUILayout.HelpBox("GetPixelsBulkCount affects sender performance", MessageType.Info);
            EditorGUILayout.PropertyField(BulkCount);
            EditorGUILayout.PropertyField(SyncInterval);
            if (SyncInterval.floatValue < 0f)
            {
                EditorGUILayout.HelpBox("SyncInterval must be positive", MessageType.Error);
            }
            if (SyncInterval.floatValue * 5000f < BulkCount.intValue)
            {
                EditorGUILayout.HelpBox("SyncInterval is short", MessageType.Warning);
            }
            EditorGUILayout.PropertyField(ShowProgress);
            EditorGUILayout.PropertyField(CallbackListener);
            EditorGUILayout.PropertyField(PrepareCallbackListener);
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
                    EditorGUILayout.TextField(nameof(SyncTexture2D16.OnPrepared));
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
            var texture = (Texture2D)property.objectReferenceValue;
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
