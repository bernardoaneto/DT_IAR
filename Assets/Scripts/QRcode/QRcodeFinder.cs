using System;
using System.Collections;

using System.Collections.Concurrent;
using UnityEngine;
using Microsoft.MixedReality.QR;
using System.Linq;

using Microsoft.MixedReality.OpenXR;


using Microsoft.MixedReality.Toolkit.Utilities;

public class QRcodeFinder : MonoBehaviour
{

    private static readonly string[] ANCHOR_CODES = { "NAVE0004" };
    private const string CODE_START = "HOLOLENS-";

    private static readonly ConcurrentDictionary<string, Guid> newCodes = new ConcurrentDictionary<string, Guid>();

    private static QRCodeWatcher qrWatcher;

    [SerializeField] public AnchorLocator prefab;
    //[SerializeField] private TMPro.TMP_Text debug_text;
    public static bool prefabInstantiated = false;

    private delegate void OnCodeFoundHandler(string code, Guid id);

    private event OnCodeFoundHandler OnCodeFound;

    // Start is called before the first frame update
    private async void Start()
    {
        if (!QRCodeWatcher.IsSupported())
        {
            Debug.Log("State Manager stopped: QR Codes not supported");
            Destroy(this);
            return;
        }

        var status = await QRCodeWatcher.RequestAccessAsync();
        if (status != QRCodeWatcherAccessStatus.Allowed)
        {
            Debug.Log($"State Manager stopped: QR Codes access denied");
            Destroy(this);
            return;
        }

        qrWatcher = new QRCodeWatcher();
        qrWatcher.Updated += QRCodeUpdated;
        qrWatcher.Start();
        Debug.Log("State Manager started");

    }

    private static void QRCodeUpdated(object sender, QRCodeUpdatedEventArgs e)
    {
        if (!e.Code.Data.StartsWith(CODE_START)) return;
        //var code = e.Code.Data[CODE_START.Length..];
        var code = e.Code.Data.Substring(CODE_START.Length);
        var id = e.Code.SpatialGraphNodeId;
        newCodes[code] = id;
    }


    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i < newCodes.Count(); i++)
        {
            var p = newCodes.ElementAt(i);
            var value = p.Value;
            var key = p.Key;

            OnCodeFound?.Invoke(key, value);

            if (!prefabInstantiated)
            {
                if (!Array.Exists(ANCHOR_CODES, x => x.Equals(key))) continue;
                else
                {
                    //debug_text.text = (!Array.Exists(ANCHOR_CODES, x => x.Equals(key))).ToString();
                    
                    var obj = Instantiate(prefab);
                    obj.StartLocating(key, value);

                    prefabInstantiated = true;
                }
            }

        }

        newCodes.Clear();

    }
}
