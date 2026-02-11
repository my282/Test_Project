using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class template: MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public void SwitchScene()
    {
        SceneManager.LoadScene("ここにシーン名を入力", LoadSceneMode.Single);
    }
}