using System;
using UnityEngine;
using UnityEngine.UI;

public class Timeseries : DataObject
{
    public override int type => 2;

    private const int MAX_POINTS = 5000;

    [SerializeField] private Image pointPrefab;
    [SerializeField] private Transform zeroZeroPoint, oneOnePoint;
    [SerializeField] private TMPro.TMP_Text title;
    [SerializeField] private TMPro.TMP_Text[] xText, yText;

    private Image[] points;
    private Vector3 start, delta;
    private string table, column;
    private float min, max, time;

    public override (string, DataType)[] Properties => new[]
    {
        ("Table", DataType.ChooseOne),
        ("Column", DataType.ChooseOne),
        ("Min", DataType.Numeric),
        ("Max", DataType.Numeric),
        ("Time", DataType.Numeric)
    };

    public override object[] PropertiesValues => new object[]
    {
        table, column, min, max, time
    };

    protected override void Start()
    {
        base.Start();
        points = new Image[MAX_POINTS];
        start = zeroZeroPoint.localPosition;
        delta = oneOnePoint.localPosition - start;
        var canvas = GetComponentInChildren<Canvas>().transform;

        for (var i = 0; i < MAX_POINTS; i++)
            points[i] = Instantiate(pointPrefab, canvas);
    }

    protected override void UpdateElement()
    {
        if (min == max) return;

        var times = Table.GetTimestamps(table);
        var values = Table.GetColumn(table, column);
        if (times == null || values == null) return;

        var end = Mathf.Min(times.Length, points.Length);
        for (var i = 0; i < end; i++)
        {
            if (!float.TryParse(values[i], out var value))
            {
                points[i].enabled = false;
                continue;
            }

            var tmp = (float)(DateTime.UtcNow - DateTime.Parse(times[i])).TotalMinutes;
            var x = 1 - tmp / time;
            var y = (value - min) / (max - min);
            x = Mathf.Clamp01(x);
            y = Mathf.Clamp01(y);

            points[i].transform.localPosition = start + Vector3.Scale(new Vector3(x, y), delta);
            points[i].enabled = true;
        }

        for (var i = end; i < points.Length; i++) points[i].enabled = false;

        var d = time / (xText.Length - 1);
        for (var i = 0; i < xText.Length; i++)
        {
            var t = DateTime.UtcNow.AddMinutes(-time + d * i);
            xText[i].text = $"{t:HH:mm:ss}";
        }
    }

    public override void SetProperties(object[] values, bool update = false)
    {
        table = (string)values[0];
        column = (string)values[1];
        title.text = $"{table ?? ""}:{column ?? ""}";
        min = (float)values[2];
        max = (float)values[3];
        var del = (max - min) / (yText.Length - 1);
        for (var i = 0; i < yText.Length; i++) yText[i].text = $"{i * del + min:F1}";
        time = Mathf.Clamp((float)values[4], 0, MAX_POINTS / 60f);

        base.SetProperties(values, update);
    }
}