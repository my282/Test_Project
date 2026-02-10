using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class backtoHub : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public void SwitchScene()
    {
        SceneManager.LoadScene("hub_page", LoadSceneMode.Single);
    }
}