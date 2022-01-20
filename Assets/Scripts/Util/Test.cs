using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] Image bulba;
    [SerializeField] Image charmander;
    [SerializeField] Image localBulba;
    [SerializeField] Image localChar;

    private AnimatedImage animatedImage;

    private void Start()
    {
        animatedImage = GetComponent<AnimatedImage>();

        StartCoroutine(Anim2());
        //StartCoroutine(AnimMove());
        //StartCoroutine(LocalAnimMove());
    }

    IEnumerator Anim2()
    {
        yield return null;
        AnimatedImage bulbaAnimated = bulba.GetComponent<AnimatedImage>();
    }

    IEnumerator Anim()
    {
        yield return null;

        Vector2 bulbaOrigPos = new Vector2(bulba.transform.position.x, bulba.transform.position.y);
        Vector2 localBulbaOrigPos = new Vector2(localBulba.transform.position.x, localBulba.transform.position.y);

        Vector2 coords = new Vector2(charmander.gameObject.transform.position.x, charmander.gameObject.transform.position.y);

        var bulbaAnimImg = bulba.GetComponent<AnimatedImage>();
        yield return bulbaAnimImg.GoTo(coords.x, coords.y, 3f);

        Vector2 localCoords = new Vector2(localChar.transform.position.x, localChar.transform.position.y);
        var localBulbaAnimImg = localBulba.GetComponent<AnimatedImage>();
        yield return localBulbaAnimImg.GoTo(localCoords.x, localCoords.y, 3f);

        yield return new WaitForSeconds(2f);
        //------------------------------------

        //bulbaAnimImg.ResetOriginalPos();
        //localBulbaAnimImg.ResetOriginalPos(); //! !! (this is using global position instead of local)

        //bulbaAnimImg.ReturnToOriginalPos(bulbaOrigPos.x, bulbaOrigPos.y, 3f);
        //yield return localBulbaAnimImg.ReturnToOriginalPosCoroutine(localBulbaOrigPos.x, localBulbaOrigPos.y, 3f);
        
    }

    IEnumerator AnimMove()
    {
        yield return null;
        
        Debug.Log("char: " + charmander.gameObject.transform.position);
        Vector2 coords = new Vector2(charmander.gameObject.transform.position.x, charmander.gameObject.transform.position.y);
        float speed = 1f;
        float threshold = Mathf.Epsilon;

        bool moving = true;
        while(moving)
        {
            bulba.transform.position = Vector2.MoveTowards(bulba.transform.position, coords, speed);
            float distance = Vector2.Distance(bulba.transform.position, coords);
            if(distance < threshold)
            {
                moving = false;
            }
            yield return null;
        }
        bulba.transform.position = coords;

    }

    //!convert to use transform.localPosition OR/AND write alt function to do this
    IEnumerator LocalAnimMove()
    {
        yield return null;

        Debug.Log("localChar: " + localChar.gameObject.transform.position);
        Vector2 coords = new Vector2(localChar.gameObject.transform.position.x, localChar.gameObject.transform.position.y);
        float speed = 1f;
        float threshold = Mathf.Epsilon;

        bool moving = true;
        while(moving)
        {
            localBulba.transform.position = Vector2.MoveTowards(localBulba.transform.position, coords, speed);
            float distance = Vector2.Distance(localBulba.transform.position, coords);
            if(distance < threshold)
            {
                moving = false;
            }
            yield return null;
        }
        localBulba.transform.position = coords;
    }
}
