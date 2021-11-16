using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] MonBase _base;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;

    public Mon Mon { get; set; }

    public void Setup()
    {
        Mon = new Mon(_base, level);
        if(isPlayerUnit)
        {
            GetComponent<Image>().sprite = Mon.Base.BackSprite;
        }
        else
        {
            GetComponent<Image>().sprite = Mon.Base.FrontSprite;
        }
    }
}
