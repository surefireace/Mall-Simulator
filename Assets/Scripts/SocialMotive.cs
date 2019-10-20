// by Donovan Colen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// controls how social needs of the patrons starts and changes over time
public class SocialMotive : MotiveBase
{
    // tuneables
    [SerializeField]
    private float m_maxSocial = 100;    // the max amount of social the patrons can handle 
    [SerializeField]
    private float m_minSocial = 10;    // the min amount of social before they leave
    [SerializeField]
    private float m_socialDecayAmount = 1;      // the amount social decays over time
    [SerializeField]
    private float m_socialReplenishAmount = 1;  // the amount social replenishes while eating over time
    [SerializeField]
    private float m_socializeTolerance = 50.0f;    // the amount of socializeing they can take in one sitting before they stop socializeing
    [SerializeField]
    private float m_socialToleranceVariation = 25.0f;    // the amount of variation in the tolerance. this is to help replicate introverts and extroverts
    [SerializeField]
    private float m_socializePenalty = .01f;  // the penalty to energy and hunger decay while socializeing over time (multiplicative)
    [SerializeField]
    private float m_waitTime = 5.0f;    // the amout of time they will wait for someone else to show up before doing something else
    [SerializeField]
    private AnimationCurve m_socialCurve = null;    // the curve that dictates how much the patron wants to socialize as its social value changes

    // variables
    private float m_beginSocial = 0;
    private float m_social = 0;
    private float m_waitTimer = 0;

    private GameObject m_socialLocation;

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        m_social = Random.Range(m_maxSocial / 3, m_maxSocial);  // set initial social to a random value

        // set social tolerance to a random value based on the current tolerance and it variation to simulate different social types amoung the shoppers
        m_socializeTolerance = Random.Range(m_socializeTolerance - m_socialToleranceVariation, m_socializeTolerance + m_socialToleranceVariation);

        if (m_socialCurve == null)
        {
            Debug.LogError("invalid hunger curve");
        }
    }

    public float Social
    {
        get { return m_social; }
        set { m_social = value; }
    }

    // finds if the patron is actually socializing and replenishes their social need acordingly
    public override bool DoMotive()
    {
        m_socialLocation.GetComponent<SpaceManager>().Taken = true;
        if (m_social - m_beginSocial < m_socializeTolerance && m_social <= m_maxSocial && m_waitTimer < m_waitTime)
        {
            if((m_socialLocation.GetComponentInParent<SocialManager>().GetCurrGroupAmount() - 1) == 0)
            {
                m_waitTimer += Time.deltaTime;
            }
            else
            {
                m_waitTimer = 0;
            }
            Vector3 temp = m_socialLocation.transform.position + m_socialLocation.transform.forward;
            temp.y = gameObject.transform.position.y;
            gameObject.transform.LookAt(temp);
            m_social += m_socialReplenishAmount * Time.deltaTime * (m_socialLocation.GetComponentInParent<SocialManager>().GetCurrGroupAmount() - 1);   // shouldnt replenish social if only one in group
            gameObject.GetComponent<RestMotive>().DecayMotive(m_socializePenalty);
            gameObject.GetComponent<EatMotive>().DecayMotive(m_socializePenalty);
            return true;
        }
        else
        {
            m_waitTimer = 0;
            m_socialLocation.GetComponent<SpaceManager>().Taken = false;
            m_socialLocation.GetComponent<SpaceManager>().Reserved = false;
            m_socialLocation.GetComponentInParent<SocialManager>().Leave(this.gameObject);
            return false;
        }
    }

    // slowly decays the social value of the patron thus increasing the need to socialize
    public override void DecayMotive(float penalty = 1)
    {
        m_social -= m_socialDecayAmount * Time.deltaTime * penalty;
        if (m_social <= 0)
        {
            m_social = 0;
        }
    }

    // returns if the patron wants to leave because lack of socializing
    public override bool ReadyToLeave()
    {
        return (m_social <= m_minSocial);
    }

    // returns the location the patron is socializing at
    public override GameObject GetLocation()
    {
        return m_socialLocation;
    }

    // starts the act of socializing
    public override Vector3 StartMotive()
    {
        m_socialLocation.GetComponentInParent<SocialManager>().Join(this.gameObject);
        m_socialLocation.GetComponent<SpaceManager>().Reserved = true;
        m_beginSocial = m_social;
        return m_socialLocation.transform.position;
    }

    // returns the amount a patrons wants to socialize based on the utility curve
    public override float ScoreMotive()
    {
        bool oneAvailable = false;
        int mostAmount = 0;
        for (int i = 0; i < Game_Manager.Instance.Social.Length; ++i)    // go through all the social areas
        {
            if (Game_Manager.Instance.Social[i].GetComponent<AvalibleScript>().HasAvalibility() == true) // if it is not full
            {
                int amount = Game_Manager.Instance.Social[i].GetComponent<SocialManager>().GetCurrGroupAmount();
                if (amount >= mostAmount) // find the area with the most patrons
                {
                    oneAvailable = true;
                    mostAmount = amount;
                    m_socialLocation = Game_Manager.Instance.Social[i].GetComponent<AvalibleScript>().GetAvalibleSpot();
                }
            }
        }
        if (oneAvailable)
        {
            return m_socialCurve.Evaluate(m_social);
        }
        else
        {
            return 0;    // try again next frame
        }
    }

}
