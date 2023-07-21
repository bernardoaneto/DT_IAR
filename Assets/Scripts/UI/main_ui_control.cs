// System 
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
// Unity
using UnityEngine;
using UnityEngine.UI;
// TM 
using TMPro;
using System.Diagnostics.Eventing.Reader;

public class main_ui_control : MonoBehaviour
{
    // -------------------- GameObject -------------------- //
    [SerializeField] private GameObject togglePrefab;
    private GameObject toggleObject;
    [SerializeField] private GameObject startPosition_prefab;

    [SerializeField] private GameObject Simulation_Light_Enable_prefab;
    [SerializeField] private GameObject RealProcess_Light_Enable_prefab;

    //// -------------------- Material -------------------- //
    //[SerializeField] private Material red_light;
    //[SerializeField] private Material blue_light;
    //[SerializeField] private Material white_light;
    //float duration = 2.0f;
    Renderer rend_sim, rend_real;

    // -------------------- Image -------------------- //
    [SerializeField] private TMP_Text disconnection_info_button;
    [SerializeField] private TMP_Text connection_info_button;

    // -------------------- TMP_InputField -------------------- //
    public TMP_InputField ip_address_txt;
    // -------------------- Float -------------------- //
    private float ex_param = 100f;
    // -------------------- TextMeshProUGUI -------------------- //
    [SerializeField] private TMP_Text program_name, opmode, robot_exec_state;
    [SerializeField] private TMP_Text cut_gas, laser_power, cut_gaspressure;
    [SerializeField] private TMP_Text focal_dist, cut_standoffdist, nozzle_d;
    [SerializeField] private TMP_Text velocity_tcp, laser_program;

    [SerializeField] private TMP_Text position_x_txt, position_y_txt, position_z_txt;

    [SerializeField] private TMP_Text position_q1_txt, position_q2_txt, position_q3_txt, position_q4_txt;

    [SerializeField] private TMP_Text position_j1_txt, position_j2_txt, position_j3_txt;
    [SerializeField] private TMP_Text position_j4_txt, position_j5_txt, position_j6_txt;

    // ------------------------------------------------------------------------------------------------------------------------ //
    // ------------------------------------------------ INITIALIZATION {START} ------------------------------------------------ //
    // ------------------------------------------------------------------------------------------------------------------------ //
    void Start()
    {
        // Connection information {text color} -> Connect/Disconnect
        connection_info_button.color = Color.white;
        disconnection_info_button.color = Color.white;

        rend_sim = Simulation_Light_Enable_prefab.GetComponent<Renderer>();
        rend_sim.material.color = Color.white;

        rend_real = RealProcess_Light_Enable_prefab.GetComponent<Renderer>();
        rend_real.material.color = Color.white;

        // Dashboard
        program_name.text = "Waiting for connection";
        opmode.text = "Waiting for connection";
        robot_exec_state.text = "running/stopped";
        cut_gas.text = "oxygen/nitrogen";
        laser_power.text = "0.00";
        cut_gaspressure.text = "0.00";
        focal_dist.text = "0.00";
        cut_standoffdist.text = "0.00";
        nozzle_d.text = "0.00";
        velocity_tcp.text = "0.00";
        laser_program.text = "0.00";

        // Position {Cartesian} -> X..Z
        position_x_txt.text = "0.00";
        position_y_txt.text = "0.00";
        position_z_txt.text = "0.00";
        // Position {Rotation} -> Quaternion(1..4)
        position_q1_txt.text = "0.00000000";
        position_q2_txt.text = "0.00000000";
        position_q3_txt.text = "0.00000000";
        position_q4_txt.text = "0.00000000";
        // Position Joint -> 1 - 6
        position_j1_txt.text = "0.00";
        position_j2_txt.text = "0.00";
        position_j3_txt.text = "0.00";
        position_j4_txt.text = "0.00";
        position_j5_txt.text = "0.00";
        position_j6_txt.text = "0.00";

        // Robot IP Address
        //ip_address_txt.text = "127.0.0.1";
        ip_address_txt.text = "192.168.190.16";
    }

