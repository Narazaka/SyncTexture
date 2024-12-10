using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.Udon;

namespace net.narazaka.vrchat.sync_texture
{
    [DisallowMultipleComponent]
    public abstract class SyncTextureData : UdonSharpBehaviour
    {
        int RetryCount = 0;
        const int MaxRetryCount = 3;
        const float RetryInterval = 4f;
        bool OnceApplied;

        protected abstract bool DataIsNull { get; }
        protected abstract bool DataIsEmpty { get; }
        protected abstract void ApplyReceiveData();

        public override void OnDeserialization()
        {
            TryApplyReceiveDataFromOnDeserialization();
        }

        void Start()
        {
            SendCustomEventDelayedSeconds(nameof(TryApplyReceiveDataFromStart), 3f);
            SendCustomEventDelayedSeconds(nameof(TryApplyReceiveDataFromStart), 6f);
            SendCustomEventDelayedSeconds(nameof(TryApplyReceiveDataFromStart), 9f);
            SendCustomEventDelayedSeconds(nameof(TryApplyReceiveDataFromStart), 12f);
        }

        public void TryApplyReceiveDataFromOnDeserialization()
        {
            if (DataIsNull)
            {
                RetryCount++;
                if (RetryCount <= MaxRetryCount)
                {
                    Debug.LogWarning($"{LogPrefix} Data is deserialized but is null or empty. Retrying...");
                    SendCustomEventDelayedSeconds(nameof(TryApplyReceiveDataFromOnDeserialization), RetryInterval);
                }
                else
                {
                    Debug.LogError($"{LogPrefix} Data is deserialized but is null or empty. Retry limit exceeded.");
                }
                return;
            }
            TryApplyReceiveData();
        }

        public void TryApplyReceiveDataFromStart()
        {
            if (DataIsNull)
            {
                Debug.Log($"{LogPrefix} Trying to get data but not received.");
                return;
            }
            if (OnceApplied)
            {
                return;
            }
            Debug.LogWarning($"{LogPrefix} Trying to get data and found non null data!");
            TryApplyReceiveData();
        }

        public void TryApplyReceiveData()
        {
            if (DataIsEmpty)
            {
                Debug.LogWarning($"{LogPrefix} Data is deserialized but is empty.");
            }
            else
            {
                OnceApplied = true;
                ApplyReceiveData();
            }
        }

        string LogPrefix => $"[SyncTextureData] ({name})";
    }
}
