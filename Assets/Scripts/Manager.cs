using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.SocialPlatforms;

public class Manager : MonoBehaviour
{

    [SerializeField] private GameObject targetObject; // Reference to the target object
    [SerializeField] private TMPro.TMP_Text position_x;
    [SerializeField] private TMPro.TMP_Text position_y;
    [SerializeField] private TMPro.TMP_Text position_z;
    [SerializeField] private TMPro.TMP_Text rotation_x;
    [SerializeField] private TMPro.TMP_Text rotation_y;
    [SerializeField] private TMPro.TMP_Text rotation_z;

    private Gauge gauge; // Reference to the Gauge script
    private Timeseries timeseries; // Reference to the Timeseries script

    // Start is called before the first frame update
    void Start()
    {
        // Drag the target object to the targetObject variable in the Manager script using the Unity inspector

        // Deactivate the target object initially
        DeactivateObject();

        //// SQL server
        //var c = new Client("192.168.190.100", 58580, message =>
        //{
        //    var msg = (JObject)JObject.Parse(message)["msg"];
        //    if (msg == null) return;
        //    var table = msg["table"]?.ToString();
        //    var data = msg["data"]?.ToString();
        //    if (table == null || data == null) return;
        //    try
        //    {
        //        var dataInput = new DataInput(table, data);
        //        Table.UpdateTable(dataInput);
        //    }
        //    catch (Exception)
        //    {
        //        // ignored
        //    }
        //});

        //object[] array_gauge = new object[] { "AGIL3D", "FIBERCUT_TRAVELDIST", -15f, 15f };
        //gauge = FindObjectOfType<Gauge>(); // Find the Gauge script in the scene
        //gauge.SetProperties(array_gauge, true);

        //object[] array_timeseries = new object[] { "AGIL3D", "FIBERCUT_TRAVELDIST", -15f, 15f, 15f };
        //timeseries = FindObjectOfType<Timeseries>(); // Find the Timeseries script in the scene
        //timeseries.SetProperties(array_timeseries, true);

    }

    // Update is called once per frame
    void Update()
    {
        position_x.text = targetObject.transform.position.x.ToString();
        position_y.text = targetObject.transform.position.y.ToString();
        position_z.text = targetObject.transform.position.z.ToString();
        rotation_x.text = targetObject.transform.rotation.x.ToString();
        rotation_y.text = targetObject.transform.rotation.y.ToString();
        rotation_z.text = targetObject.transform.rotation.z.ToString();
    }

    public void ActivateObject()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }

    public void DeactivateObject()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }

    public void SetParent(Transform parent)
    {
        if (targetObject != null && parent != null)
        {
            targetObject.transform.SetParent(parent, false);
        }
    }

}
