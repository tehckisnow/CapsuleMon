using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color highlightedColor;
    public Color HighlightedColor => highlightedColor;
    
    [SerializeField] Color unhighlightedColor;
    public Color UnhighlightedColor => unhighlightedColor;


    public static GlobalSettings i { get; private set; }

    private void Awake()
    {
        i = this;
    }
}
