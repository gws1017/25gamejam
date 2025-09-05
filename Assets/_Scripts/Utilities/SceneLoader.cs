using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public enum Scene
    {
        Default,
        MainMenuScene,
        LoadingScene,
        GameScene,
    }

    private static Scene targetScene = Scene.MainMenuScene;

    public static void LoadScene(Scene targetScene)
    {
        // If the target scene is on Default, do nothing
        if (targetScene == Scene.Default) return;

        // Set the target scene to the static variable 'targetScene'
        SceneLoader.targetScene = targetScene;

        // Load the loading scene
        SceneManager.LoadScene(Scene.LoadingScene.ToString());

        // Load the actual target scene in the SceneLoader_Callback method (which is in the LoadingScene)

        // 01) Load the LoadingScene first
        // 02) Then the LoadingScene will call the target scene in turn
    }

    public static void SceneLoader_Callback()
    {
        SceneManager.LoadScene(targetScene.ToString());

        // Clear the target scene after loading
        ClearTargetScene();
    }

    public static string GetTargetScene()
    {
        return targetScene.ToString();
    }

    private static void ClearTargetScene()
    {
        // Clear the target scene after loading
        targetScene = Scene.Default; // Reset to a default scene
    }
}