using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class get_itemA : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public void get()
    {
        GameDatabase.Instance.AddItem("itemA", "itemA", "itemA", 1, ItemType.Other);
    }
}