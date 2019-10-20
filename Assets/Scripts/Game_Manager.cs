// by Donovan Colen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// the manager for all the patrons spawning and removeing
public class Game_Manager : MonoBehaviour
{
    // singleton
    private static Game_Manager s_singleton = null;

    //tuneables
    [SerializeField]
    private int m_maxNumShoppers = 15;  // the maximum number of shoppers in the mall at a given time
    [SerializeField]
    private int m_minNumShoppers = 10;  // the minimum amount of shoppers in the mall before more can start spawning
    [SerializeField]
    private float m_maxShopperDelay = 15.0f;    // the max amount of time it takes for anouther shopper to spawn
    [SerializeField]
    private float m_minShopperDelay = 5.0f;     // the min amount of time it takes for anouther shopper to spawn
    [SerializeField]
    private float m_mallPopulationLimit = 20;   // the maximum number of shoppers the mall can hold
    [SerializeField]
    private GameObject m_mallPatron = null;
    [SerializeField]
    private GameObject m_mallEntrance = null;   // the door to enter and leave the mall
    [SerializeField]
    private Color m_selectedColor = Color.white;

    // variables
    private int m_numShoppers = 0;
    private List<GameObject> m_mallPatrons;     // list of all the current shoppers
    private GameObject m_selectedMallPatron = null;
    private GameObject[] m_shops = null;        // holds all the shops in the level
    private GameObject[] m_foodVendors = null;  // holds all the food vendors in the level
    private GameObject[] m_restAreas = null;    // holds all the rest areas in the level
    private GameObject[] m_social = null;        // holds all the social areas in the level
    private float m_spawnTimer = 0;
    private Color m_color;
    private bool m_initSpawnDone = false;
    private GameObject m_spawnedObjects = null;

    // UI stuff
    private Slider m_hungerBar = null;
    private Slider m_energyBar = null;
    private Slider m_socialBar = null;
    private Text m_hungerText = null;
    private Text m_energyText = null;
    private Text m_socialText = null;
    private Text m_numShoppersText = null;
    private Text m_selectedPatronText = null;
    private Text m_motiveText = null;
    private Text m_numItemsText = null;

    // Debug stuff
    bool m_debugInfo = false;
    private Text m_actionText = null;
    private Text m_choiceText = null;
    private GameObject m_histButton = null;
    private Text m_historyText = null;


    // Use this for initialization
    void Start()
    {
        if(s_singleton == null)
        {
            s_singleton = this;
        }

        m_mallPatrons = new List<GameObject>();
        GameObject[] placedPatrons = GameObject.FindGameObjectsWithTag("Shopper");
        m_spawnedObjects = new GameObject("SpawnedPatrons");

        for(int i = 0; i < placedPatrons.Length; ++i)
        {
            m_mallPatrons.Add(placedPatrons[i]);
            ++m_numShoppers;
        }

        m_social = GameObject.FindGameObjectsWithTag("SocialArea");
        m_shops = GameObject.FindGameObjectsWithTag("Shop");
        m_restAreas = GameObject.FindGameObjectsWithTag("RestArea");
        m_foodVendors = GameObject.FindGameObjectsWithTag("FoodVendor");

        if (!m_mallPatron)
        {
            Debug.LogError("there is no mall patron prefab selected");
        }

        if (!m_mallEntrance)
        {
            Debug.LogError("there is no mall entrance selected");
        }

        // UI stuff
        m_hungerBar = GameObject.Find("HUD/HungerSlider").GetComponent<Slider>();
        m_energyBar = GameObject.Find("HUD/EnergySlider").GetComponent<Slider>();
        m_socialBar = GameObject.Find("HUD/SocialSlider").GetComponent<Slider>();
        m_hungerText = GameObject.Find("HUD/HungerText").GetComponent<Text>();
        m_energyText = GameObject.Find("HUD/EnergyText").GetComponent<Text>();
        m_socialText = GameObject.Find("HUD/SocialText").GetComponent<Text>();
        m_numShoppersText = GameObject.Find("HUD/ShoppersText").GetComponent<Text>();
        m_selectedPatronText = GameObject.Find("HUD/PatronText").GetComponent<Text>();
        m_motiveText = GameObject.Find("HUD/MotiveText").GetComponent<Text>();
        m_numItemsText = GameObject.Find("HUD/ItemsText").GetComponent<Text>();
        m_actionText = GameObject.Find("HUD/ActionText").GetComponent<Text>();
        m_choiceText = GameObject.Find("HUD/ChoiceText").GetComponent<Text>();
        m_histButton = GameObject.Find("HUD/HistoryButton");
        m_historyText = GameObject.Find("HUD/HistoryText").GetComponent<Text>();
        m_histButton.SetActive(false);

        //Error checking
        if (!m_hungerBar)
        {
            Debug.LogError("could not find hunger's slider");
        }

        if (!m_energyBar)
        {
            Debug.LogError("could not find energy's slider");
        }

        if (!m_socialBar)
        {
            Debug.LogError("could not find social's slider");
        }

        if (!m_hungerText)
        {
            Debug.LogError("could not find hunger's text");
        }

        if (!m_energyText)
        {
            Debug.LogError("could not find energy's text");
        }

        if (!m_socialText)
        {
            Debug.LogError("could not find social's text");
        }

        if (!m_numShoppersText)
        {
            Debug.LogError("could not find Shopper's text");
        }

        if (!m_selectedPatronText)
        {
            Debug.LogError("could not find patrons's text");
        }

        if (!m_motiveText)
        {
            Debug.LogError("could not find motive text");
        }

        if (!m_numItemsText)
        {
            Debug.LogError("could not find items text");
        }

        if (!m_actionText)
        {
            Debug.LogError("could not find action text");
        }

        if (!m_choiceText)
        {
            Debug.LogError("could not find choice text");
        }

        if (!m_histButton)
        {
            Debug.LogError("could not find history button");
        }

        if (!m_historyText)
        {
            Debug.LogError("could not find history text");
        }


        m_spawnTimer = Random.Range(m_minShopperDelay, m_maxShopperDelay);
    }

