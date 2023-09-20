using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] GameEvent onGoalReached;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("Sending Goal Event");
        onGoalReached.Raise(this, "Goal Reached mf"); //GM
    }
}