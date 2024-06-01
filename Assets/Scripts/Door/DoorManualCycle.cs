using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorManualCycle : DoorManual
{
    [Range(600, 1200)]  //(10분-20분)
    public float cycle = 1200.0f;
    public float openPeriod = 300.0f; //5분

    float cycleTime = 0.0f;
    float periodTime = 0.0f;
    DoorState state = DoorState.Close;

    enum DoorState 
    {
        Open,
        Close
    }

    void Update()
    {
        switch (state) 
        {
            case DoorState.Open:
                cycleTime = 0.0f;
                periodTime += Time.time;
                if (periodTime > openPeriod) 
                {
                    state = DoorState.Close;
                }
                break;
            case DoorState.Close:
                periodTime = 0.0f;
                cycleTime += Time.time;
                if (cycleTime > cycle)
                {
                    state = DoorState.Open;
                }
                break;
        }
    }

    public override void Interect()
    {
        Open();
    }

    protected override bool preOpen()
    {
        bool result = (state == DoorState.Open);
        return result;
    }

    protected override void Open()
    {
        if (preOpen())
        {
            GameManager.Instance.EndGame("MainMenuScene");
        }
        else 
        {
            Debug.Log("Not In Time");
        }
    }
}
