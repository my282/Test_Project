using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class startGame: MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public void start_game()
    {
        SceneManager.LoadScene("hub_page", LoadSceneMode.Single);
        TimerManager.Instance.StartTimer();
    }
}