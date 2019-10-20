// by Donovan Colen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//simple class for how the player can controll the camera
public class PlayerController : MonoBehaviour
{
    //tuneables
    [SerializeField]
    float m_horizontalLookSpeed = 1;
    [SerializeField]
    float m_verticalLookSpeed = 1;
    [SerializeField]
    float m_speed = 1;
    [SerializeField]
    float m_scrollSpeed = 10;

    // variables
    private Vector2 m_mousePos;
    private Game_Manager m_gameManager = null;


    // Use this for initialization
    void Start()
    {
        m_gameManager = Game_Manager.Instance;
        m_mousePos = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        if (!m_gameManager)
        {
            Debug.LogError("no Game Manager on player");
        }

    }

    // rotates the camera along the apropriate axis
    private void RotateCam()
    {
        float aTurnX;
        aTurnX = m_mousePos.x;
        transform.Rotate(0.0f, m_horizontalLookSpeed * aTurnX * .1f, 0.0f);

        float aTiltY;
        aTiltY = m_mousePos.y;
        transform.Rotate(-m_verticalLookSpeed * aTiltY * .1f, 0.0f, 0.0f);
    }

    // input for moveing the camera around
    private void Move()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.up * m_speed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += -transform.right * m_speed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += -transform.up * m_speed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * m_speed;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0, 0, m_speed);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0, 0, -m_speed);
        }
        if (Input.GetKey(KeyCode.Backspace))    // deselect patron key
        {
            m_gameManager.SelectPatron(null);
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            transform.position += transform.forward * Input.GetAxis("Mouse ScrollWheel") * m_speed * m_scrollSpeed;
        }

    }

    // Update is called once per frame
    void Update()
    {
        m_mousePos.x = Input.GetAxis("Mouse X");
        m_mousePos.y = Input.GetAxis("Mouse Y");

        Move();

        if (Input.GetMouseButton(1))
        {
            RotateCam();
        }
        else if (Input.GetMouseButton(0))
        {
            Ray aRay = Camera.main.ScreenPointToRay(Input.mousePosition); //new Ray(transform.position, Input.mousePosition);

            RaycastHit aRaycastHit;

            if (Physics.Raycast(aRay, out aRaycastHit, 10000))  // if hit something
            {
                if (aRaycastHit.transform.tag == "Shopper") // check if shopper
                {
                    m_gameManager.SelectPatron(aRaycastHit.transform.gameObject);   // select that patron
                }
            }

        }
    }
}
