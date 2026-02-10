using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class gotoTest_1 : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public void SwitchScene()
    {
        SceneManager.LoadScene("Test_1", LoadSceneMode.Single);
    }
}