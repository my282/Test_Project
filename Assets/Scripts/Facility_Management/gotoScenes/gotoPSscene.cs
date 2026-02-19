using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class gotoPS : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public void SwitchScene()
    {
        // phishing_siteを所有しているかチェック
        if (GameDatabase.Instance.HasFacility("phishing_site"))
        {
            SceneManager.LoadScene("Facility_PS_upgrade", LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene("Facility_PS", LoadSceneMode.Single);
        }
    }
}