    // instance setting and getting
    static public Game_Manager Instance
    {
        get
        {
            if (s_singleton == null)
            {
                s_singleton = GameObject.Find("GameManager").GetComponent<Game_Manager>();
            }
            return s_singleton;
        }
    }

    public GameObject Entrance
    {
        get { return m_mallEntrance; }
    }

    public GameObject[] Social
    {
        get { return m_social; }
    }

    public GameObject[] Shops
    {
        get { return m_shops; }
    }

    public GameObject[] RestAreas
    {
        get { return m_restAreas; }
    }

    public GameObject[] FoodVendors
    {
        get { return m_foodVendors; }
    }

    public float GetMallPopPercent()
    {
        return (m_numShoppers / m_mallPopulationLimit);
    }

    public GameObject SelectedPatron
    {
        get { return m_selectedMallPatron; }
        set { m_selectedMallPatron = value; }
    }

    public void ToggleHistory()
    {
        m_historyText.enabled = !m_historyText.enabled;
    }

    // updates and shows the history of the selected patron
    public void ShowHistory()
    {
        MallPatron.HistoryFrame[] hist = m_selectedMallPatron.GetComponent<MallPatron>().GetHistory();
        string[] t = new string[hist.Length];
        m_historyText.text = "";

        for (int i = 0; i < hist.Length; ++i)
        {
            if(i == 0)
            {
                m_historyText.text += "Latest: ";
            }

            t[i] = "Hunger: " + hist[i].m_hunger.ToString("F2") + " Energy: " + hist[i].m_energy.ToString("F2") + " Social: " + hist[i].m_social.ToString("F2")
                 + " Items: " + hist[i].m_items + "\nHunger Score: " + hist[i].m_hungerScore.ToString("F2") + " Energy Score: " + hist[i].m_energyScore.ToString("F2")
                 + " Social Score: " + hist[i].m_socialScore.ToString("F2") + "\nShop Score: " + hist[i].m_shopScore.ToString("F2") + " Leave Score: " + hist[i].m_leaveScore.ToString("F2")
                 + "\nChoice: " + hist[i].m_choice;

            m_historyText.text += t[i] + "\n\n";
        }

    }

