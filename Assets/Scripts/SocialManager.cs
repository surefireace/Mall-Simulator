// by Donovan Colen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// manages the groups that are used for social interations
public class SocialManager : MonoBehaviour
{
    //tuneables
    [SerializeField]
    private int m_maxGroupSize = 5;
    [SerializeField]
    private float m_spotVariation = 3;
    [SerializeField]
    private GameObject m_spotObject = null;

    //variables
    private List<GameObject> m_members = null;
    private int m_minGroupSize = 2;
    private int m_groupSize = 0;

	// Use this for initialization
	void Start ()
    {
        m_groupSize = Random.Range(m_minGroupSize, m_maxGroupSize + 1);  // set initial groupSize to a random value
        m_members = new List<GameObject>(m_groupSize);

        for(int i = 0; i < m_groupSize; ++i)
        {
            float newX = Random.Range((int)(transform.position.x - m_spotVariation), (int)(transform.position.x + m_spotVariation) + 1);
            float newZ = Random.Range((int)(transform.position.z - m_spotVariation), (int)(transform.position.z + m_spotVariation) + 1);

            Vector3 location = new Vector3(newX, transform.position.y, newZ);
            
            // to prevent the child from being where the parent is
            while(location == transform.position)
            {
                newX = Random.Range((int)(transform.position.x - m_spotVariation), (int)(transform.position.x + m_spotVariation) + 1);
                newZ = Random.Range((int)(transform.position.z - m_spotVariation), (int)(transform.position.z + m_spotVariation) + 1);
                location = new Vector3(newX, transform.position.y, newZ);
            }

            // to prevent the child from being where anouther child is
            if (gameObject.transform.childCount > 0)
            {
                for(int j = 0; j < gameObject.transform.childCount; ++j)
                {
                    while(location == gameObject.transform.GetChild(j).transform.position)
                    {
                        newX = Random.Range((int)(transform.position.x - m_spotVariation), (int)(transform.position.x + m_spotVariation) + 1);
                        newZ = Random.Range((int)(transform.position.z - m_spotVariation), (int)(transform.position.z + m_spotVariation) + 1);
                        location = new Vector3(newX, transform.position.y, newZ);
                    }
                }
            }

            Quaternion rotation = Quaternion.LookRotation(this.transform.position);
            GameObject child = Instantiate(m_spotObject, location, rotation, transform);
            child.transform.LookAt(transform.position); // make the child look at the parent. so the patrons seem like they are looking at each other
        }
    }

    // returns if the group is at max size
    public bool IsFull()
    {
        return m_members.Count >= m_members.Capacity;
    }

    // add a patron to the group
    public void Join(GameObject obj)
    {
        m_members.Add(obj);
    }

    // removes a patron from the group
    public void Leave(GameObject obj)
    {
        m_members.Remove(obj);
    }

    // returns the current number of patrons in the group
    public int GetCurrGroupAmount()
    {
        int amount = 0;
        for(int i = 0; i < gameObject.transform.childCount; ++i)
        {
            SpaceManager temp = gameObject.transform.GetChild(i).GetComponent<SpaceManager>();
            if (temp != null && temp.Taken == true)
            {
                ++amount;
            }
        }
        return amount;
    }
}
