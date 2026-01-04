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
         player.InteractionTriggerInRange = false;
    }

  

    private bool IsPlayerStanding()
    {
        UnityEngine.Vector2 currentPos = player.transform.position;
        bool moving = UnityEngine.Vector2.Distance(currentPos, lastPlayerPos) > 0.01f;
        lastPlayerPos = currentPos;
        return !moving;
    }

    public void OnEnter(object lockView, PlayerController player)
    {
         if(lockView is MapLock)
        {
        this.lockView = (MapLock)lockView;
        this.player = player;
        currentProgress = this.lockView.LoadProgress();
        lastPlayerPos = player.transform.position;   
         player.InteractionTriggerInRange = true;
        }
    }
}
