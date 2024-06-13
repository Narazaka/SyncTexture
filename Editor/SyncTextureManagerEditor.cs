using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
                SetSelfToSyncTexture(syncTexture);
            };
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.Update();

            SyncTexturesList.DoLayoutList();

            if (GUILayout.Button("Set All SyncTextures"))
            {
                var storedSyncTextures = (target as SyncTextureManager).SyncTextures.ToImmutableHashSet();
                var syncTextures = UnityEngine.Object.FindObjectsByType<SyncTexture>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
                foreach (var syncTexture in syncTextures)
                {
                    if (storedSyncTextures.Contains(syncTexture)) continue;
                    SyncTextures.arraySize++;
                    SyncTextures.GetArrayElementAtIndex(SyncTextures.arraySize - 1).objectReferenceValue = syncTexture;
                    SetSelfToSyncTexture(syncTexture);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void SetSelfToSyncTexture(SyncTextureBase syncTexture)
        {
            if (syncTexture == null) return;
            if (syncTexture.CallbackListeners != null && syncTexture.CallbackListeners.Contains(TargetUdonBehaviour)) return;
            Undo.RecordObject(syncTexture, "set CallbackListener by SyncTextureManagerEditor");
            var len = syncTexture.CallbackListeners == null ? 0 : syncTexture.CallbackListeners.Length;
            var newCallbackListeners = new UdonBehaviour[len + 1];
            if (len > 0) Array.Copy(syncTexture.CallbackListeners, newCallbackListeners, len);
            newCallbackListeners[len] = TargetUdonBehaviour;
            syncTexture.CallbackListeners = newCallbackListeners;
        }
    }
}
