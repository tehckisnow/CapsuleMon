using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] GameObject essentialObjectsPrefab;
    [SerializeField] int x = 0;
    [SerializeField] int y = 0;
    [SerializeField] bool spawnAtCenter = true;
    
    private void Awake()
    {
        var existingObjects = FindObjectsOfType<EssentialObjects>();
        if(existingObjects.Length == 0)
        {
            var spawnPos = new Vector3(0, 0, 0);
            //if there is a grid, then spawn at it's center
            if(spawnAtCenter)
            {
                var grid = FindObjectOfType<Grid>();
                if(grid != null)
                {
                    spawnPos = grid.transform.position;
                }
            }

            Instantiate(essentialObjectsPrefab, spawnPos, Quaternion.identity);
        }
    }
}
