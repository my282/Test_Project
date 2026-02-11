using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class build_FacilityA : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public void build()
    {
        GameDatabase.Instance.UnlockFacilityWithCost("FacilityA");
    }
}