    // updates the UI for the game
    public void UpdateUI()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            m_debugInfo = !m_debugInfo;
        }

        if (m_selectedMallPatron)
        {
            m_hungerBar.value = m_selectedMallPatron.GetComponent<EatMotive>().Hunger;
            m_hungerText.text = "Hunger: ";
            m_energyBar.value = m_selectedMallPatron.GetComponent<RestMotive>().Energy;
            m_energyText.text = "Energy: ";
            m_socialBar.value = m_selectedMallPatron.GetComponent<SocialMotive>().Social;
            m_socialText.text = "Social: ";
            m_selectedPatronText.text = "Selected Patron: " + m_selectedMallPatron.name;
            m_numShoppersText.text = "Number of Shoppers: " + m_numShoppers.ToString();
            m_motiveText.text = "Current Motive: " + m_selectedMallPatron.GetComponent<MallPatron>().GetCurrentMotive();
            m_numItemsText.text = "Items to buy: " + m_selectedMallPatron.GetComponent<ShopMotive>().GetItemsToBuy();
            m_motiveText.enabled = true;
            m_numItemsText.enabled = true;

            if (m_debugInfo)
            {
                m_histButton.SetActive(true);
                m_hungerText.text = "Hunger: " + m_selectedMallPatron.GetComponent<EatMotive>().Hunger.ToString("F2") + ' ';
                m_energyText.text = "Energy: " + m_selectedMallPatron.GetComponent<RestMotive>().Energy.ToString("F2") + ' ';
                m_socialText.text = "Social: " + m_selectedMallPatron.GetComponent<SocialMotive>().Social.ToString("F2") + ' ';
                m_actionText.text = "Action: " + m_selectedMallPatron.GetComponent<MallPatron>().GetCurrentAction();
                m_actionText.enabled = true;
                m_choiceText.text = "Hunger Score: " + m_selectedMallPatron.GetComponent<MallPatron>().HungerScore.ToString("F2") + '\n'
                                  + "Energy Score: " + m_selectedMallPatron.GetComponent<MallPatron>().EnergyScore.ToString("F2") + '\n'
                                  + "Social Score: " + m_selectedMallPatron.GetComponent<MallPatron>().SocialScore.ToString("F2") + '\n'
                                  + "Shop Score: " + m_selectedMallPatron.GetComponent<MallPatron>().ShopScore.ToString("F2") + '\n'
                                  + "Leave Score: " + m_selectedMallPatron.GetComponent<MallPatron>().LeaveScore.ToString("F2");
                m_choiceText.enabled = true;

                if(m_historyText.enabled)
                {
                    ShowHistory();
                }

            }
            else
            {
                m_histButton.SetActive(false);
                m_historyText.enabled = false;
                m_actionText.enabled = false;
                m_choiceText.enabled = false;

            }
        }
        else
        {
            m_histButton.SetActive(false);
            m_historyText.enabled = false;
            m_actionText.enabled = false;
            m_choiceText.enabled = false;

            m_motiveText.enabled = false;
            m_numItemsText.enabled = false;
            float avgHunger = 0;
            float avgEnergy = 0;
            float avgSocial = 0;
            for (int i = 0; i < m_numShoppers; ++i)
            {
                avgHunger += m_mallPatrons[i].GetComponent<EatMotive>().Hunger;
                avgEnergy += m_mallPatrons[i].GetComponent<RestMotive>().Energy;
                avgSocial += m_mallPatrons[i].GetComponent<SocialMotive>().Social;
            }
            avgHunger /= m_numShoppers;
            avgEnergy /= m_numShoppers;
            avgSocial /= m_numShoppers;
            m_hungerBar.value = avgHunger;
            m_hungerText.text = "Average Hunger: ";
            m_energyBar.value = avgEnergy;
            m_energyText.text = "Average Energy: ";
            m_socialBar.value = avgSocial;
            m_socialText.text = "Average Social: ";
            m_selectedPatronText.text = "Selected Patron: None";
            m_numShoppersText.text = "Number of Shoppers: " + m_numShoppers.ToString();
        }
        
    }


    public void SpawnShopper()  // spawns the patrons
    {
        if (m_numShoppers < m_maxNumShoppers)
        {
            Vector3 location = m_mallEntrance.gameObject.transform.position;
            location.y = .5f;
            Quaternion rotation = new Quaternion();
            GameObject clone = Instantiate(m_mallPatron, location, rotation, m_spawnedObjects.transform);
            m_mallPatrons.Add(clone);
            ++m_numShoppers;
            if(m_numShoppers == m_maxNumShoppers)
            {
                m_initSpawnDone = true;
            }
            m_spawnTimer = Random.Range(m_minShopperDelay, m_maxShopperDelay);
        }

    }

    public void LeaveMall(GameObject patron)    // the patron leaves the mall
    {
        m_mallPatrons.Remove(patron);
        Destroy(patron);
        --m_numShoppers;
    }

    public void SelectPatron(GameObject patron) // selects the patron
    {
        if (patron)
        {
            if (m_selectedMallPatron)
            {
                m_selectedMallPatron.GetComponent<Renderer>().material.color = m_color;
            }
            else
            {
                m_color = patron.GetComponent<Renderer>().material.color;
            }
            m_selectedMallPatron = patron;
            m_selectedMallPatron.GetComponent<Renderer>().material.color = m_selectedColor;
        }
        else
        {
            if (m_selectedMallPatron)
            {
                m_selectedMallPatron.GetComponent<Renderer>().material.color = m_color;
            }
            m_selectedMallPatron = patron;
        }
    } 


    // Update is called once per frame
    void Update()
    {
        m_spawnTimer -= Time.deltaTime;
        if (m_spawnTimer <= 0)
        {
            if (m_numShoppers <= m_minNumShoppers || !m_initSpawnDone)
            {
                SpawnShopper();
            }
        }
        UpdateUI();
    }

}
