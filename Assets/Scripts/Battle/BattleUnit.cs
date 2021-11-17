using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;

    public Mon Mon { get; set; }

    Image image;
    Vector3 originalPos;
    Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }

    public void Setup(Mon mon)
    {
        Mon = mon;
        if(isPlayerUnit)
        {
            image.sprite = Mon.Base.BackSprite;
        }
        else
        {
            image.sprite = Mon.Base.FrontSprite;
        }

        image.color = originalColor;
        PlayEnterAnimation();
    }

    public void PlayEnterAnimation()
    {
        if(isPlayerUnit)
        {
            image.transform.localPosition = new Vector3(-500f, originalPos.y);
        }
        else
        {
            image.transform.localPosition = new Vector3(500f, originalPos.y);
        }
        //return to original position
        image.transform.DOLocalMoveX(originalPos.x, 1f);
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if(isPlayerUnit)
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        }
        else
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));
        }
        //return to original position
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        //sequence.Join(image.DOFade(0f, 0.5)); //! broken
        StartCoroutine(FadeOut(0.01f));
    }

    //image.DoFade is broken so this will replace it
    IEnumerator FadeOut(float decrement)
    {
        float a = 1;
        while(a > Mathf.Epsilon)
        {
            a -= decrement;
            image.color = new Color(image.color.r, image.color.g, image.color.b, a);
            yield return null;
        }
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
    }
}
