using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TestCellularAutomaton
{
    CellularAutomaton ca;

    public TestCellularAutomaton(CellularAutomaton ca)
    {
        this.ca = ca;
    }

    public void Update()
    {
        this.ca.UpdateTiles();
    }

    public void DrawByGizmos(float size)
    {
        for (int i = 0; i < ca.Height; i++)
        {
            for (int j = 0; j < ca.Width; j++)
            {
                Gizmos.color = ca[i, j] == 1 ? Color.white : Color.blue;
                Gizmos.DrawCube(new Vector3(i - ca.Height / 2, j - ca.Width / 2, 0) * size, Vector3.one * 0.9f * size);
            }
        }
    }
}

