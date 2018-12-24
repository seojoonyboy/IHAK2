using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using CSV_Utill;

public class CSVReader : MonoBehaviour {
    
    public TextAsset csvFile;
    DataManager data;

	void Start () {
        data = DataManager.Instance;
        InitCSVFiles();
    }
    private void Update() {
        Debug.Log(data.buildings[0].HP);
    }

    private void InitCSVFiles() {
        csvFile = Resources.Load("CSV/Building") as TextAsset;
        string[,] building = SplitCsvGrid(csvFile.text);
        GridToClass(DataType.BUILDING, ref building);
    }

    static public string[,] SplitCsvGrid(string csvText) {
        string[] lines = csvText.Split("\n"[0]);

        // finds the max width of row
        int width = 0;
        for (int i = 0; i < lines.Length; i++) {
            string[] row = SplitCsvLine(lines[i]);
            width = Mathf.Max(width, row.Length);
        }

        string[,] outputGrid = new string[lines.Length, width];
        for (int x = 0; x < lines.Length; x++) {
            string[] row = SplitCsvLine(lines[x]);
            for (int y = 0; y < width; y++) {
                outputGrid[x, y] = row[y];
            }
        }

        return outputGrid;
    }

    static public string[] SplitCsvLine(string line) {
        return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
        @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
        System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
                select m.Groups[1].Value).ToArray();
    }

    public void GridToClass(DataType type, ref string[,] classData) {
        int colums = classData.GetLength(0);
        int index = 1;
        switch(type) {
            case DataType.BUILDING:
                data.buildings = new Building[colums];
                for(int i =0; i<colums; i++) {
                    if (index > colums - 1) { return; }

                    Building build = new Building();
                    build.ID = Int32.Parse(classData[index, 0]);
                    build.Tier = Int32.Parse(classData[index, 1]);
                    build.Name = classData[index, 2];
                    build.HP = Int32.Parse(classData[index, 3]);
                    data.buildings[i] = build;
                }
                break;
        }
    }

    public enum DataType {
        BUILDING,
        UNIT
    }


}
