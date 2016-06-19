using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class CellularAutomaton
{
    class RuleParseException : Exception
    {

    }

    bool[] survive, beBorn;
    byte[,] cells;
    byte[] updateBuf; //One-line buffer for inplace update

    public readonly int Width, Height;    

    public string Rule
    {
        get { return GetRule(); }
        set { SetRule(value); }
    }

    public byte this[int y, int x]
    {
        get { return cells[y + 1, x + 1]; }
        set { cells[y + 1, x + 1] = value; }
    }

    public CellularAutomaton(int w, int h, string rule)
    {
        this.Width = w;
        this.Height = h;
        this.survive = new bool[9];
        this.beBorn = new bool[9];
        this.updateBuf = new byte[w + 1];
        cells = new byte[h + 2, w + 2];
        Rule = rule;        
    }    

    public void UpdateTiles()
    {
        //Initially fill the buffer
        for (int j = 0; j < this.Width; j++)
            this.updateBuf[j + 1] = GetNextCellState(0, j);

        //Process rest of the tiles
        int i;
        for (i = 1; i < this.Height; i++)
        {
            this.updateBuf[0] = GetNextCellState(i, 0);
            int j;
            for (j = 1; j < this.Width; j++)
            {
                byte nextState = GetNextCellState(i, j);
                this[i - 1, j - 1] = this.updateBuf[j];
                this.updateBuf[j] = this.updateBuf[0];
                this.updateBuf[0] = nextState;
            }
            this[i - 1, j - 1] = this.updateBuf[j];
            this.updateBuf[j] = this.updateBuf[0];
        }

        for (int j = 0; j < this.Width; j++)
            this[i - 1, j] = this.updateBuf[j + 1];
    }

    int GetOnNeighbours(int y, int x)
    {
        int count = 0;
        for (int i = -1; i <= 1; i++)        
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;
                count += this[y + i, x + j];
            }

        //Debug.Log("Cell ON neighbours (" + y + "," + x + ") - " + count);
        return count;    
    }

    byte GetNextCellState(int y, int x)
    {
        byte currentState = this[y, x];
        byte nextState;
        int onCount = GetOnNeighbours(y, x);

        if(currentState == 0)
        {
            nextState = (byte)(beBorn[onCount] ? 1 : 0);
        }
        else
        {
            nextState = (byte)(survive[onCount] ? 1 : 0);
        }

        //Debug.Log("Cell state (" + y + "," + x + ") - " + nextState);
        return nextState;
    }

    void SetRule(string rule)
    {
        string[] sbParts = rule.Split('/');
        if (sbParts.Length != 2) throw new RuleParseException();

        for (int i = 0; i < 2; i++)
        {
            bool[] currentList = (i == 0) ? survive : beBorn;
            for (int j = 0; j < 9; j++)
                currentList[j] = false;

            int lastC = -1;
            foreach (char cn in sbParts[i])
            {
                uint c;
                if (!uint.TryParse(cn.ToString(), out c) || c > 8 || currentList[c] || c < lastC) throw new RuleParseException();
                currentList[c] = true;
                lastC = (int)c;
            }
        }
    }

    string GetRule()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < 2; i++)
        {
            bool[] currentList = (i == 0) ? survive : beBorn;
            for (int j = 0; j < 9; j++)
                if (currentList[j]) sb.Append(j.ToString());

            if (i == 0) sb.Append("/");
        }
        return sb.ToString();
    }
}

