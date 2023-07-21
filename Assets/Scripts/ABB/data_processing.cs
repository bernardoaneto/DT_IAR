// System
using System;
using System.Threading;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Diagnostics;
// Unity
using UnityEngine;
using Debug = UnityEngine.Debug;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class data_processing : MonoBehaviour
{
    public static class GlobalVariables_Main_Control
    {
        public static bool connect, disconnect;
    }

    public static class ABB_Stream_Data_XML
    {
        //  IP Port Number and IP Address
        public static string ip_address;
        //  The target resource of reading the data: program, opmode, execution, jointtarget, robtarget, signals,
        //  cut_gas, laser_power, cut_gaspressure, focal_dist, cut_standoffdist, nozzle_d, velocity_tcp, laser_program
        public static string resource = "";
        //  Comunication Speed (ms)
        public static int time_step;

        // Joint Space: jointtarget
        //  Orientation {J1 .. J6} (°)
        public static double[] J_Orientation = new double[6];
        // Cartesian Space: robtarget
        //  Position {X, Y, Z} (mm)
        public static double[] C_Position = new double[3];
        //  Orientation {Quaternion} (-):
        public static double[] C_Orientation = new double[4];

        //  RAPID Program name: program
        public static string program_name = "Waiting for connection";
        //  Operation Mode: ( INIT | AUTO_CH | MANF_CH | MANR | MANF | AUTO | UNDEF )
        public static string opmode = "Waiting for connection";
        //  RAPID Execution state: execution ( running | stopped )
        public static string robot_exec_state = "running/stopped";

        // signals
        //  Simul_Status
        //  Simulation_Light_Enable: Bit (0/1)
        public static int Simulation_Light_Enable = 0;
        //  RealProcess_Light_Enable: Bit (0/1)
        public static int RealProcess_Light_Enable = 0;

        // rapid/symbol/data/RAPID/T_ROB1/Main_linhas/
        //  Active_Gas: 0 - oxygen, 1 - nitrogen
        public static bool cut_gas = true;
        //  Laser_Power: int/float (0-3000) W
        public static double laser_power = 9.99f;
        //  Gas_Press_Setpoint: float (0-25.0) bar
        public static double cut_gaspressure = 9.99f;
        //  FocalDistance: float (-5 , +5)
        public static double focal_dist = 9.99f;
        //  FiberCut_Standoff: float (0.5 - 2.5) mm
        public static double cut_standoffdist = 9.99f;
        //  NozzleAperture: float ( 0.5 - 2) mm
        public static double nozzle_d = 9.99f;
        //  Robot_Vel_Setpoint: float  *1000/60
        public static double velocity_tcp = 9.99f;
        //  Laser_Program: int (1-7)
        public static int laser_program = 9;

        // Class thread information (is alive or not)
        public static bool is_alive = false;
    }

    //public static class ABB_Stream_Resources_Data_XML
    //{
    //    // IP Port Number and IP Address
    //    public static string ip_address;
    //    //  The target of reading the data: jointtarget / robtarget
    //    public static string resource = "";
    //    // Comunication Speed (ms)
    //    public static int time_step;
    //    // Cartesian Space:
    //    //  Position {X, Y, Z} (mm)
    //    public static double[] C_Position = new double[3];
    //    //  Orientation {Quaternion} (-):
    //    public static double[] C_Orientation = new double[4];
    //    // Class thread information (is alive or not)
    //    public static bool is_alive = false;
    //}

    // Class Stream {ABB Robot Web Services - XML}
    private ABB_Stream_XML ABB_Stream_Robot_XML;
    //// Start Stream {ABB Robot Web Services - JSON}
    //private ABB_Stream_Resources_XML ABB_Stream_Robot_JSON;

    // Other variables
    private int main_state = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Initialization {Robot Web Services ABB - XML}
        //  Stream Data:
        //ABB_Stream_Data_XML.ip_address = "127.0.0.1";
        ABB_Stream_Data_XML.ip_address = "192.168.190.16";
        //  The target of reading the data: jointtarget / robtarget
        //ABB_Stream_Data_XML.resource = "jointtarget"; ////////////////////////////////tirar daqui e por na próxima função a ser usado
        //  Communication speed (ms)
        ABB_Stream_Data_XML.time_step = 20;
        // Initialization {Robot Web Services ABB - JSON}
        //  Stream Data:
        ////ABB_Stream_Data_JSON.ip_address = "127.0.0.1";
        //ABB_Stream_Resources_Data_XML.ip_address = "192.168.190.16";
        ////  The target of reading the data: jointtarget / robtarget
        //ABB_Stream_Resources_Data_XML.resource = "robtarget";
        ////  Communication speed (ms)
        //ABB_Stream_Resources_Data_XML.time_step = 200;

        // Start Stream {ABB Robot Web Services - XML}
        ABB_Stream_Robot_XML = new ABB_Stream_XML();
        //// Start Stream {ABB Robot Web Services - JSON}
        //ABB_Stream_Robot_JSON = new ABB_Stream_Resources_XML();

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        switch (main_state)
        {
            case 0:
                {
                    // ------------------------ Wait State {Disconnect State} ------------------------//
                    if (GlobalVariables_Main_Control.connect == true)
                    {
                        //Start Stream { ABB Robot Web Services - XML}
                        ABB_Stream_Robot_XML.Start();
                        ////Start Stream { ABB Robot Web Services - JSON}
                        //ABB_Stream_Robot_JSON.Start();

                        // go to connect state
                        main_state = 1;
                    }
                }
                break;
            case 1:
                {
                    // ------------------------ Data Processing State {Connect State} ------------------------//
                    if (GlobalVariables_Main_Control.disconnect == true)
                    {
                        // Stop threading block {ABB Robot Web Services - XML}
                        if (ABB_Stream_Data_XML.is_alive == true)
                        {
                            ABB_Stream_Robot_XML.Stop();
                        }

                        //// Stop threading block {ABB Robot Web Services - JSON}
                        //if (ABB_Stream_Resources_Data_XML.is_alive == true)
                        //{
                        //    ABB_Stream_Robot_JSON.Stop();
                        //}

                        if (ABB_Stream_Data_XML.is_alive == false /*&& ABB_Stream_Resources_Data_XML.is_alive == false*/)
                        {
                            // go to initialization state {wait state -> disconnect state}
                            main_state = 0;
                        }
                    }
                }
                break;
        }
    }

    void OnApplicationQuit()
    {
        try
        {
            // Destroy Stream {ABB Robot Web Services - XML}
            ABB_Stream_Robot_XML.Destroy();
            ////Start Stream { ABB Robot Web Services - JSON}
            //ABB_Stream_Robot_JSON.Destroy();

            Destroy(this);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    class ABB_Stream_XML
    {
        // Initialization of Class variables
        //  Thread
        private Thread robot_thread = null;
        private bool exit_thread = false;
        // Robot Web Services (RWS): XML Communication
        private CookieContainer c_cookie = new CookieContainer();
        private NetworkCredential n_credential = new NetworkCredential("Default User", "robotics");
        

        public void ABB_Stream_Thread_XML()
        {
            try
            {
                // Initialization timer
                var t = new Stopwatch();

                while (!exit_thread)
                {
                    // t_{0}: Timer start.
                    t.Start();

                    ////  The target of reading the data: jointtarget / robtarget
                    //ABB_Stream_Data_XML.resource = "jointtarget";
                    //// Get the system resource
                    //Stream resource_data = GetResource(ABB_Stream_Data_XML.ip_address, ABB_Stream_Data_XML.resource);
                    //// Current data streaming from the source page
                    //ProcessResource(resource_data, ABB_Stream_Data_XML.resource);

                    // List of resources to read
                    List<string> resources = new List<string> { "jointtarget", "program", "opmode", "execution", "robtarget", "signals", "cut_gas", "laser_power", "cut_gaspressure", "focal_dist", "cut_standoffdist", "nozzle_d", "velocity_tcp", "laser_program" };
                    //List<Stream> streams = new List<Stream>(resources.Count());

                    // Process each resource
                    //for (int i = 0; i < resources.Count(); i++)
                    foreach (string resource in resources)
                    {
                        // Set the current resource
                        //ABB_Stream_Data_XML.resource = resource;

                        // Get the system resource
                        //streams[i] = GetResource(ABB_Stream_Data_XML.ip_address, resources[i]);
                        Stream stream = GetResource(ABB_Stream_Data_XML.ip_address, resource);

                        // Process the resource data
                        ProcessResource(stream, resource);

                    }

                    // t_{1}: Timer stop.
                    t.Stop();

                    // Recalculate the time: t = t_{1} - t_{0} -> Elapsed Time in milliseconds
                    if (t.ElapsedMilliseconds < ABB_Stream_Data_XML.time_step)
                    {
                        Thread.Sleep(ABB_Stream_Data_XML.time_step - (int)t.ElapsedMilliseconds);
                    }

                    // Reset (Restart) timer.
                    t.Restart();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        Stream GetResource(string host, string value)
        {
            string url = "http://" + host + "/";

            switch (value)
            {
                case "program":
                    url += "rw/rapid/tasks/T_ROB1/program";
                    break;
                case "opmode":
                    url += "rw/panel/opmode";
                    break;
                case "execution":
                    url += "rw/rapid/execution";
                    break;
                case "jointtarget":
                    url += "rw/rapid/tasks/T_ROB1/motion?resource=jointtarget";
                    break;
                case "robtarget":
                    url += "rw/rapid/tasks/T_ROB1/motion?resource=robtarget";
                    break;
                case "signals":
                    url += "rw/iosystem/signals";
                    break;
                case "cut_gas":
                    //url += "rw/rapid/symbol/data/RAPID/T_ROB1/Main_linhas/cut_gas";
                    url += "rw/rapid/symbol/data/RAPID/T_ROB1/mAUTODESK/cut_gas";
                    break;
                case "laser_power":
                    //url += "rw/rapid/symbol/data/RAPID/T_ROB1/Main_linhas/laser_power";
                    url += "rw/rapid/symbol/data/RAPID/T_ROB1/mAUTODESK/laser_power";
                    break;
                case "cut_gaspressure":
                    //url += "rw/rapid/symbol/data/RAPID/T_ROB1/Main_linhas/cut_gaspressure";
                    url += "rw/rapid/symbol/data/RAPID/T_ROB1/mAUTODESK/cut_gaspressure";
                    break;
                case "focal_dist":
                    //url += "rw/rapid/symbol/data/RAPID/T_ROB1/Main_linhas/focal_dist";
                    url += "rw/rapid/symbol/data/RAPID/T_ROB1/mAUTODESK/focal_dist";
                    break;
                case "cut_standoffdist":
                    //url += "rw/rapid/symbol/data/RAPID/T_ROB1/Main_linhas/cut_standoffdist";
                    url += "rw/rapid/symbol/data/RAPID/T_ROB1/mAUTODESK/cut_standoffdist";
                    break;
                case "nozzle_d":
                    //url += "rw/rapid/symbol/data/RAPID/T_ROB1/Main_linhas/nozzle_d";
                    url += "rw/rapid/symbol/data/RAPID/T_ROB1/mAUTODESK/nozzle_d";
                    break;
                case "velocity_tcp":
                    //url += "rw/rapid/symbol/data/RAPID/T_ROB1/Main_linhas/velocity_tcp";
                    url += "rw/rapid/symbol/data/RAPID/T_ROB1/mAUTODESK/velocity_tcp";
                    break;
                case "laser_program":
                    //url += "rw/rapid/symbol/data/RAPID/T_ROB1/Main_linhas/laser_program";
                    url += "rw/rapid/symbol/data/RAPID/T_ROB1/mAUTODESK/laser_program";
                    break;
                default:
                    throw new ArgumentException("Invalid value");
            }
            
            // Create the HttpWebRequest based on the constructed URL
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
            // Login: Default User; Password: robotics
            request.Credentials = n_credential;
            // Don't use proxy, it's assumed that the RC/VC is reachable without going via proxy 
            request.Proxy = null;
            request.Method = "GET";
            // Re-use HTTP session between requests 
            request.CookieContainer = c_cookie;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response.GetResponseStream();
        }

        void ProcessResource(Stream resource_data, string value)
        {
            // Process the resource data based on the specified value
            switch (value)
            {
                case "program":
                    ProcessProgramData(resource_data);
                    break;
                case "opmode":
                    ProcessOpmodeData(resource_data);
                    break;
                case "execution":
                    ProcessExecutionData(resource_data);
                    break;
                case "jointtarget":
                    ProcessJointTargetData(resource_data);
                    break;
                case "robtarget":
                    ProcessRobTargetData(resource_data);
                    break;
                case "signals":
                    ProcessSignalsData(resource_data);
                    break;
                case "cut_gas":
                    ProcessCutGasData(resource_data);
                    break;
                case "laser_power":
                    ProcessLaserPowerData(resource_data);
                    break;
                case "cut_gaspressure":
                    ProcessCutGasPressureData(resource_data);
                    break;
                case "focal_dist":
                    ProcessFocalDistData(resource_data);
                    break;
                case "cut_standoffdist":
                    ProcessCutStandOffDistData(resource_data);
                    break;
                case "nozzle_d":
                    ProcessNozzleDData(resource_data);
                    break;
                case "velocity_tcp":
                    ProcessVelocityTCPData(resource_data);
                    break;
                case "laser_program":
                    ProcessLaserProgramData(resource_data);
                    break;
                default:
                    throw new ArgumentException("Invalid value");
            }
        }


        void ProcessProgramData(Stream resource_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(resource_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read State {Program Name} -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='rap-program']", nsmgr);

            // Program Name -> Read RWS XML
            ABB_Stream_Data_XML.program_name = xml_node[0].SelectSingleNode("ns:span[@class='name']", nsmgr).InnerText.ToString();
        }

        void ProcessOpmodeData(Stream resource_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(resource_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read State {Operation Mode} -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='pnl-opmode']", nsmgr);

            // Operation Mode -> Read RWS XML
            ABB_Stream_Data_XML.opmode = xml_node[0].SelectSingleNode("ns:span[@class='opmode']", nsmgr).InnerText.ToString();
        }

        void ProcessExecutionData(Stream resource_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(resource_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read State {Execution State} -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='rap-execution']", nsmgr);

            // Execution State -> Read RWS XML
            ABB_Stream_Data_XML.robot_exec_state = xml_node[0].SelectSingleNode("ns:span[@class='ctrlexecstate']", nsmgr).InnerText.ToString();
        }

        void ProcessJointTargetData(Stream resource_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(resource_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read State {Joint (1 - 6)} -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='rapid-jointtarget']", nsmgr);

            // Joint (1 - 6) -> Read RWS XML
            ABB_Stream_Data_XML.J_Orientation[0] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='j1']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.J_Orientation[1] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='j2']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.J_Orientation[2] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='j3']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.J_Orientation[3] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='j4']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.J_Orientation[4] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='j5']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.J_Orientation[5] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='j6']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
        }

        void ProcessRobTargetData(Stream resource_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(resource_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read State {Rob Target} -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='rapid-robtarget']", nsmgr);

            // Cartesian {X, Y, Z} -> Read RWS XML
            ABB_Stream_Data_XML.C_Position[0] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='x']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.C_Position[1] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='y']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.C_Position[2] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='z']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);

            //  Quaternion { q1..q4} -> Read RWS XML
            ABB_Stream_Data_XML.C_Orientation[0] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='q1']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.C_Orientation[1] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='q2']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.C_Orientation[2] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='q3']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.C_Orientation[3] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='q4']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
        }

        void ProcessSignalsData(Stream resource_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(resource_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read ios-signal-li -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='ios-signal-li']", nsmgr);

            // Iterate over the signal elements
            foreach (XmlNode node in xml_node)
            {
                // Check if the signal name matches "RealProcess_Light_Enable"
                if (node.SelectSingleNode("ns:span[@class='name']", nsmgr).InnerText.ToString() == "RealProcess_Light_Enable")
                {
                    // Get the value of the "lvalue" element
                    ABB_Stream_Data_XML.RealProcess_Light_Enable = int.Parse(node.SelectSingleNode("ns:span[@class='lvalue']", nsmgr).InnerText.ToString());
                    break; // No need to continue iterating if the value is found
                }
            }

            // -------------------- Read ios-signal-li -------------------- //
            xml_node = xml_doc.SelectNodes("//ns:li[@class='ios-signal-li']", nsmgr);
            foreach (XmlNode node in xml_node)
            {
                // Check if the signal name matches "Simulation_Light_Enable"
                if (node.SelectSingleNode("ns:span[@class='name']", nsmgr).InnerText.ToString() == "Simulation_Light_Enable")
                {
                    // Get the value of the "lvalue" element
                    ABB_Stream_Data_XML.Simulation_Light_Enable = int.Parse(node.SelectSingleNode("ns:span[@class='lvalue']", nsmgr).InnerText.ToString());
                    break; // No need to continue iterating if the value is found
                }
            }

        }

        void ProcessCutGasData(Stream resource_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(resource_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read State {Cut Gas} -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='rap-data']", nsmgr);

            // Cut Gas -> Read RWS XML
            ABB_Stream_Data_XML.cut_gas = Convert.ToBoolean(double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='value']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat));
        }
        void ProcessLaserPowerData(Stream resource_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(resource_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read State {Laser Power} -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='rap-data']", nsmgr);

            // Laser Power -> Read RWS XML
            ABB_Stream_Data_XML.laser_power = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='value']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
        }
        void ProcessCutGasPressureData(Stream resource_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(resource_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read State {Cut Gas Pressure} -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='rap-data']", nsmgr);

            // Cut Gas Pressure -> Read RWS XML
            ABB_Stream_Data_XML.cut_gaspressure = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='value']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
        }
        void ProcessFocalDistData(Stream resource_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(resource_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read State {Focal Distance} -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='rap-data']", nsmgr);

            // Focal Distance -> Read RWS XML
            ABB_Stream_Data_XML.focal_dist = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='value']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
        }
        void ProcessCutStandOffDistData(Stream resource_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(resource_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read State {Cut Stand Off Distance} -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='rap-data']", nsmgr);

            // Cut Stand Off Distance -> Read RWS XML
            ABB_Stream_Data_XML.cut_standoffdist = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='value']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
        }
        void ProcessNozzleDData(Stream resource_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(resource_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read State {Nozzle Aperture} -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='rap-data']", nsmgr);

            // Nozzle Aperture -> Read RWS XML
            ABB_Stream_Data_XML.nozzle_d = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='value']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
        }
        void ProcessVelocityTCPData(Stream resource_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(resource_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read State {Nozzle Aperture} -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='rap-data']", nsmgr);

            // Nozzle Aperture -> Read RWS XML
            ABB_Stream_Data_XML.velocity_tcp = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='value']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
        }
        void ProcessLaserProgramData(Stream resource_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(resource_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read State {Laser Program} -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='rap-data']", nsmgr);

            // Laser Program -> Read RWS XML
            ABB_Stream_Data_XML.laser_program = int.Parse(xml_node[0].SelectSingleNode("ns:span[@class='value']", nsmgr).InnerText.ToString());
        }

        public void Start()
        {
            exit_thread = false;
            // Start a thread to stream ABB Robot
            robot_thread = new Thread(new ThreadStart(ABB_Stream_Thread_XML));
            robot_thread.IsBackground = true;
            robot_thread.Start();
            // Thread is active
            ABB_Stream_Data_XML.is_alive = true;
        }
        public void Stop()
        {
            exit_thread = true;
            // Stop a thread
            Thread.Sleep(100);
            ABB_Stream_Data_XML.is_alive = robot_thread.IsAlive;
            robot_thread.Abort();
        }
        public void Destroy()
        {
            // Stop a thread (Robot Web Services communication)
            Stop();
            Thread.Sleep(100);
        }
    }

    //class ABB_Stream_Resources_XML
    //{
    //    // Initialization of Class variables
    //    //  Thread
    //    private Thread robot_thread = null;
    //    private bool exit_thread = false;

    //    async void ABB_Stream_Thread_JSON()
    //    {
    //        var handler = new HttpClientHandler { Credentials = new NetworkCredential("Default User", "robotics") };
    //        // disable the proxy, the controller is connected on same subnet as the PC 
    //        handler.Proxy = null;
    //        handler.UseProxy = false;

    //        try
    //        {
    //            // Send a request continue when complete
    //            using (HttpClient client = new HttpClient(handler))
    //            {
    //                // Initialization timer
    //                var t = new Stopwatch();

    //                while (exit_thread == false)
    //                {
    //                    // t_{0}: Timer start.
    //                    t.Start();

    //                    // Current data streaming from the source page
    //                    using (HttpResponseMessage response = await client.GetAsync("http://" + ABB_Stream_Resources_Data_XML.ip_address + "/rw/rapid/tasks/T_ROB1/motion?resource=" + ABB_Stream_Resources_Data_XML.resource + "&json=1"))
    //                    {
    //                        using (HttpContent content = response.Content)
    //                        {
    //                            try
    //                            {
    //                                // Check that response was successful or throw exception
    //                                response.EnsureSuccessStatusCode();
    //                                // Get HTTP response from completed task.
    //                                string result = await content.ReadAsStringAsync();
    //                                // Deserialize the returned json string
    //                                dynamic obj = JsonConvert.DeserializeObject(result);

    //                                // Display controller name, version and version name
    //                                var service = obj._embedded._state[0];

    //                                // TCP {X, Y, Z} -> Read RWS JSON
    //                                ABB_Stream_Resources_Data_XML.C_Position[0] = (double)service.x;
    //                                ABB_Stream_Resources_Data_XML.C_Position[1] = (double)service.y;
    //                                ABB_Stream_Resources_Data_XML.C_Position[2] = (double)service.z;
    //                                // Quaternion {q1 .. q4} -> Read RWS JSON
    //                                ABB_Stream_Resources_Data_XML.C_Orientation[0] = (double)service.q1;
    //                                ABB_Stream_Resources_Data_XML.C_Orientation[1] = (double)service.q2;
    //                                ABB_Stream_Resources_Data_XML.C_Orientation[2] = (double)service.q3;
    //                                ABB_Stream_Resources_Data_XML.C_Orientation[3] = (double)service.q4;

    //                            }
    //                            catch (Exception e)
    //                            {
    //                                Console.WriteLine(e.Message);
    //                            }
    //                        }
    //                    }

    //                    // t_{1}: Timer stop.
    //                    t.Stop();

    //                    // Recalculate the time: t = t_{1} - t_{0} -> Elapsed Time in milliseconds
    //                    if (t.ElapsedMilliseconds < ABB_Stream_Resources_Data_XML.time_step)
    //                    {
    //                        Thread.Sleep(ABB_Stream_Resources_Data_XML.time_step - (int)t.ElapsedMilliseconds);
    //                    }

    //                    // Reset (Restart) timer.
    //                    t.Restart();
    //                }
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine("Communication Problem: {0}", e);
    //        }
    //    }

    //    public void Start()
    //    {
    //        exit_thread = false;
    //        // Start a thread to stream ABB Robot
    //        robot_thread = new Thread(new ThreadStart(ABB_Stream_Thread_JSON));
    //        robot_thread.IsBackground = true;
    //        robot_thread.Start();
    //        // Thread is active
    //        ABB_Stream_Resources_Data_XML.is_alive = true;
    //    }
    //    public void Stop()
    //    {
    //        exit_thread = true;
    //        // Stop a thread
    //        Thread.Sleep(100);
    //        ABB_Stream_Resources_Data_XML.is_alive = robot_thread.IsAlive;
    //        robot_thread.Abort();
    //    }
    //    public void Destroy()
    //    {
    //        // Stop a thread (Robot Web Services communication)
    //        Stop();
    //        Thread.Sleep(100);
    //    }
    //}

}
