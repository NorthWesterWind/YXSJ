using System;
using Controller.Player;
using UnityEngine;

public interface ILockInteractStrategy
{
    void OnEnter(object lockView, PlayerController player, Transform transform);
    void OnExit();

    bool IsFinished { get; }
}
