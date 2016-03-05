using UnityEngine;
using System;

/// <summary>
/// used WSADQE and mouse to control cam movement
/// </summary>
public class MHCamera : MonoBehaviour {
	
	public float m_fXRotMul = 1.1f;
	public float m_fYRotMul = 1.1f;
	
	public float m_fMovSpd = 15f; // per second	

    private Transform m_tr;
	
	// Use this for initialization
	void Start () {
        m_tr = transform;
        Screen.lockCursor = true;
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            Screen.lockCursor = false;
        }
        else
        {
            Screen.lockCursor = true;

            float XRotDelta = -Input.GetAxis("Mouse Y") * m_fXRotMul;
            float YRotDelta = Input.GetAxis("Mouse X") * m_fYRotMul;

            Vector3 euler = m_tr.eulerAngles;
            float xRot = euler.x;
            float yRot = euler.y;
            xRot += XRotDelta;
            yRot = Mathf.Repeat(yRot + YRotDelta, 360f);
            transform.eulerAngles = new Vector3(xRot, yRot, 0);
        }

		Vector3 mov = Vector3.zero;
		mov.x = Input.GetAxis("Horizontal") * Time.deltaTime * m_fMovSpd;
		mov.z = Input.GetAxis("Vertical") * Time.deltaTime * m_fMovSpd;

        if (Input.GetKey(KeyCode.E))
        {
            mov.y = Time.deltaTime * m_fMovSpd;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            mov.y = -Time.deltaTime * m_fMovSpd;
        }
		
		transform.Translate(mov, Space.Self);
	}
}

