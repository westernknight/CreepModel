using UnityEngine;
using System.Collections;

public class CCDemo0_HandCollision : MonoBehaviour
{
    public Transform m_Ctrl;

    void OnTriggerEnter()
    {
        m_Ctrl.SendMessage("Msg_CaughtTheCube");
    }    
    
}
