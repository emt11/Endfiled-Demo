using System;
using System.Collections.Generic;
using UnityEngine;

namespace GP
{
    [Serializable]
    public abstract class GPDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {

        // ⭐ 核心改动：List 作为唯一持久化数据源
        [SerializeField]
        private List<KeyValuePair> m_SerializedData = new();

        // ==================== 序列化回调 ====================

        public void OnBeforeSerialize() // 保存数据，从字典同步到 List
        {
            // ⭐ 移除 Application.isPlaying 判断
            // 始终以内部 Dictionary 为准同步到 List（编辑器修改或运行时修改都能正确保存）
            m_SerializedData.Clear();
            foreach (var kvp in this)
            {
                m_SerializedData.Add(new KeyValuePair(kvp.Key, kvp.Value));
            }
        }

        public void OnAfterDeserialize() // 读取数据，从 List 同步到字典
        {
            Clear();

            // ⭐ 零 GC 的遍历方式，替代 LINQ
            for (int i = 0; i < m_SerializedData.Count; i++)
            {
                var kvp = m_SerializedData[i];

                if (kvp.Key == null) continue;

                // ⭐ 用 TryAdd 替代 try-catch，避免异常开销
                if (!TryAdd(kvp.Key, kvp.Value))
                {
                    Debug.LogWarning($"[SerializableDict] 检测到重复键: {kvp.Key}，已跳过后续项。");
                }
            }
        }
        [Serializable]
        private struct KeyValuePair
        {
            public TKey Key;
            public TValue Value;

            public KeyValuePair(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

    }
}
