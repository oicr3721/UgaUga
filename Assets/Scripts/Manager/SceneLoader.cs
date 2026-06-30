using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SceneType
{
    Home,
    Hunt,
    Result
}

public static class SceneLoader
{
    private static readonly Dictionary<SceneType, string> sceneTable = new()
    {
        { SceneType.Home, "HomeScene" },
        { SceneType.Hunt, "HuntScene" },
        { SceneType.Result, "ResultScene" },
    };

    public static void Load(SceneType scene)
    {
        if (!sceneTable.TryGetValue(scene, out string sceneName))
        {
            Debug.LogError($"등록되지 않은 Scene : {scene}");
            return;
        }

        SceneTransition.Instance.Load(sceneName);
    }
}