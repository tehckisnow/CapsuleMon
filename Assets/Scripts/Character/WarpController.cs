using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpController : MonoBehaviour
{
    [SerializeField] private Vector2 lastWarpPoint;

    private Character character;

    private void Start()
    {
        character = GetComponent<Character>();
        SetWarpPoint(transform);
    }

    public void GoToLastWarp()
    {
        character.SetPositionAndSnapToTile(lastWarpPoint);
    }

    public IEnumerator GoToLastWarpAnim()
    {
        yield return Fader.Instance.FadeIn(2f);
        character.SetPositionAndSnapToTile(lastWarpPoint);
        FindObjectOfType<HealMons>().HealParty(MonParty.GetPlayerParty());
        yield return Fader.Instance.FadeOut(2f);
    }

    public void SetWarpPoint(Vector2 warpPoint)
    {
        lastWarpPoint = warpPoint;
    }

    public void SetWarpPoint(Transform transform)
    {
        lastWarpPoint = transform.position;
    }
}
