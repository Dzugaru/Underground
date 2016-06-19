using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Map
{
    public readonly int W, H;
    public byte[,] T;

    public Map(int w, int h)
    {
        this.W = w;
        this.H = h;
        this.T = new byte[h, w];
    }

    public Map(CellularAutomaton ca, int border)
    {
        this.W = ca.Width + border * 2;
        this.H = ca.Height + border * 2;
        this.T = new byte[H, W];

        for (int i = 0; i < H; i++)
            for (int j = 0; j < W; j++)
            {
                bool outside = i < border || j < border || i >= (H - border) || j >= (W - border);
                T[i, j] = outside ? (byte)0 : ca[i - border, j - border];
            }
    }
}

