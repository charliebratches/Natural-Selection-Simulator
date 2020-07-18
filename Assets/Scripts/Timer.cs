using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public DayManager dayManager;
    public float timeLeft;
    public TextMeshProUGUI timer;
    public bool enableTimer;

    void Start()
    {
        enableTimer = true;
    }

    void Update()
    {
        if (!enableTimer)
        {
            return;
        }
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            timer.text = (Math.Truncate(100 * timeLeft) / 100).ToString();
        }
        else
        {
            timeLeft = 0;
            enableTimer = false;
            timer.text = (Math.Truncate(100 * timeLeft) / 100).ToString();
            StartCoroutine(dayManager.EndDay());
        }
    }
}
