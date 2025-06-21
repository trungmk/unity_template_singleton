using System.Collections.Generic;
using UnityEngine;

public static class SceneHandler 
{
    private class SceneData
    {
        public string Name;

        public string Path;
    }

    static Dictionary<int, SceneData> _contextDict = new Dictionary<int, SceneData>();

    public static void AddContext(int contextKey, string contextName, string path)
    {
        _contextDict.Add(contextKey, new SceneData { Name = contextName, Path = path });
    }

    public static string GetSceneName(int contextKey)
    {
        if(!_contextDict.ContainsKey(contextKey))
        {
            Debug.LogError(contextKey + "is not exist.");
            return string.Empty;
        }

        return _contextDict[contextKey].Name;
    }

    public static string GetScenePath(int contextKey)
    {
        if (!_contextDict.ContainsKey(contextKey))
        {
            Debug.LogError(contextKey + "is not exist.");
            return string.Empty;
        }

        return _contextDict[contextKey].Path;
    }
}