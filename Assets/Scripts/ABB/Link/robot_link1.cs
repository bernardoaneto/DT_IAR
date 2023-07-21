// System
using System;
// Unity 
using UnityEngine;
using static data_processing;
using Debug = UnityEngine.Debug;

public class robot_link1 : MonoBehaviour
{
    void FixedUpdate()
    {
        try
        {
            transform.localEulerAngles = new Vector3(0f, 0f, (float)((-1) * ABB_Stream_Data_XML.J_Orientation[0]));
        }
        catch (Exception e)
        {
            Debug.Log("Exception:" + e);
        }
    }
    void OnApplicationQuit()
    {
        Destroy(this);
    }
}
