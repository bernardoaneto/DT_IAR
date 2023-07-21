// System
using System;
// Unity 
using UnityEngine;
using static data_processing;
using Debug = UnityEngine.Debug;

public class robot_link5 : MonoBehaviour
{ 
    void FixedUpdate()
    {
        try
        {
            transform.localEulerAngles = new Vector3(0f, (float)((-1) * ABB_Stream_Data_XML.J_Orientation[4]), 0f);
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
