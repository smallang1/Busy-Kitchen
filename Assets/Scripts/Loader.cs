using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        GameMenuScene,
        LoadingScene,
        GameScene
    }
    private static Scene targetScene;

    public static void Load(Scene target)
    {
        //将游戏时间缩放比例设置为正常速度（1倍速）。
        Time.timeScale = 1;
        targetScene = target;
        SceneManager.LoadScene((int)Scene.LoadingScene);
    }
    public static void LoadBack()
    {
        SceneManager.LoadScene((int)targetScene);
    }
}
