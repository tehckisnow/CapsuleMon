using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempPersistentGameLoader : MonoBehaviour
{
    [SerializeField] string gamePlayScene = "Gameplay";
    private string saveFile;

    public void Init(string saveFile)
    {
        DontDestroyOnLoad(gameObject);
        this.saveFile = saveFile;
        SceneManager.sceneLoaded += Finish;
        SceneManager.LoadScene(gamePlayScene);
    }

    //gives SceneManager a chance to load scene before starting next processes
    private void Finish(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= Finish;
        //coroutines delay to give SaveSystem time to load data
        StartCoroutine(Transition());
        StartCoroutine(LoadGameCoroutine());
        StartCoroutine(Dest());
    }

    //gives saveSystem a chance to load before displaying game world
    IEnumerator Transition()
    {
        GameController.Instance.state = GameState.Paused;
        yield return Fader.Instance.FadeIn(0f);
        yield return new WaitForSeconds(1f);
        yield return Fader.Instance.FadeOut(1f);
        yield return new WaitForSeconds(0.2f);
        GameController.Instance.state = GameState.FreeRoam;
    }

    //gives new scene a chance to load before saveSystem loads save data
    IEnumerator LoadGameCoroutine()
    {
        yield return new WaitForSeconds(1f);
        LoadGame(saveFile);
    }

    private void LoadGame(string saveFile)
    {
        SavingSystem.i.Load(saveFile);
        GameController.Instance.state = GameState.FreeRoam;
    }

    //gives everything else a chance to conclude before destroying this object
    IEnumerator Dest()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}
