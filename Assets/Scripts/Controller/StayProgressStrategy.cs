using System;
using Controller.Player;
using UnityEngine;

public class StayProgressStrategy : ILockInteractStrategy
{
    private MapLock lockView;
    private PlayerController player;

    private float currentProgress;

    private UnityEngine.Vector2 lastPlayerPos;
    private bool interacting;

    public StayProgressStrategy()
    {
    }

    public bool IsFinished => currentProgress >= lockView.mapLockData.needMoney;


    public void OnExit()
    {
        interacting = false;
        lockView.SaveProgress(currentProgress);
    }

    public void OnEnter(object lockView, PlayerController player , Transform transform)
    {
         if(lockView is MapLock)
        {
        this.lockView = (MapLock)lockView;
        this.player = player;
        currentProgress = this.lockView.LoadProgress();
        lastPlayerPos = player.transform.position;   
        }
    }
}
