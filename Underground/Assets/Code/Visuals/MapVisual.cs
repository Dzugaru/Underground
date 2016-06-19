using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MapVisual : MonoBehaviour
{
    public float CellSize;

    public void Build(Map map, float cellSz, float wallH)
    {
        CellSize = cellSz;

        List<Vector3> v = new List<Vector3>();
        List<int> t = new List<int>();
        MapBuilder mb = new MapBuilder(map, cellSz, wallH,  v, t);
        mb.Build();

        Mesh mesh = new Mesh();
        mesh.vertices = v.ToArray();
        mesh.triangles = t.ToArray();
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().sharedMesh = mesh;        


        //Mesh mesh = new Mesh();
        //mesh.vertices = new Vector3[3]
        //{
        //    new Vector3(0,0,0),
        //    new Vector3(0,0,1),
        //    new Vector3(1,0,0)
        //};
        //mesh.triangles = new int[3]
        //{
        //    0,1,2
        //};        

        //mesh.RecalculateNormals();

        //GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    class MapBuilder
    {
        Map map;
        float cellSz2, wallHeight;
        List<Vector3> verts;
        List<int> tris;

        int bi, bj;
        List<Vector3> walls;

        //Indices of already added vertices, which we'll reuse if avail        
        int[,] existCell;
        int[] existLine;        

        public MapBuilder(Map map, float cellSz, float wallHeight, List<Vector3> v, List<int> t)
        {
            this.map = map;
            this.cellSz2 = cellSz * 0.5f;
            this.wallHeight = wallHeight;
            this.verts = v;
            this.tris = t;
        }

        public void Build()
        {
            InitExist();

            for (bi = 0; bi < map.H - 1; bi++)
            {
                for (bj = 0; bj < map.W - 1; bj++)
                {
                    Preproc();

                    int c = map.T[bi, bj] | (map.T[bi, bj + 1] << 1) |
                            (map.T[bi + 1, bj + 1] << 2) | (map.T[bi + 1, bj] << 3);

                    switch (c)
                    {
                        case 0: break;

                        case 1: AddVert(0, 1, true); AddVert(1, 0, true); AddVert(0, 0); break;
                        case 2: AddVert(1, 0, true); AddVert(2, 1, true); AddVert(2, 0); break;
                        case 4: AddVert(2, 1, true); AddVert(1, 2, true); AddVert(2, 2);  break;
                        case 8: AddVert(1, 2, true); AddVert(0, 1, true); AddVert(0, 2);  break;

                        case 3:
                            AddVert(0, 1, true); AddVert(2, 1, true); AddVert(2, 0);
                            AddVert(0, 0); AddVert(0, 1); AddVert(2, 0);                            
                            break;
                        case 6:
                            AddVert(1, 0, true); AddVert(1, 2, true); AddVert(2, 2);
                            AddVert(2, 0); AddVert(1, 0); AddVert(2, 2);                            
                            break;
                        case 12:
                            AddVert(2, 1, true); AddVert(0, 1, true); AddVert(0, 2);
                            AddVert(0, 2); AddVert(2, 2); AddVert(2, 1);                            
                            break;
                        case 9:
                            AddVert(1, 2, true); AddVert(1, 0, true); AddVert(0, 0);
                            AddVert(0, 0); AddVert(0, 2); AddVert(1, 2);                            
                            break;

                        case 5:
                            AddVert(0, 1, true); AddVert(1, 2, true); AddVert(1, 0);
                            AddVert(2, 1, true); AddVert(1, 0, true); AddVert(1, 2);
                            AddVert(0, 0); AddVert(0, 1); AddVert(1, 0);
                            AddVert(1, 2); AddVert(2, 2); AddVert(2, 1);
                            break;
                        case 10:
                            AddVert(1, 0, true); AddVert(0, 1, true); AddVert(1, 2);
                            AddVert(1, 2, true); AddVert(2, 1, true); AddVert(1, 0);
                            AddVert(1, 0); AddVert(2, 1); AddVert(2, 0);
                            AddVert(0, 1); AddVert(0, 2); AddVert(1, 2);                            
                            break;

                        case 14:
                            AddVert(1, 0, true); AddVert(0, 1, true); AddVert(2, 2);
                            AddVert(0, 1); AddVert(0, 2); AddVert(2, 2);
                            AddVert(1, 0); AddVert(2, 2); AddVert(2, 0);
                            break;
                        case 13:
                            AddVert(2, 1, true); AddVert(1, 0, true); AddVert(0, 2);
                            AddVert(0, 0); AddVert(0, 2); AddVert(1, 0);                            
                            AddVert(2, 1); AddVert(0, 2); AddVert(2, 2);
                            break;
                        case 11:
                            AddVert(1, 2, true); AddVert(2, 1, true); AddVert(0, 0);
                            AddVert(0, 0); AddVert(2, 1); AddVert(2, 0);                            
                            AddVert(0, 0); AddVert(0, 2); AddVert(1, 2);
                            break;
                        case 7:
                            AddVert(0, 1, true); AddVert(1, 2, true); AddVert(2, 0);
                            AddVert(0, 0); AddVert(0, 1); AddVert(2, 0);                            
                            AddVert(1, 2); AddVert(2, 2); AddVert(2, 0);
                            break;

                        case 15:
                            AddVert(0, 0); AddVert(0, 2); AddVert(2, 0);
                            AddVert(0, 2); AddVert(2, 2); AddVert(2, 0);
                            break;
                    }

                    Postproc();
                    AddWallSection();
                }

                PostprocRow();
            }            
        }
        
        void InitExist()
        {
            existLine = new int[map.W * 2];
            existCell = new int[3,3];

            for (int i = 0; i < existLine.Length; i++)
                existLine[i] = -1;            

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    existCell[i, j] = -1;            
        }      
        
        void Preproc()
        {
            if (existLine[2 * bj] != -1) existCell[0, 0] = existLine[2 * bj];
            for (int j = 1; j < 3; j++) existCell[0, j] = existLine[2 * bj + j];

            walls = new List<Vector3>();
        } 
        
        void Postproc()
        {
            existLine[2 * bj] = existCell[2, 0];
            existLine[2 * bj + 1] = existCell[2, 1];

            for (int i = 0; i < 3; i++)
                existCell[i, 0] = existCell[i, 2];

            for (int i = 0; i < 3; i++)
                for (int j = 1; j < 3; j++)
                    existCell[i, j] = -1;

            

            //Debug.Log("(" + bi + "," + bj + ") - ex: (" + string.Join(",", ex.Select(e => e.ToString()).ToArray()) + "), l: " + l + ", lt: " + lt);
        }

        void PostprocRow()
        {
            existLine[2 * bj] = existCell[2, 0];

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    existCell[i, j] = -1;           

            //Debug.Log("bi: " + bi + ", ex: " + string.Join(",", exLine.Select(e => e.ToString()).ToArray()));
        }

        void AddWallSection()
        {
            for (int i = 0; i < walls.Count; i += 4)
            {
                int tid = verts.Count;
                for (int j = 0; j < 4; j++)                
                    verts.Add(walls[i + j]);

                tris.Add(tid);
                tris.Add(tid + 1);
                tris.Add(tid + 3);
                tris.Add(tid);
                tris.Add(tid + 3);
                tris.Add(tid + 2);
            }
        }

        void AddVert(int x, int y, bool edge = false)
        {           
            int vi = existCell[y, x];
            
            if (vi != -1)
            {
                tris.Add(vi);
            }
            else
            {
                tris.Add(verts.Count);
                vi = verts.Count;
                verts.Add(new Vector3(2 * bj + x, 0, 2 * bi + y) * cellSz2);
            }

            existCell[y, x] = vi;         
            
            if(edge)
            {
                walls.Add(new Vector3(2 * bj + x, 0, 2 * bi + y) * cellSz2);
                walls.Add(new Vector3((2 * bj + x) * cellSz2, wallHeight, (2 * bi + y) * cellSz2));
            }
        }
    }   
}

