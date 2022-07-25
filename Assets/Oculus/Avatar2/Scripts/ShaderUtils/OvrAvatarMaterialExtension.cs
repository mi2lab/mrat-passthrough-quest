using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Oculus.Avatar2
{
    public class OvrAvatarMaterialExtension
    {
        //////////////////////////////////////////////////
        // ExtenstionEntry<T>
        //////////////////////////////////////////////////
        private class ExtensionEntry<T>
        {
            private string _name;
            private T _payload;

            public ExtensionEntry(string name, T payload)
            {
                _name = name;
                _payload = payload;
            }

            public string Name => _name;
            public T Payload => _payload;
        }

        //////////////////////////////////////////////////
        // ExtenstionEntries
        //////////////////////////////////////////////////
        private class ExtensionEntries
        {
            private static readonly string LOG_SCOPE =
                $"{nameof(OvrAvatarMaterialExtension)}_{nameof(ExtensionEntries)}";

            private readonly List<ExtensionEntry<Vector3>> _vector3Entries = new List<ExtensionEntry<Vector3>>();
            private readonly List<ExtensionEntry<Vector4>> _vector4Entries = new List<ExtensionEntry<Vector4>>();
            private readonly List<ExtensionEntry<float>> _floatEntries = new List<ExtensionEntry<float>>();
            private readonly List<ExtensionEntry<int>> _intEntries = new List<ExtensionEntry<int>>();
            private readonly List<ExtensionEntry<Texture2D>> _textureEntries = new List<ExtensionEntry<Texture2D>>();

            public void ApplyToMaterial(Material mat, string extensionName, OvrAvatarMaterialExtensionConfig extensionConfig)
            {
                Debug.Assert(extensionConfig != null);

                string nameInShader;
                foreach (var entry in _vector3Entries)
                {
                    if (extensionConfig.TryGetNameInShader(extensionName, entry.Name, out nameInShader))
                    {
                        mat.SetVector(nameInShader, entry.Payload);
                    }
                }

                foreach (var entry in _vector4Entries)
                {
                    if (extensionConfig.TryGetNameInShader(extensionName, entry.Name, out nameInShader))
                    {
                        mat.SetVector(nameInShader, entry.Payload);
                    }
                }

                foreach (var entry in _floatEntries)
                {
                    if (extensionConfig.TryGetNameInShader(extensionName, entry.Name, out nameInShader))
                    {
                        mat.SetFloat(nameInShader, entry.Payload);
                    }
                }

                foreach (var entry in _intEntries)
                {
                    if (extensionConfig.TryGetNameInShader(extensionName, entry.Name, out nameInShader))
                    {
                        mat.SetInt(nameInShader, entry.Payload);
                    }
                }

                foreach (var entry in _textureEntries)
                {
                    if (extensionConfig.TryGetNameInShader(extensionName, entry.Name, out nameInShader))
                    {
                        mat.SetTexture(nameInShader, entry.Payload);
                    }
                }
            }

            public bool LoadEntry(CAPI.ovrAvatar2Id primitiveId, UInt32 extensionIndex, UInt32 entryIndex)
            {
                if (!GetEntryMetaData(primitiveId, extensionIndex, entryIndex,
                        out CAPI.ovrAvatar2MaterialExtensionEntry metaData))
                {
                    return false;
                }

                // Now grab the name and the data of the entry
                bool success = false;
                switch (metaData.entryType)
                {
                    case CAPI.ovrAvatar2MaterialExtensionEntryType.Float:
                        success = StoreNameAndPayloadForEntry(
                            primitiveId,
                            extensionIndex,
                            entryIndex,
                            metaData,
                            _floatEntries);
                        break;
                    case CAPI.ovrAvatar2MaterialExtensionEntryType.Int:
                        success = StoreNameAndPayloadForEntry(
                            primitiveId,
                            extensionIndex,
                            entryIndex,
                            metaData,
                            _intEntries);
                        break;
                    case CAPI.ovrAvatar2MaterialExtensionEntryType.Vector3f:
                        success = StoreNameAndPayloadForEntry(
                            primitiveId,
                            extensionIndex,
                            entryIndex,
                            metaData,
                            _vector3Entries);
                        break;
                    case CAPI.ovrAvatar2MaterialExtensionEntryType.Vector4f:
                        success = StoreNameAndPayloadForEntry(
                            primitiveId,
                            extensionIndex,
                            entryIndex,
                            metaData,
                            _vector4Entries);
                        break;
                    case CAPI.ovrAvatar2MaterialExtensionEntryType.ImageId:
                        success = GetNameAndPayloadForEntry(
                            primitiveId,
                            extensionIndex,
                            entryIndex,
                            metaData,
                            out string entryName,
                            out CAPI.ovrAvatar2Id payload);
                        if (success)
                        {
                            // Convert image ID to texture
                            success = OvrAvatarManager.GetOvrAvatarAsset(payload, out OvrAvatarImage image);
                            if (success)
                            {
                                _textureEntries.Add(new ExtensionEntry<Texture2D>(entryName, image.texture));
                            }
                            else
                            {
                                OvrAvatarLog.LogError($"Could not find image {payload}", LOG_SCOPE);
                            }
                        }

                        break;
                }

                return success;
            }

            private static bool GetEntryMetaData(
                CAPI.ovrAvatar2Id primitiveId,
                UInt32 extensionIdx,
                UInt32 entryIdx,
                out CAPI.ovrAvatar2MaterialExtensionEntry metaData)
            {
                var result = CAPI.ovrAvatar2Primitive_MaterialExtensionEntryMetaDataByIndex(
                    primitiveId,
                    extensionIdx,
                    entryIdx,
                    out metaData);

                if (!result.EnsureSuccess(
                        $"MaterialExtensionEntryMetaDataByIndex ({extensionIdx}, {entryIdx}) {result}", LOG_SCOPE))
                {
                    return false;
                }

                return true;
            }

            private static bool GetNameAndPayloadForEntry<T>(
                CAPI.ovrAvatar2Id primitiveId,
                UInt32 extensionIdx,
                UInt32 entryIdx,
                in CAPI.ovrAvatar2MaterialExtensionEntry metaData,
                out string entryName,
                out T payload)
            {
                entryName = String.Empty;
                payload = default;

                unsafe
                {
                    var nameBuffer = stackalloc byte[(int)metaData.nameBufferSize];
                    var dataBuffer = stackalloc byte[(int)metaData.dataBufferSize];

                    var result = CAPI.ovrAvatar2Primitive_MaterialExtensionEntryDataByIndex(
                        primitiveId,
                        extensionIdx,
                        entryIdx,
                        nameBuffer,
                        metaData.nameBufferSize,
                        dataBuffer,
                        metaData.dataBufferSize);

                    if (!result.EnsureSuccess(
                            $"MaterialExtensionEntryDataByIndex ({extensionIdx}, {entryIdx}) {result}", LOG_SCOPE))
                    {
                        return false;
                    }

                    entryName = Marshal.PtrToStringAnsi((IntPtr)nameBuffer);
                    payload = (T)Marshal.PtrToStructure((IntPtr)dataBuffer, typeof(T));
                }

                return true;
            }

            private static bool StoreNameAndPayloadForEntry<T>(
                CAPI.ovrAvatar2Id primitiveId,
                UInt32 extensionIdx,
                UInt32 entryIdx,
                in CAPI.ovrAvatar2MaterialExtensionEntry metaData,
                in List<ExtensionEntry<T>> listToStoreInto)
            {
                if (!GetNameAndPayloadForEntry(
                        primitiveId,
                        extensionIdx,
                        entryIdx,
                        metaData,
                        out string entryName,
                        out T payload))
                {
                    return false;
                }

                listToStoreInto.Add(new ExtensionEntry<T>(entryName, payload));
                return true;
            }
        }

        //////////////////////////////////////////////////
        // OvrAvatarMaterialExtension
        //////////////////////////////////////////////////
        private readonly ExtensionEntries _entries;
        private readonly string _name;

        private static readonly string LOG_SCOPE = nameof(OvrAvatarMaterialExtension);

        private OvrAvatarMaterialExtension(string extensionName, ExtensionEntries entries)
        {
          _name = extensionName;
          _entries = entries;
        }

        public string Name => _name;

        public void ApplyEntriesToMaterial(Material material, OvrAvatarMaterialExtensionConfig extensionConfig)
        {
            if (_entries == null || material == null || extensionConfig == null)
            {
                return;
            }

            _entries.ApplyToMaterial(material, _name, extensionConfig);
        }

        public static bool LoadExtension(CAPI.ovrAvatar2Id primitiveId, UInt32 extensionIndex, out OvrAvatarMaterialExtension materialExtension)
        {
            materialExtension = null;

            // Get extension name
            if (!GetMaterialExtensionName(primitiveId, extensionIndex, out string extensionName))
            {
                return false;
            }

            // Get entries for the extension
            ExtensionEntries entries = new ExtensionEntries();
            if (!GetNumEntries(primitiveId, extensionIndex, out uint numEntries))
            {
                return false;
            }

            // Loop over all entries
            for (UInt32 entryIdx = 0; entryIdx < numEntries; entryIdx++)
            {
                if (!entries.LoadEntry(primitiveId, extensionIndex, entryIdx))
                {
                    return false;
                }
            }

            materialExtension = new OvrAvatarMaterialExtension(extensionName, entries);

            return true;
        }

        private static bool GetMaterialExtensionName(CAPI.ovrAvatar2Id primitiveId, UInt32 extensionIdx, out string extensionName)
        {
            unsafe
            {
                extensionName = String.Empty;

                // Get extension name
                uint nameSize = 0;
                var result = CAPI.ovrAvatar2Primitive_GetMaterialExtensionName(
                    primitiveId,
                    extensionIdx,
                    null,
                    &nameSize);

                if (!result.EnsureSuccess($"GetMaterialExtensionName ({extensionIdx}) {result}", LOG_SCOPE)) {
                    return false;
                }

                var nameBuffer = stackalloc byte[(int)nameSize];
                result = CAPI.ovrAvatar2Primitive_GetMaterialExtensionName(
                    primitiveId,
                    extensionIdx,
                    nameBuffer,
                    &nameSize);
                if (!result.EnsureSuccess($"GetMaterialExtensionName ({extensionIdx}) {result}", LOG_SCOPE)) {
                    return false;
                }

                extensionName = Marshal.PtrToStringAnsi((IntPtr)nameBuffer);
            }

            return true;
        }

        private static bool GetNumEntries(CAPI.ovrAvatar2Id primitiveId, UInt32 extensionIdx, out UInt32 count)
        {
            var result =
                CAPI.ovrAvatar2Primitive_GetNumEntriesInMaterialExtensionByIndex(primitiveId, extensionIdx, out count);

            if (!result.EnsureSuccess($"GetNumEntriesInMaterialExtensionByIndex ({extensionIdx}) {result}", LOG_SCOPE)) {
                return false;
            }

            return true;
        }
    }
}
