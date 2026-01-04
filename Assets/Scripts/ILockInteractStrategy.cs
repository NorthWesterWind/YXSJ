using System;
using Controller.Player;

public interface ILockInteractStrategy
{
    void OnEnter(Object lockView, PlayerController player);
    void OnExit();

    bool IsFinished { get; }
}
