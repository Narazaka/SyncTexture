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
        SerializedProperty ReceiveEnabled;
        SerializedProperty Source;
        SerializedProperty Target;
        SerializedProperty ColorEncoder;
        SerializedProperty GetPixelsBulkCount;
        SerializedProperty BulkLineCount;
        SerializedProperty BulkRateOfNetworkSpec;
        SerializedProperty SyncInterval;
        SerializedProperty CallbackListeners;
        SerializedProperty PrepareCallbackAsync;
        SerializedProperty DataList;
        bool ShowColorEncoders;
        bool ShowCalllbackHelp;

        void OnEnable()
        {
            ReceiveEnabled = serializedObject.FindProperty("ReceiveEnabled");
            Source = serializedObject.FindProperty("Source");
            Target = serializedObject.FindProperty("Target");
            ColorEncoder = serializedObject.FindProperty("ColorEncoder");
            GetPixelsBulkCount = serializedObject.FindProperty("GetPixelsBulkCount");
            BulkLineCount = serializedObject.FindProperty("BulkLineCount");
            BulkRateOfNetworkSpec = serializedObject.FindProperty("BulkRateOfNetworkSpec");
            SyncInterval = serializedObject.FindProperty("SyncInterval");
            CallbackListeners = serializedObject.FindProperty("CallbackListeners");
            PrepareCallbackAsync = serializedObject.FindProperty("PrepareCallbackAsync");
            DataList = serializedObject.FindProperty("DataList");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.Update();
            EditorGUILayout.PropertyField(ReceiveEnabled);
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
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing));
            using (new EditorGUI.PropertyScope(rect, GUIContent.none, BulkLineCount))
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var bulkLineCount = EditorGUI.ToggleLeft(rect, "Calc BulkLineCount by network spec per second", BulkLineCount.intValue == 0);
                    if (check.changed)
                    {
                        BulkLineCount.intValue = bulkLineCount ? 0 : 10;
                    }
                }
                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var bulkLineCount = EditorGUI.ToggleLeft(rect, "Calc BulkLineCount by network spec per serialization", BulkLineCount.intValue == -1);
                    if (check.changed)
                    {
                        BulkLineCount.intValue = bulkLineCount ? -1 : 10;
                    }
                }
            }
            if (BulkLineCount.intValue == 0 || BulkLineCount.intValue == -1)
            {
                EditorGUILayout.PropertyField(BulkRateOfNetworkSpec);
            }
            else
            {
                EditorGUILayout.PropertyField(BulkLineCount);
            }
            var stat = GetSyncTextureTypeStat(serializedObject, Source: Source, ColorEncoder: ColorEncoder, BulkLineCount: BulkLineCount, BulkRateOfNetworkSpec: BulkRateOfNetworkSpec);
            if (stat != null)
            {
                EditorGUILayout.HelpBox($"{stat.ChunkCount} steps : {stat.BulkByteCount} bytes/step.", MessageType.Info);
                EditorGUILayout.HelpBox($"{stat.DataLimitRatePerSecond * 100} % of network spec per second", stat.DataLimitRatePerSecond < 1 ? MessageType.Info : MessageType.Warning);
                EditorGUILayout.HelpBox($"{stat.DataLimitRatePerSerialization * 100} % of network spec per serialization", stat.DataLimitRatePerSerialization < 1 ? MessageType.Info : MessageType.Error);
            }
            if (BulkLineCount.intValue < -1)
            {
                EditorGUILayout.HelpBox("BulkCount must be 0, -1 or a positive integer", MessageType.Error);
            }
            EditorGUILayout.PropertyField(SyncInterval);
            if (SyncInterval.floatValue < 0f)
            {
                EditorGUILayout.HelpBox("SyncInterval must be positive", MessageType.Error);
            }
            EditorGUILayout.PropertyField(DataList, true);
            if (stat.ChunkCount > 0)
            {
                SetDataList(DataList, stat.ChunkCount);
            }
            EditorGUILayout.PropertyField(CallbackListeners);
            EditorGUILayout.PropertyField(PrepareCallbackAsync);
            ShowCalllbackHelp = EditorGUILayout.Foldout(ShowCalllbackHelp, "Callback Help");
            if (ShowCalllbackHelp)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.HelpBox("CallbackListener callbacks:", MessageType.Info);
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnPreSync));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnPrepare));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnPrepareCancel));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnSyncStart));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnSync));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnSyncComplete));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnSyncCanceled));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnReceiveStart));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnReceive));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnReceiveComplete));
                    EditorGUILayout.TextField(nameof(SyncTextureCallbackListener.OnReceiveCanceled));
                    EditorGUILayout.HelpBox("async CallbackListener preparing should call this:", MessageType.Info);
                    EditorGUILayout.TextField(nameof(SyncTexture.OnPrepared));
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        public static void SetDataList(SerializedProperty DataList, int chunkCount)
        {
            var syncTexture2D = DataList.serializedObject.targetObject as SyncTexture2D;
            var type = syncTexture2D.UnitByteLength switch
            {
                1 => typeof(SyncTextureData8),
                2 => typeof(SyncTextureData16),
                _ => null,
            };

            var count = 0;
            var len = DataList.arraySize;
            var uniq = new HashSet<SyncTextureData>();
            for (var i = len - 1; i >= 0; i--)
            {
                var element = DataList.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue == null)
                {
                    DataList.DeleteArrayElementAtIndex(i);
                    continue;
                }
                var data = element.objectReferenceValue as SyncTextureData;
                if (data.GetType() != type)
                {
                    Undo.DestroyObjectImmediate(data.gameObject);
                    DataList.DeleteArrayElementAtIndex(i);
                    continue;
                }
                if (data.transform.parent != syncTexture2D.transform)
                {
                    Undo.RecordObject(data.transform, "Reparent SyncTextureData");
                    data.transform.SetParent(syncTexture2D.transform, false);
                }
                if (!uniq.Add(data))
                {
                    DataList.DeleteArrayElementAtIndex(i);
                    continue;
                }
                count++;
            }
            if (count < chunkCount)
            {
                for (var i = count; i < chunkCount; i++)
                {
                    DataList.InsertArrayElementAtIndex(i);
                    var dataGo = new GameObject($"SyncTextureData{i}");
                    dataGo.transform.SetParent(syncTexture2D.transform, false);
                    var data = dataGo.AddComponent(type) as SyncTextureData;
                    Undo.RegisterCreatedObjectUndo(dataGo, "Create SyncTextureData");
                    DataList.GetArrayElementAtIndex(i).objectReferenceValue = data;
                }
            }
            else if (count > chunkCount)
            {
                for (var i = count - 1; i >= chunkCount; i--)
                {
                    var element = DataList.GetArrayElementAtIndex(i);
                    Undo.DestroyObjectImmediate((element.objectReferenceValue as SyncTextureData).gameObject);
                    DataList.DeleteArrayElementAtIndex(i);
                }
            }
            len = DataList.arraySize;
            var allDataList = new List<SyncTextureData>();
            for (var i = 0; i < len; ++i)
            {
                var element = DataList.GetArrayElementAtIndex(i).objectReferenceValue as SyncTextureData;
                var index = element.transform.GetSiblingIndex();
                if (index != i)
                {
                    Undo.RecordObject(element.transform, "Reorder SyncTextureData");
                    element.transform.SetSiblingIndex(i);
                }
                var data = new SerializedObject(element);
                data.Update();
                data.FindProperty("SyncTexture2D").objectReferenceValue = syncTexture2D;
                data.ApplyModifiedProperties();
                allDataList.Add(element);
            }
            var toDestroies = new List<SyncTextureData>();
            foreach (Transform child in syncTexture2D.transform)
            {
                var data = child.GetComponent<SyncTextureData>();
                if (data == null) continue;
                if (!allDataList.Contains(data))
                {
                    toDestroies.Add(data);
                }
            }
            foreach (var data in toDestroies)
            {
                Undo.DestroyObjectImmediate(data.gameObject);
            }
        }

        public class SyncTextureTypeStat
        {
            public int UnitByteLength;
            public int PackUnitLength;
            public int EffectiveBulkLineCount;
            public int BulkPixelCount;
            public int BulkUnitCount;
            public int BulkByteCount;
            public float DataLimitRatePerSerialization;
            public float DataLimitRatePerSecond;
            public int ChunkCount;
        }

        public static SyncTextureTypeStat GetSyncTextureTypeStat(SerializedObject serializedObject, SerializedProperty Source = null, SerializedProperty ColorEncoder = null, SerializedProperty BulkLineCount = null, SerializedProperty BulkRateOfNetworkSpec = null)
        {
            if (Source == null) Source = serializedObject.FindProperty("Source");
            if (ColorEncoder == null) ColorEncoder = serializedObject.FindProperty("ColorEncoder");
            if (BulkLineCount == null) BulkLineCount = serializedObject.FindProperty("BulkLineCount");
            if (BulkRateOfNetworkSpec == null) BulkRateOfNetworkSpec = serializedObject.FindProperty("BulkRateOfNetworkSpec");

            var stat = new SyncTextureTypeStat();
            if (ColorEncoder.objectReferenceValue == null || Source.objectReferenceValue == null)
            {
                return stat;
            }
            var colorEncoder = ColorEncoder.objectReferenceValue;
            switch (colorEncoder)
            {
                case ColorEncoder8 c:
                    stat.PackUnitLength = c.PackUnitLength;
                    stat.UnitByteLength = 1;
                    break;
                case ColorEncoder16 c:
                    stat.PackUnitLength = c.PackUnitLength;
                    stat.UnitByteLength = 2;
                    break;
            }
            var sourceTexture = (Texture)Source.objectReferenceValue;
            var width = sourceTexture.width;
            stat.EffectiveBulkLineCount = SyncTexture.GetEffectiveBulkLineCount(BulkLineCount.intValue, BulkRateOfNetworkSpec.floatValue, width, stat.UnitByteLength, stat.PackUnitLength);
            stat.BulkPixelCount = stat.EffectiveBulkLineCount * width;
            stat.BulkUnitCount = stat.BulkPixelCount * stat.PackUnitLength;
            stat.BulkByteCount = stat.BulkUnitCount * stat.UnitByteLength;
            stat.DataLimitRatePerSerialization = (float)stat.BulkByteCount / SyncTexture.MaxBulkBytesPerSerialization;
            stat.DataLimitRatePerSecond = (float)stat.BulkByteCount / SyncTexture.MaxBulkBytesPerSecond;
            stat.ChunkCount = Mathf.CeilToInt((float)sourceTexture.height / stat.EffectiveBulkLineCount);
            return stat;
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
