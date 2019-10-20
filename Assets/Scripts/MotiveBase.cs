// by Donovan Colen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// the base class for all the other motives
public abstract class MotiveBase : MonoBehaviour
{
    // Use this for initialization
    public virtual void Start()
    {
        // empty on purpose
    }

    // returns false when the motive is done
    public abstract bool DoMotive();

    // starts the patron doing that motive they choose
    public abstract Vector3 StartMotive();

    // returns true when the patron wants to leave the mall
    public abstract bool ReadyToLeave();

    // returns the want value that the patron wants to do that action
    public abstract float ScoreMotive();

    // returns the location of where the patron is or is heading
    public abstract GameObject GetLocation();

    // decays the motive values over time
    public virtual void DecayMotive(float penalty = 1) { }

}