    // ------------------------------------------------------------------------------------------------------------------------ //
    // ------------------------------------------------ MAIN FUNCTION {Cyclic} ------------------------------------------------ //
    // ------------------------------------------------------------------------------------------------------------------------ //
    void FixedUpdate()
    {
        // Robot IP Address (Read) -> XML thread function
        data_processing.ABB_Stream_Data_XML.ip_address = ip_address_txt.text;
        // Robot IP Address (Write) -> JSON thraed function
        //data_processing.ABB_Stream_Resources_Data_XML.ip_address = data_processing.ABB_Stream_Data_XML.ip_address;

        // ------------------------ Connection Information ------------------------//
        // If the button (connect/disconnect) is pressed, change the color of text
        if (data_processing.GlobalVariables_Main_Control.connect == true)
        {
            // green color
            connection_info_button.color = Color.green;
            disconnection_info_button.color = Color.white;
        }
        else if(data_processing.GlobalVariables_Main_Control.disconnect == true)
        {
            // red color
            connection_info_button.color = Color.white;
            disconnection_info_button.color = Color.red;            
        }

        // ------------------------ Cyclic read parameters {diagnostic panel} ------------------------ //

        if (data_processing.ABB_Stream_Data_XML.Simulation_Light_Enable == 1)
        {
            rend_sim.material.color = new Color(0, 44, 255); //blue
            rend_real.material.color = Color.white;
            
        }
        if (data_processing.ABB_Stream_Data_XML.RealProcess_Light_Enable == 1)
        {
            rend_sim.material.color = Color.white;
            rend_real.material.color = new Color(255, 0, 0); //red
        }

        //Dashboard
        program_name.text = data_processing.ABB_Stream_Data_XML.program_name;

        opmode.text = data_processing.ABB_Stream_Data_XML.opmode;

        robot_exec_state.text = data_processing.ABB_Stream_Data_XML.robot_exec_state;

        if (data_processing.ABB_Stream_Data_XML.cut_gas) cut_gas.text = "nitrogen";
        else cut_gas.text = "oxygen";

        laser_power.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.laser_power, 2)).ToString();

        cut_gaspressure.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.cut_gaspressure, 2)).ToString();

        focal_dist.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.focal_dist, 2)).ToString();

        cut_standoffdist.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.cut_standoffdist, 2)).ToString();

        nozzle_d.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.nozzle_d, 2)).ToString();

        velocity_tcp.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.velocity_tcp, 2)).ToString();

        laser_program.text = data_processing.ABB_Stream_Data_XML.laser_program.ToString();

        // Position {Cartesian} -> X..Z
        position_x_txt.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.C_Position[0], 2)).ToString();
        position_y_txt.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.C_Position[1], 2)).ToString();
        position_z_txt.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.C_Position[2], 2)).ToString();
        // Position {Rotation} -> Quaternion(q1..q4)
        position_q1_txt.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.C_Orientation[0], 6)).ToString();
        position_q2_txt.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.C_Orientation[1], 6)).ToString();
        position_q3_txt.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.C_Orientation[2], 6)).ToString();
        position_q4_txt.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.C_Orientation[3], 6)).ToString();
        // Position Joint -> 1 - 6
        position_j1_txt.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.J_Orientation[0], 2)).ToString();
        position_j2_txt.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.J_Orientation[1], 2)).ToString();
        position_j3_txt.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.J_Orientation[2], 2)).ToString();
        position_j4_txt.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.J_Orientation[3], 2)).ToString();
        position_j5_txt.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.J_Orientation[4], 2)).ToString();
        position_j6_txt.text = ((float)Math.Round(data_processing.ABB_Stream_Data_XML.J_Orientation[5], 2)).ToString();
    }

    // ------------------------------------------------------------------------------------------------------------------------//
    // -------------------------------------------------------- FUNCTIONS -----------------------------------------------------//
    // ------------------------------------------------------------------------------------------------------------------------//

    // -------------------- Destroy Blocks -------------------- //
    void OnApplicationQuit()
    {
        // Destroy all
        Destroy(this);
    }

    // -------------------- Connect Button -> is pressed -------------------- //
    public void TaskOnClick_ConnectBTN()
    {
        data_processing.GlobalVariables_Main_Control.connect    = true;
        data_processing.GlobalVariables_Main_Control.disconnect = false;
    }

    // -------------------- Disconnect Button -> is pressed -------------------- //
    public void TaskOnClick_DisconnectBTN()
    {
        data_processing.GlobalVariables_Main_Control.connect    = false;
        data_processing.GlobalVariables_Main_Control.disconnect = true;
    }

    // -------------------- InspectArmManipulator Button -> is pressed -------------------- //
    public void TaskOnClick_InspectArmManipulator()
    {
        if (toggleObject == null)
        {
            // Calculate the start position based on the camera's position and forward direction
            
            Vector3 startPosition = startPosition_prefab.transform.position;
            //Quaternion startRotation = GameObject.Find("digital_twin").transform.rotation;
            Quaternion startRotation = startPosition_prefab.transform.rotation;

            // Instantiate the toggle prefab at the starting position
            toggleObject = Instantiate(togglePrefab, startPosition, startRotation);

            // Specify the desired end position
            Vector3 endPosition = Camera.main.transform.position + Camera.main.transform.right * 1f;

            // Specify the starting scale
            Vector3 startScale = toggleObject.transform.localScale;

            // Specify the ending scale
            Vector3 endScale = new Vector3(startScale.x * 0.5f, startScale.y * 0.5f, startScale.z * 0.5f); // Adjust scale as needed

            // Start the smooth movement coroutine
            StartCoroutine(MoveObjectSmoothly(toggleObject.transform, startPosition, endPosition, startScale, endScale));
        }
        else
        {
            // Delete the toggle object
            Destroy(toggleObject);
            toggleObject = null;
        }

    }


    // -------------------- Move Object Smoothly -> from start position/scale to end position/scale -------------------- //
    private IEnumerator MoveObjectSmoothly(Transform objectTransform, Vector3 startPosition, Vector3 endPosition, Vector3 startScale, Vector3 endScale, float duration = 3f)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate the interpolation factor
            float t = elapsedTime / duration;

            // Interpolate the position using Vector3.Lerp
            objectTransform.position = Vector3.Lerp(startPosition, endPosition, t);

            // Interpolate the scale using Vector3.Lerp
            objectTransform.localScale = Vector3.Lerp(startScale, endScale, t);

            // Update the elapsed time
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Set the final position and scale to ensure accuracy
        objectTransform.position = endPosition;
        objectTransform.localScale = endScale;
    }

}
