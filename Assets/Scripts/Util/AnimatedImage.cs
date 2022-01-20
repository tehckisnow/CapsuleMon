using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AnimatedImage : MonoBehaviour
{
    private Image image;
    public bool inTransition = false;
    public bool currentlyAnimating = false;

    private Coroutine coroutine;
    private Action OnAnimComplete;

    private void Awake()
    {
        Image imageComponent = GetComponent<Image>();
        if(imageComponent != null)
        {
            SetImage(imageComponent);
        }
    }

    public void SetImage(Image _image)
    {
        this.image = _image;
    }

    public void SetSprite(Sprite _sprite)
    {
        this.image.sprite = _sprite;
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        currentlyAnimating = false;
        inTransition = false;
        coroutine = null;
        gameObject.SetActive(false);
    }

    public Vector3 ScreenToWorld(Vector2 vector2)
    {
        Vector3 coords = new Vector3(vector2.x, vector2.y);
        Camera camera = image.canvas.worldCamera;
        Vector3 worldCoords = camera.ScreenToWorldPoint(coords);
        return worldCoords;
    }

    public IEnumerator GoTo(float x, float y, float time=0.5f, Action callback=null)
    {
        yield return Move(x, y, time, callback);
    }

    public IEnumerator Move(float x, float y, float time=0.5f, Action callback=null)
    {
        Vector2 coords = new Vector2(x, y);
        if(time == 0f)
        {
            currentlyAnimating = true;
            transform.position = coords;
            currentlyAnimating = false;
        }
        else
        {
            currentlyAnimating = true;
            float totalDistance = Vector2.Distance(transform.position, coords);
            float distance = totalDistance;
            float distanceToMove = totalDistance / time / 60;
            while(distance > Mathf.Epsilon)
            {
                transform.position = Vector2.MoveTowards(transform.position, coords, distanceToMove);
                distance = Vector2.Distance(transform.position, coords);
                yield return null;
            }
            transform.position = coords;

            currentlyAnimating = false;

            OnAnimComplete?.Invoke();
            callback?.Invoke();
        }
    }

    public void MoveRelative(float x, float y, float time=0.5f, Action callback=null)
    {
        Vector2 coords = new Vector2(x, y);
        Vector2 pos = transform.position;
        Vector2 finalCoords = pos + coords;
        StartCoroutine(Move(finalCoords.x, finalCoords.y, time, callback));
    }

    public IEnumerator MoveRelativeCoroutine(float x, float y, float time=0.5f, Action callback=null)
    {
        Vector2 coords = new Vector2(x, y);
        Vector2 pos = transform.position;
        Vector2 finalCoords = pos + coords;
        yield return Move(finalCoords.x, finalCoords.y, time, callback);
    }

    // public IEnumerator ReturnToOriginalPosCoroutine(float x, float y, float time=0.5f, Action callback=null)
    // {
    //     Vector2 origPos = transform.position;
    //     Vector2 newPos = new Vector2(x, y);
    //     transform.position = newPos;
    //     yield return Move(origPos.x, origPos.y, time, callback);
    // }

    // public void ReturnToOriginalPos(float x, float y, float time=0.5f, Action callback=null)
    // {
    //     Vector2 origPos = transform.position;
    //     Vector2 newPos = new Vector2(x, y);
    //     transform.position = newPos;
    //     StartCoroutine(Move(origPos.x, origPos.y, time, callback));
    // }

    // public void ReturnToOriginalPosRelative(float x, float y, float time=0.5f, Action callback=null)
    // {
    //     Debug.Log(transform.position);
    //     Vector2 origPos = transform.position;
    //     Debug.Log(origPos);
    //     Vector2 newPos = new Vector2(x, y);
    //     Debug.Log(newPos);
    //     newPos = newPos + origPos;
    //     Debug.Log(newPos);
    //     transform.position = newPos;
    //     StartCoroutine(Move(origPos.x, origPos.y, time, callback));
    // }

    // public IEnumerator ReturnToOriginalPosRelativeCoroutine(float x, float y, float time=0.5f, Action callback=null)
    // {
    //     Vector2 origPos = transform.position;
    //     Vector2 newPos = new Vector2(x, y);
    //     newPos = newPos + origPos;
    //     transform.position = newPos;
    //     yield return Move(origPos.x, origPos.y, time, callback);
    // }

    // public void ReturnToOriginalPosLocal(float x, float y, float time=0.5f, Action callback=null)
    // {
    //     ReturnToOriginalPos(x, y, time, callback);
    // }

    // public IEnumerator ReturnToOriginalPosLocalCoroutine(float x, float y, float time=0.5f, Action callback=null)
    // {
    //     yield return ReturnToOriginalPosCoroutine(x, y, time, callback);
    // }

    public IEnumerator FadeIn(float time=0.5f)
    {
        float alpha = 0;
        float alphaIncrement = 0.01f;
        Color newColor = new Color(image.color.r, image.color.g, image.color.b, alpha);
        float timeIncrement = time / (1 / alphaIncrement);

        while(alpha < 1)
        {
            alpha += alphaIncrement;
            newColor = new Color(image.color.r, image.color.g, image.color.b, alpha);
            image.color = newColor;
            yield return new WaitForSeconds(timeIncrement);
        }

        newColor = new Color(image.color.r, image.color.g, image.color.b, alpha);
        image.color = newColor;
    }

    public IEnumerator FadeOut(float time=0.5f)
    {
        float alpha = 1;
        float alphaIncrement = 0.01f;
        Color newColor = new Color(image.color.r, image.color.g, image.color.b, alpha);
        float timeIncrement = time / (alpha / alphaIncrement);

        while(alpha > 0)
        {
            alpha -= alphaIncrement;
            newColor = new Color(image.color.r, image.color.g, image.color.b, alpha);
            image.color = newColor;
            yield return new WaitForSeconds(timeIncrement);
        }

        newColor = new Color(image.color.r, image.color.g, image.color.b, alpha);
        image.color = newColor;
    }

    //! unfinished
    public void StopAnim()
    {
        if(currentlyAnimating)
        {
            if(coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            currentlyAnimating = false;
        }
    }

    // transition between two images
    public void Transition(Sprite newSprite, float time, int cycles=10, Action callback=null)
    {
        StartCoroutine(TransitionCoroutine(newSprite, time, cycles, callback));
    }

    public IEnumerator TransitionCoroutine(Sprite newSprite, float time, int cycles=10, Action callback=null)
    {
        inTransition = true;
        currentlyAnimating = true;

        var firstSprite = image.sprite;
        //int numberOfCycles = 10;
        int numberOfCycles = cycles;
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
        currentlyAnimating = false;
        callback?.Invoke();
        yield return null;
    }
}