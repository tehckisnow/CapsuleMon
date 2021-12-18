using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Opening : MonoBehaviour
{
    [SerializeField] LevelLoader levelLoader;
    [SerializeField] Image whiteBG;
    [SerializeField] TextMeshProUGUI nottendo;
    [SerializeField] Image star;
    [SerializeField] Image frontSprite;
    [SerializeField] Image rearSprite;
    [SerializeField] Image ground;
    [SerializeField] Image pressStartBG;
    [SerializeField] GameObject letterbox;
    [SerializeField] Image capsulemon;
    [SerializeField] TextMeshProUGUI version;
    [SerializeField] TextMeshProUGUI pressStart;

    private Coroutine openingCoroutine;

    private void Init()
    {
        whiteBG.enabled = false;
        nottendo.enabled = false;
        star.enabled = false;
        frontSprite.enabled = false;
        rearSprite.enabled = false;
        ground.enabled = false;
        capsulemon.enabled = false;
        version.enabled = false;
        pressStart.enabled = false;
        pressStartBG.enabled = false;
        letterbox.SetActive(false);
    }

    void Start()
    {
        Init();
        openingCoroutine = StartCoroutine(OpeningAnimation());
    }

    private void Update()
    {
        if(Input.GetButtonDown("Submit"))
        {
            SkipIntro();
        }
    }

    private void SkipIntro()
    {
        StopCoroutine(openingCoroutine);
        levelLoader.LoadStartMenu();
    }

    IEnumerator OpeningAnimation()
    {
        yield return new WaitForSeconds(2f);
        whiteBG.enabled = true;
        yield return new WaitForSeconds(2f);
        nottendo.enabled = true;
        yield return new WaitForSeconds(1f);
        star.enabled = true;
        yield return new WaitForSeconds(2f);

        letterbox.SetActive(true);
        yield return new WaitForSeconds(1f);

        nottendo.enabled = false;
        star.enabled = false;
        yield return new WaitForSeconds(2f);


        frontSprite.enabled = true;
        rearSprite.enabled = true;
        //frontSprite.GetComponent<AnimatedImage>().PlayEnterAnim(-500f, 2f);
        frontSprite.GetComponent<AnimatedImage>().ReturnToOriginalPos(-500f, 0f, 2f);
        //rearSprite.GetComponent<AnimatedImage>().PlayEnterAnim(500f, 2f);
        rearSprite.GetComponent<AnimatedImage>().ReturnToOriginalPos(500f, 0f, 2f);
        yield return new WaitForSeconds(2f);

        ground.enabled = true;
        letterbox.SetActive(false);
        yield return new WaitForSeconds(1f);
        capsulemon.enabled = true;
        yield return new WaitForSeconds(1f);
        version.enabled = true;
        yield return new WaitForSeconds(2f);
        pressStartBG.enabled = true;
        pressStart.enabled = true;
        
        yield return new WaitUntil(() => Input.GetButtonDown("Submit"));
        yield return new WaitForSeconds(0.5f);
        levelLoader.LoadStartMenu();
    }
}
