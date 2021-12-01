using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPoint : MonoBehaviour
{
    public void SetPoint()
    {
        var warpController = PlayerController.Instance.GetComponent<WarpController>();
        warpController.SetWarpPoint(gameObject.transform.position);
    }
}
