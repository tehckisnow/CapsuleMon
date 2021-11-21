using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFader : MonoBehaviour
{
    public static SpriteFader instance;

    private void Awake()
    {
        instance = this;
        
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            if(instance != this)
            {
                Destroy(this);
            }
        }
    }

    public static void FadeSprite(SpriteRenderer sprite, float increment)
    {
        SpriteFader spriteFader = new SpriteFader();
        spriteFader.StartCoroutine(FadeOut(sprite, increment));
    }

    private static IEnumerator FadeOut(SpriteRenderer sprite, float decrement)
    {
        float a = 1;
        while(a > Mathf.Epsilon)
        {
            a -= decrement;
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, a);
            yield return null;
        }
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0);
    }
}
