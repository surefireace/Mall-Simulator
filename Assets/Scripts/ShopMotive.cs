// by Donovan Colen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// controls how shopping needs of the patrons starts and changes over time
public class ShopMotive : MotiveBase
{
    // tuneables
    [SerializeField]
    private int m_minShopItems = 1;     // the minimum number of random items the shopper will want to buy
    [SerializeField]
    private int m_maxShopItems = 10;     // the maximum number of random items the shopper will want to buy
    [SerializeField]
    private float m_maxShopDesire = 0.8f;     // the maximum amount of desire a person can have to shop. must be less than one
    [SerializeField]
    private float m_shopTime = 5.0f;    // the amount of time it takes to shop
    [SerializeField]
    private float m_shoppingPenalty = 1;  // the penalty to energy and hunger decay while shopping over time (multiplicative)

    // variables
    private int m_itemsToBuy = 0;
    private float m_shopTimer = 0;
    private GameObject m_shopLocation;

    // Use this for initialization
    public override void Start()
    {
        base.Start();

        m_itemsToBuy = Random.Range(m_minShopItems, m_maxShopItems + 1);

    }

    // returns how many items the patron wants to buy
    public int GetItemsToBuy()
    {
        return m_itemsToBuy;
    }

    // controls how the patron shops and how the other needs are effected during shopping
    public override bool DoMotive()
    {
        m_shopTimer += Time.deltaTime;
        if (m_shopTimer < m_shopTime)
        {
            gameObject.GetComponent<Renderer>().enabled = false;
            Renderer[] r = gameObject.GetComponentsInChildren<Renderer>();
            for(int i = 0; i < r.Length; ++i)
            {
                r[i].enabled = false;
            }
            gameObject.GetComponent<RestMotive>().DecayMotive(m_shoppingPenalty);
            gameObject.GetComponent<EatMotive>().DecayMotive(m_shoppingPenalty);
            gameObject.GetComponent<SocialMotive>().DecayMotive(m_shoppingPenalty);
            return true;
        }
        else
        {
            --m_itemsToBuy; // one less item to buy
            m_shopTimer = 0;
            gameObject.GetComponent<Renderer>().enabled = true;
            Renderer[] r = gameObject.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < r.Length; ++i)
            {
                r[i].enabled = true;
            }
            return false;
        }
    }

    // returns true when the patron is out of things to buy
    public override bool ReadyToLeave()
    {
        return m_itemsToBuy == 0;
    }

    // returns the store location the patron wants to shop at
    public override GameObject GetLocation()
    {
        return m_shopLocation;
    }

    // starts the act of shopping and chooses a random store to shop at
    public override Vector3 StartMotive()
    {
        int index = Random.Range(0, Game_Manager.Instance.Shops.Length);    // choose random store
        m_shopLocation = Game_Manager.Instance.Shops[index];
        return m_shopLocation.transform.position;
    }

    // returns the amount a patrons wants to shop. standard linear utility curve
    public override float ScoreMotive()
    {
        float desire = (float)(m_itemsToBuy + 1) / m_maxShopItems;
        if (desire > m_maxShopDesire)
        {
            return m_maxShopDesire;
        }
        else
        {
            return desire;
        }
    }
}
