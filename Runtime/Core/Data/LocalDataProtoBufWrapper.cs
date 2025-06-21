using System.IO;
using System;
using UnityEngine;
using ProtoBuf;

namespace Core
{
    public class LocalDataProtoBufWrapper<T> : ILocalDataWrapper where T : ILocalData, new()
    {
        private T _obj;
        private const string LOCAL_DATA_FOLDER_NAME = "Profiles";
        private const string LOCAL_DATA_FILE_NAME_TEMPLATE = "{0}.dat";
        private string _gameDataFolderPath = string.Empty;

        private static readonly byte[] xorKey = System.Text.Encoding.UTF8.GetBytes("@lo_12345####_HuHa");

        public virtual void Init()
        {
            _gameDataFolderPath = GetProfileFolderPath();
            if (!Directory.Exists(_gameDataFolderPath))
            {
                Directory.CreateDirectory(_gameDataFolderPath);
            }

            string dataFilePath = GetLocalDataPath(typeof(T).Name);

            if (File.Exists(dataFilePath))
            {
                _obj = LoadData<T>(dataFilePath);
            }
            else
            {
                _obj = new T();
                _obj.InitAfterLoadData();
                SaveData<T>();
            }
        }

        public virtual void SaveData<T1>() where T1 : ILocalData
        {
            if (_obj == null)
            {
                return;
            }

            string filePath = GetLocalDataPath(typeof(T1).Name);
            try
            { 
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, _obj);
                    byte[] rawData = ms.ToArray();

                    byte[] encoded = XorBytes(rawData, xorKey);

                    File.WriteAllBytes(filePath, encoded);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save data: {ex.Message}");
            }
        }

        public virtual void SaveData<T1>(object obj) where T1 : ILocalData
        {
            if (obj is T data)
            {
                _obj = data;
                SaveData<T1>();
            }
        }

        public T1 GetData<T1>() where T1 : ILocalData
        {
            if (_obj is T1 data)
            {
                return data;
            }

            return default(T1);
        }

        private string GetProfileFolderPath()
        {
            string folderPath = Path.Combine(Application.persistentDataPath, LOCAL_DATA_FOLDER_NAME);
            Directory.CreateDirectory(folderPath);
            return folderPath;
        }

        private string GetLocalDataPath(string dataName)
        {
            string dataFileName = string.Format(LOCAL_DATA_FILE_NAME_TEMPLATE, dataName);
            return Path.Combine(_gameDataFolderPath, dataFileName);
        }

        private T1 LoadData<T1>(string path) where T1 : ILocalData
        {
            try
            {
                byte[] encoded = File.ReadAllBytes(path);
                byte[] decoded = XorBytes(encoded, xorKey);

                using (var ms = new MemoryStream(decoded))
                {
                    return ProtoBuf.Serializer.Deserialize<T1>(ms);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load data: {ex.Message}");
                return default;
            }
        }

        private static byte[] XorBytes(byte[] data, byte[] key)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            return result;
        }
    }
}