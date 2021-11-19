using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask collisionsLayer;
    [SerializeField] LayerMask interactablesLayer;
    [SerializeField] LayerMask walkableLayer;
    [SerializeField] LayerMask grassLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask fovLayer;

    public static GameLayers Instance { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public LayerMask CollisionsLayer {
        get => collisionsLayer;
    }
    
    public LayerMask InteractablesLayer {
        get => interactablesLayer;
    }
    
    public LayerMask WalkableLayer {
        get => walkableLayer;
    }
    
    public LayerMask GrassLayer {
        get => grassLayer;
    }
    
    public LayerMask PlayerLayer {
        get => playerLayer;
    }

    public LayerMask FOVLayer {
        get => fovLayer;
    }
}
