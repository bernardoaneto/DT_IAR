using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

[RequireComponent(typeof(Collider),
    typeof(ObjectManipulator),
    typeof(NearInteractionGrabbable))]
[RequireComponent(typeof(MoveAxisConstraint),
    typeof(RotationAxisConstraint),
    typeof(MinMaxScaleConstraint))]
public abstract class MovableObject : MonoBehaviour
{
    private const float MAX_DIST = 5f;
    public static readonly List<MovableObject> ObjectCache = new List<MovableObject>();
    public static InteractionType interaction;

    [HideInInspector] public string uid;
    [HideInInspector] public bool isVisible;
    public virtual int type => 0;

    [SerializeField] private GameObject deleteSphere;
    private bool deleteConfirm;

    protected ObjectManipulator manipulator;
    protected NearInteractionGrabbable grabbable;
    protected TransformConstraint[] constraints;
    protected Renderer[] renderers;

    protected virtual void Start()
    {
        ObjectCache.Add(this);
        constraints = new TransformConstraint[3];
        constraints[0]  = GetComponent<MoveAxisConstraint>();
        constraints[1]  = GetComponent<RotationAxisConstraint>();
        constraints[2]  = GetComponent<MinMaxScaleConstraint>();
        renderers       = GetComponentsInChildren<Renderer>();
        manipulator     = GetComponent<ObjectManipulator>();
        grabbable       = GetComponent<NearInteractionGrabbable>();

        uid = Guid.NewGuid().ToString();
        SetConstraints(interaction != InteractionType.Move);
        manipulator.OnManipulationEnded.AddListener(Interact);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
    }

    private void OnDestroy()
    {
        ObjectCache.Remove(this);
    }

    protected virtual void Interact(ManipulationEventData _)
    {
        switch (interaction)
        {
            case InteractionType.View:
                ViewObject();
                break;
            case InteractionType.Edit:
                break;
            case InteractionType.Move:
                break;
            case InteractionType.Delete when deleteConfirm:
                //StateManager.RemoveElement(this);
                Destroy(gameObject);
                break;
            case InteractionType.Delete:
                deleteSphere.SetActive(true);
                deleteConfirm = true;
                break;
        }
    }

    private void FixedUpdate()
    {
        var dist = Vector3.Distance(transform.position, Camera.main.transform.position);
        var angle = Vector3.Angle(transform.forward, Camera.main.transform.forward);

        isVisible = dist < MAX_DIST || dist < 2 * MAX_DIST && angle < 60f;

        if (isVisible == renderers[0].enabled) return;
        foreach (var r in renderers) r.enabled = isVisible;
    }

    private void SetConstraints(bool active)
    {
        foreach (var c in constraints) c.enabled = active;
    }

    public void CancelDelete()
    {
        deleteConfirm = false;
        deleteSphere.SetActive(false);
    }

    protected virtual void ViewObject() { }

    public enum InteractionType
    {
        View,
        Edit,
        Move,
        Delete
    }
}