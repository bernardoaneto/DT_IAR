using UnityEngine;

public class Gauge : DataObject
{
    public override int type => 0;

    [SerializeField] private TMPro.TMP_Text title, value;
    [SerializeField] private Transform needle;
    [SerializeField] private TMPro.TMP_Text[] labels;

    private string table, column;
    private float min, max;

    public override (string, DataType)[] Properties => new[]
    {
        ("Table", DataType.ChooseOne),
        ("Column", DataType.ChooseOne),
        ("Min", DataType.Numeric),
        ("Max", DataType.Numeric)
    };

    public override object[] PropertiesValues => new object[]
    {
        table, column, min, max
    };


    protected override void UpdateElement()
    {
        if (min == max) return;

        var lastValueStr = Table.GetLastValue(table, column);
        if (lastValueStr == null) return;
        if (!float.TryParse(lastValueStr, out var lastValue)) return;

        value.text = $"{lastValue:F2}";
        var r = (lastValue - min) / (max - min);
        r = Mathf.Clamp01(r);
        needle.localRotation = Quaternion.Euler(0, 0, -180 * r);
    }

    public override void SetProperties(object[] values, bool update = false)
    {
        table = (string)values[0];
        column = (string)values[1];
        title.text = $"{table ?? ""}:{column ?? ""}";
        min = (float)values[2];
        max = (float)values[3];
        var d = (max - min) / (labels.Length - 1);
        for (var i = 0; i < labels.Length; i++) labels[i].text = $"{i * d + min:F1}";

        base.SetProperties(values, update);
    }
}