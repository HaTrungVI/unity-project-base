using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectBase.UI.Core
{
    [CreateAssetMenu(fileName = "UIRegistry", menuName = "ProjectBase/UI Registry")]
    public class UIRegistry : ScriptableObject
    {
        [System.Serializable]
        public class UIEntry
        {
            public string id;
            public AssetReference assetReference;
            public UIEntryType type;
            public bool preload;
        }

        public enum UIEntryType { Screen, Popup, Overlay }

        [SerializeField] private List<UIEntry> _entries = new();

        private Dictionary<string, UIEntry> _lookup;

        public void Initialize()
        {
            _lookup = new Dictionary<string, UIEntry>();
            foreach (var entry in _entries)
                _lookup[entry.id] = entry;
        }

        public bool TryGetEntry(string id, out UIEntry entry)
        {
            if (_lookup == null) Initialize();
            return _lookup.TryGetValue(id, out entry);
        }

        public List<UIEntry> GetAllEntries() => _entries;

#if UNITY_EDITOR
        public void AddEntry(UIEntry entry)
        {
            _entries.Add(entry);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void RemoveEntry(string id)
        {
            _entries.RemoveAll(e => e.id == id);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
