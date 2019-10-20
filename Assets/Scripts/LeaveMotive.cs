// by Donovan Colen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// controls when and if the patron wants to leave
public class LeaveMotive : MotiveBase
{
    [SerializeField]
    private float m_mallCapacityTolerance = .8f;    // the percent of mall capacity the shopper will tolerate

    private GameObject m_leaveLocation;

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        m_leaveLocation = Game_Manager.Instance.Entrance;
    }

    // returns false because the patron left the mall
    public override bool DoMotive()
    {
        gameObject.GetComponent<MallPatron>().Left = true;
        Game_Manager.Instance.LeaveMall(gameObject);
        return false;
    }

    //the patron is never ready to leave because they just leave
    public override bool ReadyToLeave()
    {
        return false;
    }

    // retuns the leaveing location of the mall
    public override GameObject GetLocation()
    {
        return m_leaveLocation;
    }

    // starts the patron to leaveing the mall
    public override Vector3 StartMotive()
    {
        return m_leaveLocation.transform.position;
    }

    // scores how much the patron wants to leave based on how crowded the mall is
    public override float ScoreMotive()
    {
        float currCapacity = Game_Manager.Instance.GetMallPopPercent();

        if (currCapacity > m_mallCapacityTolerance)
        {
            return 1f;
        }
        else
        {
            float exponent = .2f;
            float b = -(m_mallCapacityTolerance * 100);
            float temp = ((currCapacity * 100) + b) / b;
            temp = Mathf.Pow(temp, exponent);
            return 1 - temp;
        }
    }
}
