// by Donovan Colen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// controls how resting needs of the patrons starts and changes over time
public class RestMotive : MotiveBase
{
    // tuneables
    [SerializeField]
    private float m_maxEnergy = 100;
    [SerializeField]
    private float m_minEnergy = 10;    // the min amount of energy before they leave
    [SerializeField]
    private float m_energyDecayAmount = 1;      // the amount energy decays over time
    [SerializeField]
    private float m_energyReplenishAmount = 1;  // the amount energy replenishes while resting over time
    [SerializeField]
    private float m_restingPenalty = 1;  // the penalty to hunger decay while resting over time (multiplicative)
    [SerializeField]
    private AnimationCurve m_energyCurve = null;

    // variables
    private float m_energy = 0;
    private GameObject m_restLocation;

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        m_energy = Random.Range(m_maxEnergy / 3, m_maxEnergy);  // set initial energy to a random value

        if (m_energyCurve == null)
        {
            Debug.LogError("invalid energy curve");
        }

    }

    public float EnergyDecayAmount
    {
        get { return m_energyDecayAmount; }
        set { m_energyDecayAmount = value; }
    }

    public float Energy
    {
        get { return m_energy; }
        set { m_energy = value; }
    }

    public override GameObject GetLocation()
    {
        return m_restLocation;
    }

    // controls the energy recovery speed while resting
    public override bool DoMotive()
    {
        m_restLocation.GetComponent<SpaceManager>().Taken = true;
        if (m_energy < m_maxEnergy)
        {
            Vector3 temp = m_restLocation.transform.position + m_restLocation.transform.forward;
            temp.y = gameObject.transform.position.y;
            gameObject.transform.LookAt(temp);
            m_energy += m_energyReplenishAmount * Time.deltaTime;
            gameObject.GetComponent<EatMotive>().DecayMotive(m_restingPenalty);
            gameObject.GetComponent<SocialMotive>().DecayMotive(m_restingPenalty);
            return true;
        }
        else
        {
            m_restLocation.GetComponent<SpaceManager>().Taken = false;
            m_restLocation.GetComponent<SpaceManager>().Reserved = false;
            return false;
        }
    }

    // slowly decays the energy of the patron thus increasing the need to rest
    public override void DecayMotive(float penalty = 1)
    {
        m_energy -= m_energyDecayAmount * Time.deltaTime * penalty;
        if (m_energy <= 0)
        {
            m_energy = 0;
        }

    }

    // returns true when the patron is out of energy and ready to leave the mall
    public override bool ReadyToLeave()
    {
        return m_energy <= m_minEnergy;
    }

    // strats the resting motive and reserves a space to rest
    public override Vector3 StartMotive()
    {
        m_restLocation.GetComponent<SpaceManager>().Reserved = true;
        return m_restLocation.transform.position;
    }

    // returns the amount a patrons wants to rest based on the utility curve
    public override float ScoreMotive()
    {
        bool oneAvailable = false;
        float smallestDist = Mathf.Infinity;
        for (int i = 0; i < Game_Manager.Instance.RestAreas.Length; ++i)    // go through all the rest areas
        {
            if (Game_Manager.Instance.RestAreas[i].GetComponent<AvalibleScript>().HasAvalibility() == true) // if it is not taken
            {
                float dist = Vector3.Distance(transform.position, Game_Manager.Instance.RestAreas[i].transform.position);  // get the distance to the rest area
                if (dist < smallestDist) // find the smaller dist
                {
                    smallestDist = dist;
                    oneAvailable = true;
                    m_restLocation = Game_Manager.Instance.RestAreas[i].GetComponent<AvalibleScript>().GetAvalibleSpot();
                }
            }
        }
        if (oneAvailable)
        {
            //float exponent = m_energy - 50;   // old exponential formula that was used before the animation curves
            //float a = 1.1161f;
            //float temp1 = Mathf.Pow(a, exponent);
            //float desire = 1 / (1 + temp1);
            return m_energyCurve.Evaluate(m_energy);
        }
        else
        {
            return 0;    // try again next frame
        }

    }
}
