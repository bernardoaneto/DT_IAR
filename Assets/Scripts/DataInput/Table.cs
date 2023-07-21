using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Table
{
    private const string timestampIndex = "TIMESTAMP";
    public static int MaxSize = 5000;

    public static readonly Dictionary<string, Table> Tables = new Dictionary<string, Table>();

    private readonly List<string> ColumnNames;
    private readonly List<Dictionary<string, string>> Rows = new List<Dictionary<string, string>>();

    private Table(in DataInput data)
    {
        UpdateTable(data.rows);
        ColumnNames = new List<string>(Rows[0].Keys);
        Tables[data.table] = this;
        Debug.Log($"Add table {data.table}");
    }

    public static void UpdateTable(in DataInput data)
    {
        if (!TableExists(data.table))
        {
            _ = new Table(data);
            DataObject.AddNewMachine(data.table);
        }
        else
        {
            Tables[data.table].UpdateTable(data.rows);
        }
    }

    private void UpdateTable(in List<Dictionary<string, string>> rows)
    {
        foreach (var row in rows) Rows.Add(row);
        Rows.Sort((a, b) =>
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;
            return string.Compare(a[timestampIndex], b[timestampIndex], StringComparison.Ordinal);
        });
        for (var i = 0; i < Rows.Count - 1;)
        {
            if (Rows[i][timestampIndex] != Rows[i + 1][timestampIndex]) i++;
            else Rows.RemoveAt(i);
        }

        while (Rows.Count > MaxSize) Rows.RemoveAt(0);
    }

    public static string[] GetTimestamps(string table)
    {
        if (!TableExists(table)) return new string[] { };
        var rows = Tables[table].Rows;
        var result = new string[rows.Count];
        for (var i = 0; i < rows.Count; i++) result[i] = rows[i][timestampIndex];
        return result;
    }

    public static string[] GetColumn(string table, string column)
    {
        if (!ColumnExists(table, column)) return new string[] { };
        var rows = Tables[table].Rows;
        var result = new string[rows.Count];
        for (var i = 0; i < rows.Count; i++) result[i] = rows[i][column];
        return result;
    }

    public static string GetLastValue(string table, string column)
    {
        if (!ColumnExists(table, column)) return null;
        var rows = Tables[table].Rows;
        return rows.Last()[column];
    }

    private static bool TableExists(string table)
    {
        return table != null && Tables.ContainsKey(table);
    }

    private static bool ColumnExists(string table, string column)
    {
        return TableExists(table) && column != null && Tables[table].ColumnNames.Contains(column);
    }

    public static bool IsOnline(string table)
    {
        var last = DateTime.Parse(GetTimestamps(table).Last());
        var diff = last.Subtract(DateTime.UtcNow).TotalMinutes;
        return Math.Abs(diff) < 1;
    }

    public static string[] GetColumnNames(string machine)
    {
        if (!TableExists(machine)) return new string[] { };
        var result = new List<string>(Tables[machine].ColumnNames);
        result.Remove(timestampIndex);
        result.Remove($"ID{machine}");
        return result.ToArray();
    }

    public static string[] GetNumericColumnNames(string table)
    {
        if (!TableExists(table)) return new string[] { };
        var result = new List<string>(Tables[table].ColumnNames);
        result.Remove(timestampIndex);
        result.Remove($"ID{table}");
        for (var i = result.Count - 1; i >= 0; i--)
        {
            if (!float.TryParse(GetLastValue(table, result[i]), out _))
            {
                result.RemoveAt(i);
            }
        }
        return result.ToArray();
    }
}