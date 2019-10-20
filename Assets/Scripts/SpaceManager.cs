// by Donovan Colen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// simple class to tell if there is open slots for the chairs
public class SpaceManager : MonoBehaviour
{
    private bool m_occupied = false;
    private bool m_reserved = false;

    public bool Taken
    {
        get { return m_occupied; }
        set { m_occupied = value; }
    }

    public bool Reserved
    {
        get { return m_reserved; }
        set { m_reserved = value; }
    }
}
