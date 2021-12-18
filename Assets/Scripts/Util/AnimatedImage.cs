using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;

public class AnimatedImage : MonoBehaviour
{
    [SerializeField] Image image;
    Vector3 originalPos;
    public bool inTransition = false;

    private void Awake()
    {
        ResetOriginalPos();
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void ResetOriginalPos()
    {
        originalPos = gameObject.transform.position;
    }

    public void Reset()
    {
        transform.position = originalPos;
    }

    public void Disable()
    {
        inTransition = false;
        Reset();
        gameObject.SetActive(false);
    }

    public IEnumerator ReturnToOriginalPosCoroutine(float x, float y, float time, Action callback=null)
    {
        Tween tween = ReturnToOriginalPos(x, y, time);
        yield return tween.WaitForCompletion();
        //yield return new WaitForSeconds(2f);
        callback?.Invoke();
    }

    // public void ReturnToOriginalPos(float x, float y, float time)
    // {
    //     var startPosition = new Vector3(originalPos.x + x, originalPos.y + y);
    //     image.transform.position = startPosition;
    //     image.transform.DOMove(originalPos, time);
    // }

    public Tween ReturnToOriginalPos(float x, float y, float time)
    {
        //world position
        var startPosition = new Vector3(originalPos.x + x, originalPos.y + y);
        image.transform.position = startPosition;
        Tween tween = image.transform.DOMove(originalPos, time);
        return tween;
    }

    //!untested
    public void ReturnToOriginalPosLocal(float x, float y, float time)
    {
        //local position
        //inTransition = true;
        var origPos = image.transform.localPosition;
        image.transform.localPosition = new Vector3(x, y);
        image.transform.DOLocalMove(origPos, time);
        //inTransition = false;
    }

    //!untested
    //world coordinates
    public void GoTo(float x, float y, float time)
    {
        //inTransition = true;
        var newPos = new Vector3(x, y);
        image.transform.DOMove(newPos, time);
        //inTransition = false;
    }

    //!untested
    public void GoToLocal(float x, float y, float time)
    {
        //inTransition = true;
        var newPos = new Vector3(x, y);
        image.transform.DOLocalMove(newPos, time);
        //inTransition = false;
    }

    //!untested
    //world coordinates, relative to current position
    public void MoveRelative(float x, float y, float time)
    {
        //inTransition = true;
        var newPos = new Vector3(image.transform.position.x + x, image.transform.position.y + y);
        image.transform.DOMove(newPos, time);
        //inTransition = false;
    }

    //local coordinates, relative to current position    
    public void MoveRelativeLocal(float x, float y, float time)
    {
        //inTransition = true;
        var newPos = new Vector3(image.transform.localPosition.x + x, image.transform.localPosition.y + y);
        image.transform.DOLocalMove(newPos, time);
        //inTransition = false;
    }

    public void Transition(Sprite newSprite, float time)
    {
        StartCoroutine(TransitionCoroutine(newSprite, time));
    }

    public IEnumerator TransitionCoroutine(Sprite newSprite, float time)
    {
        inTransition = true;

        var firstSprite = image.sprite;
        int numberOfCycles = 10;
        int currentCycle = 0;
        IEnumerator Cycle(float increment1, float increment2)
        {
            //original image
            yield return new WaitForSeconds(increment1);
            image.sprite = newSprite;
            yield return new WaitForSeconds(increment2);
            image.sprite = firstSprite;
            yield return null;
        }

        //first increment = how long to show first image each cycle
        //second increment = how long to show second image each cycle
        //start off equal, but gradually
        var firstIncrement = time / (float)numberOfCycles;
        var secondIncrement = firstIncrement;
        var firstIncrementOrig = firstIncrement;
        while(currentCycle <= numberOfCycles)
        {
            yield return Cycle(firstIncrement, secondIncrement);
            //shrink increment here
            var shift = firstIncrementOrig / (float)numberOfCycles;
            firstIncrement -= shift;
            secondIncrement += shift;
            currentCycle++;
        }
        image.sprite = newSprite;

        yield return new WaitForSeconds(0.2f);

        inTransition = false;
        yield return null;
    }
}