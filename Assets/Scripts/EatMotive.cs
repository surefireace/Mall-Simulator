// by Donovan Colen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// controls how food needs of the patrons starts and changes over time
public class EatMotive : MotiveBase
{
    // tuneables
    [SerializeField]
    private float m_maxHunger = 100;
    [SerializeField]
    private float m_minHunger = 10;    // the min amount of hunger before they leave
    [SerializeField]
    private float m_hungerDecayAmount = 1;      // the amount hunger decays over time
    [SerializeField]
    private float m_hungerReplenishAmount = 1;  // the amount hunger replenishes while eating over time
    [SerializeField]
    private float m_eatingPenalty = 1;  // the penalty to energy decay while eating over time (multiplicative)
    [SerializeField]
    private AnimationCurve m_hungerCurve = null;

    // variables
    private float m_hunger = 0;
    private GameObject m_eatLocation;

    // Use this for initialization
    public override void Start()
    {
        base.Start();

        m_hunger = Random.Range(m_maxHunger / 3, m_maxHunger);  // set initial hunger to a random value

        if (m_hungerCurve == null)
        {
            Debug.LogError("invalid hunger curve");
        }

    }

    public float Hunger
    {
        get { return m_hunger; }
        set { m_hunger = value; }
    }

    // controls the replenishment of patrons food 
    public override bool DoMotive()
    {
        m_eatLocation.GetComponent<SpaceManager>().Taken = false;
        if (m_hunger < m_maxHunger)
        {
            Vector3 temp = m_eatLocation.transform.position + m_eatLocation.transform.forward;
            temp.y = gameObject.transform.position.y;
            gameObject.transform.LookAt(temp);
            m_hunger += m_hungerReplenishAmount * Time.deltaTime;
            gameObject.GetComponent<RestMotive>().DecayMotive(m_eatingPenalty);
            gameObject.GetComponent<SocialMotive>().DecayMotive(m_eatingPenalty);
            return true;
        }
        else
        {
            m_eatLocation.GetComponent<SpaceManager>().Taken = false;
            m_eatLocation.GetComponent<SpaceManager>().Reserved = false;
            return false;
        }
    }


    // slowly decays the hunger value of the patron making the patron hungry
    public override void DecayMotive(float penalty = 1)
    {
        m_hunger -= m_hungerDecayAmount * Time.deltaTime * penalty;
        if (m_hunger <= 0)
        {
            m_hunger = 0;
        }
    }

    // returns true when the patron becomes too hungry to shop
    public override bool ReadyToLeave()
    {
        return m_hunger <= m_minHunger;
    }

    // returns the location the patron is eating at
    public override GameObject GetLocation()
    {
        return m_eatLocation;
    }

    // starts the patron moveing to the eating location
    public override Vector3 StartMotive()
    {
        m_eatLocation.GetComponent<SpaceManager>().Reserved = true;
        return m_eatLocation.transform.position;
    }

    // scores the motive on how badly it wants to eat based of the utility curve
    public override float ScoreMotive()
    {
        bool oneAvailable = false;
        float smallestDist = Mathf.Infinity;
        for (int i = 0; i < Game_Manager.Instance.FoodVendors.Length; ++i)    // go through all the food vendors
        {
            if (Game_Manager.Instance.FoodVendors[i].GetComponent<AvalibleScript>().HasAvalibility() == true) // if it is not taken
            {
                float dist = Vector3.Distance(transform.position, Game_Manager.Instance.FoodVendors[i].transform.position);  // get the distance to the food vendor
                if (dist < smallestDist) // find the smaller dist
                {
                    smallestDist = dist;
                    oneAvailable = true;
                    m_eatLocation = Game_Manager.Instance.FoodVendors[i].GetComponent<AvalibleScript>().GetAvalibleSpot();
                }
            }
        }
        if (oneAvailable)
        {
            //if (0 <= m_hunger && m_hunger < 10)      // old piecewise function used before animation curves were used
            //{
            //    return 1;
            //}
            //else if (10 <= m_hunger && m_hunger < 25)
            //{
            //    return 0.9f;
            //}
            //else if (25 <= m_hunger && m_hunger < 40)
            //{
            //    return 0.75f;
            //}
            //else if (40 <= m_hunger && m_hunger < 60)
            //{
            //    return 0.5f;
            //}
            //else if (60 <= m_hunger && m_hunger < 80)
            //{
            //    return 0.2f;
            //}
            //else// if (80 <= m_hunger && m_hunger < 100)
            //{
            //    return 0;
            //}
            return m_hungerCurve.Evaluate(m_hunger);
        }
        else
        {
            return 0;    // try again next frame
        }
    }
}
