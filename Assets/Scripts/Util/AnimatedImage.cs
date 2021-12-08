using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class AnimatedImage : MonoBehaviour
{
    [SerializeField] Image image;
    Vector3 originalPos;

    private void Awake()
    {
        originalPos = gameObject.transform.position;
    }

    //move image horizontally to original position over time
    public void PlayEnterAnim(float xVal, float time)
    {
        //local position
        //var start = image.transform.localPosition.x + xVal;
        //image.transform.localPosition = new Vector3(start, originalPos.y);
        //image.transform.DOLocalMoveX(originalPos.x, time);
        
        //world position
        var start = image.transform.position.x + xVal;
        image.transform.position = new Vector3(start, originalPos.y);
        image.transform.DOMoveX(originalPos.x, time);
    }
}
