using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;

    public bool IsLoaded { get; private set; }

    private List<SavableEntity> savableEntities;

    [SerializeField] private bool isOutdoors = true;
    public bool IsOutdoors => isOutdoors;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            PlayerController.Instance.IsOutside = IsOutdoors;

            LoadScene();
            GameController.Instance.SetCurrentScene(this);

            //load all connected scenes
            foreach(var scene in connectedScenes)
            {
                scene.LoadScene();
            }

            // unload the scenes that are no longer connected
            var prevScene = GameController.Instance.PrevScene;
            if(GameController.Instance.PrevScene != null)
            {
                var previouslyLoadedScenes = prevScene.connectedScenes;
                foreach(var scene in previouslyLoadedScenes)
                {
                    if(!connectedScenes.Contains(scene) && scene != this)
                    {
                        scene.UnloadScene();
                    }
                }

                // also check if prevScene needs to be unloaded (necessary because of save/load and teleporting)
                if(!connectedScenes.Contains(prevScene))
                {
                    prevScene.UnloadScene();
                }
            }
        }
    }

    public void LoadScene()
    {
        if(!IsLoaded)
        {
            //SceneManager.LoadSceneAsync loads scene asynchronously and returns AsyncOperation object
            // which is used below to assign an event to trigger after the scene has finished loading
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;

            operation.completed += (AsyncOperation) =>
            {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };
            
        }
    }

    public void UnloadScene()
    {
        if(IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);

            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    private List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        //var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();
        //replaced the line above with the code below using FindObjectsOfTypeAll so that it operates on inactive objects as well;
        var savableEntitiesArray = Resources.FindObjectsOfTypeAll(typeof(SavableEntity));
        //convert to list
        List<SavableEntity> savableEntities = new List<SavableEntity>();
        foreach(SavableEntity entity in savableEntitiesArray)
        {
            if(entity.gameObject.scene == currScene)
            {
                savableEntities.Add(entity);
            }
        }

        return savableEntities;
    }
}
