using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRC.Udon;

namespace net.narazaka.vrchat.sync_texture.editor
{
    [CustomEditor(typeof(SyncTextureManager))]
    public class SyncTextureManagerEditor : Editor
    {
        UdonBehaviour TargetUdonBehaviour;
        SerializedProperty SyncTextures;
        ReorderableList SyncTexturesList;

        void OnEnable()
        {
            TargetUdonBehaviour = (target as UdonSharpBehaviour).GetComponent<UdonBehaviour>();
            SyncTextures = serializedObject.FindProperty("SyncTextures");
            SyncTexturesList = new ReorderableList(serializedObject, SyncTextures, true, true, true, true);
            SyncTexturesList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "SyncTextures");
            };
            SyncTexturesList.elementHeight = EditorGUIUtility.singleLineHeight;
            SyncTexturesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = SyncTextures.GetArrayElementAtIndex(index);

                EditorGUI.PropertyField(rect, element);

                var syncTexture = element.objectReferenceValue as SyncTexture;
                if (syncTexture != null && syncTexture.CallbackListener != TargetUdonBehaviour)
                {
                    Undo.RecordObject(syncTexture, "set CallbackListener by SyncTextureManagerEditor");
                    syncTexture.CallbackListener = TargetUdonBehaviour;
                }
            };
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.Update();

            SyncTexturesList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
