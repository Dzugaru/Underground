using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour
{
    TestCellularAutomaton testCA;
    Map map;

	void Start ()
    {
        //CellularAutomaton ca = new CellularAutomaton(50, 50, "45678/5678");
        ////ca[1, 0] = ca[1, 1] = ca[1, 2] = 1; //Blinker
        ////ca[1, 1] = ca[1, 2] = ca[1, 3] =
        ////ca[2, 0] = ca[2, 1] = ca[2, 2] = 1; //Toad

        //for (int i = 0; i < 50; i++)
        //{
        //    for (int j = 0; j < 50; j++)
        //    {
        //        ca[i,j] = (byte)(Random.value < 0.55f ? 1 : 0);
        //    }
        //}

        //testCA = new TestCellularAutomaton(ca);

        CellularAutomaton ca = new CellularAutomaton(100, 50, "45678/5678");
        for (int i = 0; i < 50; i++)
            for (int j = 0; j < 100; j++)
                ca[i, j] = (byte)(Random.value < 0.55f ? 1 : 0);

        for (int i = 0; i < 10; i++)
            ca.UpdateTiles();

        //Map map = new Map(4, 4);
        //map.T = new byte[,]
        //{
        //    { 0,1,0,1 },
        //    { 1,0,1,0 },
        //    { 1,1,1,1 },
        //    { 1,1,1,1 }
        //};

        map = new Map(ca, 3);
        GameObject mapVisual = GameObject.Find("Map");
        mapVisual.GetComponent<MapVisual>().Build(map, 1, 1);
	}
	
	
	void Update ()
    {
	    //if(Input.GetKeyDown(KeyCode.Space))
        //{
        //   testCA.Update();
        //}
	}

    void OnDrawGizmos()
    {
        if (map != null)
        {
            for (int i = 0; i < map.H; i++)
            {
                for (int j = 0; j < map.W; j++)
                {
                    Gizmos.color = map.T[i, j] == 1 ? Color.white : Color.blue;
                    Gizmos.DrawCube(new Vector3(j, 0, i), Vector3.one * 0.2f);
                }
            }
        }
    }    
}
