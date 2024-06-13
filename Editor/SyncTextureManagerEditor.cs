using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            TargetUdonBehaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(target as SyncTextureManager);
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

                var syncTexture = element.objectReferenceValue as SyncTextureBase;
                if (syncTexture != null && (syncTexture.CallbackListeners == null || !syncTexture.CallbackListeners.Contains(TargetUdonBehaviour)))
                {
                    Undo.RecordObject(syncTexture, "set CallbackListener by SyncTextureManagerEditor");
                    var len = syncTexture.CallbackListeners == null ? 0 : syncTexture.CallbackListeners.Length;
                    var newCallbackListeners = new UdonBehaviour[len + 1];
                    if (len > 0) Array.Copy(syncTexture.CallbackListeners, newCallbackListeners, len);
                    newCallbackListeners[len] = TargetUdonBehaviour;
                    syncTexture.CallbackListeners = newCallbackListeners;
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
