using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DataObject : MovableObject
{
    //public static readonly Dictionary<string, MachineAbstraction> abstractions = new(); //tirar linha
    public static readonly List<DataObject> DataObjectCache = new List<DataObject>();
    //public static MachineAbstraction AbstractionPrefab;

    [SerializeField][Range(0.01f, 5f)] private float refreshRate;

    public abstract (string, DataType)[] Properties { get; }
    public abstract object[] PropertiesValues { get; }

    protected override void Start()
    {
        base.Start();

        DataObjectCache.Add(this);
        //StateManager.UpdateElement(this); //tirar
        StartCoroutine(UpdateElementCoroutine());
    }

    private void OnDestroy()
    {
        DataObjectCache.Remove(this);
    }

    protected override void Interact(ManipulationEventData arg)
    {
        base.Interact(arg);
        switch (interaction)
        {
            case InteractionType.View:
                break;
            case InteractionType.Edit:
                //EditorMenu.Instance.OpenMenu(this, Properties, PropertiesValues);
                break;
            case InteractionType.Move:
                //foreach (var (_, v) in abstractions) v.UpdatePositions();
                break;
            case InteractionType.Delete:
                //foreach (var (k, v) in abstractions) v.UpdateObjects(k);
                break;
        }
    }

    private IEnumerator UpdateElementCoroutine()
    {
        var delay = false;
        var delayedRefreshRate = 30 * refreshRate;

        while (true)
        {
            try
            {
                UpdateElement();
            }
            catch
            {
                delay = true;
            }
            yield return new WaitForSecondsRealtime(delay ? delayedRefreshRate : refreshRate);
            delay = false;
        }
    }

    protected abstract void UpdateElement();

    public virtual void SetProperties(object[] values, bool update = false)
    {
        //if (update) StateManager.UpdateElement(this);

        //foreach (var (k,v) in abstractions) v.UpdateObjects(k);
    }

    public static void AddNewMachine(string table)
    {
        //var o = Instantiate(AbstractionPrefab, null);
        //abstractions[table] = o;
    }

    public enum DataType
    {
        Numeric,
        ChooseOne
    }
}