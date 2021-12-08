using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempPersistentNameSetter : MonoBehaviour
{
    [SerializeField] string gamePlayScene = "Gameplay";

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += Finish;
        SceneManager.LoadScene(gamePlayScene);
    }

    private void Finish(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= Finish;
        GameController.Instance.StartNameSetterMenu();
        Destroy(gameObject);
    }
}
