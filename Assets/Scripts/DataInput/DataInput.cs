using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

public struct DataInput
{
    public readonly string table;
    public readonly List<Dictionary<string, string>> rows;

    public DataInput(string table, in List<Dictionary<string, string>> data)
    {
        this.table = table;
        rows = data;
    }

    public DataInput(string table, string data)
    {
        this.table = table.ToUpper();
        var array = JArray.Parse(data);
        if (array.Count <= 0)
        {
            throw new Exception("Empty array");
        }
        rows = new List<Dictionary<string, string>>();
        foreach (var obj in array)
        {
            var dict = obj.ToObject<Dictionary<string, string>>();
            var newDict = new Dictionary<string, string>();

            for (int i = 0; i < dict.Count(); i++) {

                var pair = dict.ElementAt(i);
                var value = pair.Value;
                var key = pair.Key;

                newDict[key.ToUpper()] = value;
            }
            //foreach (var (key, value) in dict) newDict[key.ToUpper()] = value;
            rows.Add(newDict);
        }
    }
}