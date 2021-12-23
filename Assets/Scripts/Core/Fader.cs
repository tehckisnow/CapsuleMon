using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Fader : MonoBehaviour
{
    [SerializeField] Color blackColor;
    [SerializeField] Color whiteColor;
    [SerializeField] Color color2;
    [SerializeField] Color color3;
    [SerializeField] Color color4;
    [SerializeField] Color color5;

    Image image;

    private Color defaultColor;
    public Color DefaultColor => defaultColor;
    public Color BlackColor => blackColor;
    public Color WhiteColor => whiteColor;
    public Color Color2 => color2;
    public Color Color3 => color3;
    public Color Color4 => color4;
    public Color Color5 => color5;

    public static Fader Instance;

    private void Awake()
    {
        image = GetComponent<Image>();
        Instance = this;
        defaultColor = image.color;
    }

    public IEnumerator FadeIn(float time)
    {
        yield return image.DOFade(1f, time).WaitForCompletion();
    }

    public IEnumerator FadeOut(float time)
    {
        yield return image.DOFade(0f, time).WaitForCompletion();
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }
}
