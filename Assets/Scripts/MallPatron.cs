// by Donovan Colen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

// needed components for the class to work
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ShopMotive))]
[RequireComponent(typeof(EatMotive))]
[RequireComponent(typeof(RestMotive))]
[RequireComponent(typeof(LeaveMotive))]
[RequireComponent(typeof(SocialMotive))]

// the main class that is the patron and controls its actions
public class MallPatron : MonoBehaviour
{
    //tuneables
    [SerializeField]
    private float m_speedVariation = 2;

    // variables
    private Motives m_currentMotive = Motives.kNone;
    private NavMeshAgent m_navAgent = null;
    private Motives m_previousChoice = Motives.kNone;
    private ShopMotive m_shopMotive = null;
    private EatMotive m_eatMotive = null;
    private RestMotive m_restMotive = null;
    private LeaveMotive m_leaveMotive = null;
    private SocialMotive m_socialMotive = null;
    private MotiveBase[] m_motives = null;
    private bool m_left = false;
    private float m_shopRadius = 5;
    private float m_hungerScore = 0;
    private float m_energyScore = 0;
    private float m_shopScore = 0;
    private float m_leaveScore = 0;
    private float m_socialScore = 0;

    // debug stuff
    private int m_histSize = 5;
    private HistoryFrame[] m_history;

    // a struct representing the past of the patrons choices 
    public struct HistoryFrame
    {
        public float m_hunger;
        public float m_energy;
        public float m_social;
        public float m_items;
        public float m_hungerScore;
        public float m_energyScore;
        public float m_socialScore;
        public float m_shopScore;
        public float m_leaveScore;
        public string m_choice;
        
        public HistoryFrame(float hunger, float energy, float social, float items, float hungerScore, float energyScore, float socialScore, float shopScore, float leaveScore, string choice)
        {
            m_hunger = hunger;
            m_energy = energy;
            m_social = social;
            m_items = items;
            m_hungerScore = hungerScore;
            m_energyScore = energyScore;
            m_socialScore = socialScore;
            m_shopScore = shopScore;
            m_leaveScore = leaveScore;
            m_choice = choice;
        }
    }

    // the different motives the patron wants to do at the mall
    private enum Motives
    {
        kShop = 0,
        kEat = 1,
        kRest = 2,
        kLeave = 3,
        kSocialize = 4,

        kCount,
        kNone,
    }

    // class used to put a utility score to the motive
    private class MotiveValuePair : IComparable<MotiveValuePair>
    {
        public Motives mot;
        public float val;

        public MotiveValuePair(Motives newMot, float newVal)
        {
            mot = newMot;
            val = newVal;
        }

        public int CompareTo(MotiveValuePair obj)
        {
            return (int)((val - obj.val) * 100);
        }
    }


    // Use this for initialization
    void Start()
    {
        m_navAgent = GetComponent<NavMeshAgent>();
        m_shopMotive = GetComponent<ShopMotive>();
        m_eatMotive = GetComponent<EatMotive>();
        m_restMotive = GetComponent<RestMotive>();
        m_leaveMotive = GetComponent<LeaveMotive>();
        m_socialMotive = GetComponent<SocialMotive>();

        // simulate different walking speeds
        float tempSpeed = UnityEngine.Random.Range(m_navAgent.speed - m_speedVariation, m_navAgent.speed + m_speedVariation);
        m_navAgent.speed = tempSpeed;
        m_motives = new MotiveBase[] { m_shopMotive, m_eatMotive, m_restMotive, m_leaveMotive, m_socialMotive };

        if (!m_navAgent)
        {
            Debug.LogError("no nav mesh agent component");
        }

        m_history = new HistoryFrame[m_histSize];
    }

    public bool Left
    {
        get { return m_left; }
        set { m_left = value; }
    }

    public float HungerScore
    {
        get { return m_hungerScore; }
    }

    public float EnergyScore
    {
        get { return m_energyScore; }
    }

    public float ShopScore
    {
        get { return m_shopScore; }
    }

    public float LeaveScore
    {
        get { return m_leaveScore; }
    }

    public float SocialScore
    {
        get { return m_socialScore; }
    }

    // returns the currents motives name
    public string GetCurrentMotive()
    {
        string motive = m_currentMotive.ToString();
        return motive.Replace("k", string.Empty);   // to remove the k at the begining of the motive
    }

    // returns the history of the patrons actions
    public HistoryFrame[] GetHistory()
    {
        return m_history;
    }

    // retuns the current action that the patrons is doing
    public string GetCurrentAction()
    {
        if(m_left)
        {
            return "Leaving";
        }
        if(AtLocation())
        {
            string motive = m_currentMotive.ToString();
            motive = motive.Replace("k", string.Empty);   // to remove the k at the begining of the motive
            if(motive == "Shop")
            {
                return motive + "ping";
            }
            if(motive == "Socialize")
            {
                motive = motive.Replace("e", string.Empty);   // to remove the e at the end of the motive
                return motive + "ing";
            }

            return motive + "ing";
        }
        if(m_currentMotive == Motives.kNone)
        {
            return "Nothing";
        }
        else
        {
            string action = "Moving To " + m_motives[(int)m_currentMotive].GetLocation().name;

            return action;
        }
    }

