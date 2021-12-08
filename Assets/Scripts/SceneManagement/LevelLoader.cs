using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] string openingSceneName = "Opening";
    [SerializeField] string gameplaySceneName = "Gameplay";
    [SerializeField] string startMenuSceneName = "StartMenu";
    [SerializeField] string settingsSceneName = "Settings";

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void UnloadScene(string scene)
    {
        SceneManager.UnloadSceneAsync(scene);
        //!Resources.UnloadUnusedAssets();
    }

    public void LoadSceneAdditive(string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Additive);
    }

    public void LoadSceneFresh(string scene)
    {
        Destroy(FindObjectOfType<EssentialObjects>().gameObject);
        LoadScene(scene);
    }

    public void LoadSceneFreshAdditive(string scene)
    {
        Destroy(FindObjectOfType<EssentialObjects>().gameObject);
        LoadSceneFresh(scene);
    }

    public void LoadOpening(bool fresh=false)
    {
        if(fresh)
        {
            LoadSceneFresh(openingSceneName);
        }
        else
        {
            LoadScene(openingSceneName);    
        }
    }

    public void LoadGameplay(bool fresh=false, bool additive=false)
    {
        if(fresh && additive)
        {
            LoadSceneFreshAdditive(gameplaySceneName);    
        }
        else if(fresh && !additive)
        {
            LoadSceneFresh(gameplaySceneName);    
        }
        else if(!fresh && additive)
        {
            LoadSceneAdditive(gameplaySceneName);
        }
        else
        {
            LoadScene(gameplaySceneName);    
        }
    }

    public void LoadStartMenu(bool fresh=false)
    {
        if(fresh)
        {
            LoadSceneFresh(startMenuSceneName);    
        }
        else
        {
            LoadScene(startMenuSceneName);    
        }
    }

    public void LoadSettingsMenu()
    {
        LoadScene(settingsSceneName);
    }

    public string GetNameOfCurrentScene()
    {
        return SceneManager.GetActiveScene().name;
    }
}
