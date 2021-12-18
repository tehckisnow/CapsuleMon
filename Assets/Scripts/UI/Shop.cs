using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Shop : MonoBehaviour
{
    [SerializeField] string shopName = "MonMart";
    public string ShopName => shopName;
    [SerializeField] List<ShopItem> items;
    public List<ShopItem> Items => items;

    public void Activate()
    {
        GameController.Instance.OpenShopUI(this);
    }

    //when called in inspector, this gives time for invisible shopkeepers to call Character.FacePlayer()
    public void ActivateWithDelay(float time)
    {
        StartCoroutine(ActivateDelayCoroutine(time));
    }

    IEnumerator ActivateDelayCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        Activate();
    }
}

[System.Serializable]
public class ShopItem
{
    [SerializeField] ItemBase item;
    [SerializeField] int value;

    public ItemBase Item => item;
    public int Value => value;
}