    // adds the history to the array
    private void AddHistory(HistoryFrame newHistory)
    {
        for(int i = m_histSize - 1; i > 0; --i)
        {
            m_history[i] = m_history[i - 1];
        }

        m_history[0] = newHistory;
    }

    // returns true when the patron wants to leave the mall
    private bool ReadyToLeave()
    {
        for(int i = 0; i < m_motives.Length; ++i)
        {
            if(m_motives[i].ReadyToLeave())
            {
                return true;
            }
        }
        return false;
    }

    // decays all the motives
    private void DecayMotives()
    {
        for (int i = 0; i < m_motives.Length; ++i)
        {
            m_motives[i].DecayMotive();
        }
    }

    private void ChooseMotive()  // picks a motive randomly and does it if posible
    {
        if (ReadyToLeave())
        {
            m_currentMotive = Motives.kLeave;
            m_navAgent.SetDestination(m_motives[(int)m_currentMotive].StartMotive());

            HistoryFrame historyFrame = new HistoryFrame((m_motives[(int)Motives.kEat] as EatMotive).Hunger,
                (m_motives[(int)Motives.kRest] as RestMotive).Energy, (m_motives[(int)Motives.kSocialize] as SocialMotive).Social,
                (m_motives[(int)Motives.kShop] as ShopMotive).GetItemsToBuy(), 0, 0, 0, 0, 1, GetCurrentMotive());

            AddHistory(historyFrame);
        }
        else
        {
            m_hungerScore = m_motives[(int)Motives.kEat].ScoreMotive();
            m_energyScore = m_motives[(int)Motives.kRest].ScoreMotive();
            m_shopScore = m_motives[(int)Motives.kShop].ScoreMotive();
            m_leaveScore = m_motives[(int)Motives.kLeave].ScoreMotive();
            m_socialScore = m_motives[(int)Motives.kSocialize].ScoreMotive();

            List<MotiveValuePair> desireOrder = new List<MotiveValuePair>
            {
                new MotiveValuePair(Motives.kEat, m_hungerScore), new MotiveValuePair(Motives.kRest, m_energyScore),
                new MotiveValuePair(Motives.kShop, m_shopScore), new MotiveValuePair(Motives.kLeave, m_leaveScore),
                new MotiveValuePair(Motives.kSocialize, m_socialScore)
            };

            desireOrder.Sort();

            m_currentMotive = desireOrder[desireOrder.Count - 1].mot;
            float choice = desireOrder[desireOrder.Count - 1].val;
            
            if(m_currentMotive == m_previousChoice && (choice - desireOrder[desireOrder.Count - 2].val) <= .1) // if choice is the same as last choice and the desire difference between second best is less then .1
            {
                m_currentMotive = desireOrder[desireOrder.Count - 2].mot;    // choose the next best thing

            }
            m_previousChoice = m_currentMotive;


            HistoryFrame historyFrame = new HistoryFrame((m_motives[(int)Motives.kEat] as EatMotive).Hunger,
                (m_motives[(int)Motives.kRest] as RestMotive).Energy, (m_motives[(int)Motives.kSocialize] as SocialMotive).Social,
                (m_motives[(int)Motives.kShop] as ShopMotive).GetItemsToBuy(), m_hungerScore, m_energyScore, m_socialScore,
                 m_shopScore, m_leaveScore, GetCurrentMotive());

            AddHistory(historyFrame);

            m_navAgent.SetDestination(m_motives[(int)m_currentMotive].StartMotive());
        }
    }

    // returns true if the patron reaches the location for them to do the motive they chose
    private bool AtLocation()
    {
        if (m_currentMotive == Motives.kShop)
        {
            if (Vector3.Distance(m_navAgent.destination, transform.position) < m_shopRadius)
            {
                return true;
            }
            return false;

        }
        else
        {
            if (m_navAgent.remainingDistance < m_navAgent.stoppingDistance)
            {
                if (Vector3.Distance(m_navAgent.destination, transform.position) < m_navAgent.stoppingDistance + 1)
                {
                    return true;
                }
            }
            return false;
        }

    }


    // Update is called once per frame
    void Update()
    {
        float temp = (m_motives[(int)Motives.kRest] as RestMotive).EnergyDecayAmount;
        (m_motives[(int)Motives.kRest] as RestMotive).EnergyDecayAmount = (m_motives[(int)Motives.kRest] as RestMotive).EnergyDecayAmount * (1 + m_motives[(int)Motives.kLeave].ScoreMotive()); // take into account frustration

        if (m_currentMotive == Motives.kNone)    // if no motive get a motive
        {
            ChooseMotive();
            DecayMotives();
        }
        else if(m_currentMotive == Motives.kLeave && AtLocation()) // if wanting to leave and at door leave
        {
            if(m_motives[(int)m_currentMotive].DoMotive() == false)
            {
                m_currentMotive = Motives.kNone;
            }
        }
        else
        {
            if (AtLocation()) // do the apropriate task when at location
            {
                if (m_motives[(int)m_currentMotive].DoMotive() == false)
                {
                    m_currentMotive = Motives.kNone;
                }
            }
            else
            {
                // standard over time decay
                DecayMotives();
            }

        }
        (m_motives[(int)Motives.kRest] as RestMotive).EnergyDecayAmount = temp;
    }
}
