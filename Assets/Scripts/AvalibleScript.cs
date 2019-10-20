// by Donovan Colen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// checks the avalibilty of loaction the patron wants to go
public class AvalibleScript : MonoBehaviour
{
    // retruns true if there is a slot open on the object
    public bool HasAvalibility()
    {
        SpaceManager[] temp = gameObject.GetComponentsInChildren<SpaceManager>();

        for(int i = 0; i < temp.Length; ++i)
        {
            if(temp[i].Reserved == false)
            {
                return true;
            }
        }
        return false;
        
    }

    // returns the object if there is a slot open
    public GameObject GetAvalibleSpot()
    {
        for(int i = 0; i < gameObject.transform.childCount; i++)
        {
            SpaceManager temp = gameObject.transform.GetChild(i).GetComponent<SpaceManager>();
            if (temp != null && temp.Reserved == false)
            {
                return gameObject.transform.GetChild(i).gameObject;
            }
        }
        return null;
    }
	
}
