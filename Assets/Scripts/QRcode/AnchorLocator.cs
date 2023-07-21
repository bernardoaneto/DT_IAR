using Microsoft.MixedReality.OpenXR;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class AnchorLocator : MonoBehaviour
{
    public static bool DebugCodes = true; //true

    [SerializeField] private TMPro.TMP_Text title;

    [SerializeField] private TMPro.TMP_Text debug; //apagar este debug
    private int count = 0; //apagar este debug

    private Renderer[] renderers;
    private SpatialGraphNode node;

    private Manager manager; // Reference to the Manager script

    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        manager = FindObjectOfType<Manager>(); // Find the Manager script in the scene

    }

    public void StartLocating(string code, Guid nodeID)
    {
        name = code;
        node = SpatialGraphNode.FromStaticNodeId(nodeID);
        StartCoroutine(Locate());
    }

    private IEnumerator Locate()
    {

        while (true)
        {
            if (node.TryLocate(FrameTime.OnUpdate, out var pose))
            {
                pose = pose.GetTransformedBy(Camera.main.transform.parent);

                count++; //apagar este debug
                debug.text = count.ToString(); //apagar este debug

                if ( ( (transform.position - pose.position).magnitude < 0.0001f ) && ( Quaternion.Angle(transform.rotation, pose.rotation) < 0.1f ) )
                {
                    if (DebugCodes)
                    {
                        foreach (var r in renderers)
                            r.enabled = true;
                        title.text = name + " - position: " + pose.position.ToString() + " - rotation: " + pose.rotation.ToString();
                    }

                    transform.SetPositionAndRotation(pose.position, pose.rotation);

                    // Activate the target object using the Manager script
                    manager.ActivateObject();

                    // Set the anchor as the new parent of the target object using the Manager script
                    manager.SetParent(transform);

                    yield break;
                }

                transform.SetPositionAndRotation(pose.position, pose.rotation);
            }

            yield return null;
        }
    }


}