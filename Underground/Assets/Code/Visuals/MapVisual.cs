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
        
        MapBuilder mb = new MapBuilder(map, cellSz, wallH);
        MapMesh mapMesh = mb.Build();

        Mesh floor = CreateMesh(mapMesh.Floor);
        Mesh ceil = CreateMesh(mapMesh.Ceil);
        Mesh walls = CreateMesh(mapMesh.Walls);

        transform.Find("Floor").GetComponent<MeshFilter>().sharedMesh = floor;
        transform.Find("Ceiling").GetComponent<MeshFilter>().sharedMesh = ceil;
        transform.Find("Walls").GetComponent<MeshFilter>().sharedMesh = walls;



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

    Mesh CreateMesh(MeshData data)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = data.Verts.ToArray();
        mesh.triangles = data.Tris.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    class MeshData
    {
        public List<Vector3> Verts = new List<Vector3>();
        public List<int> Tris = new List<int>();
    }

    class MapMesh
    {
        public MeshData Floor = new MeshData();
        public MeshData Ceil = new MeshData();
        public MeshData Walls = new MeshData();
    }

    class MapBuilder
    {
        Map map;
        float cellSz2, wallHeight;

        MapMesh mapMesh;
        bool buildWalls;

        MeshData mesh;

        int bi, bj;
        List<Vector3> wallSection;

        //Indices of already added vertices, which we'll reuse if avail        
        int[,] existCell;
        int[] existLine;        

        public MapBuilder(Map map, float cellSz, float wallHeight)
        {
            this.map = map;
            this.cellSz2 = cellSz * 0.5f;
            this.wallHeight = wallHeight;
            this.mapMesh = new MapMesh();
        }

        public MapMesh Build()
        {
            Build(mapMesh.Floor, false, true);
            Build(mapMesh.Ceil, true, false);

            return mapMesh;
        }

        void Build(MeshData mesh, bool inverted, bool buildWalls)
        {
            this.buildWalls = buildWalls;
            this.mesh = mesh;

            InitExist();

            for (bi = 0; bi < map.H - 1; bi++)
            {
                for (bj = 0; bj < map.W - 1; bj++)
                {
                    Preproc();

                    int c = map.T[bi, bj] | (map.T[bi, bj + 1] << 1) |
                            (map.T[bi + 1, bj + 1] << 2) | (map.T[bi + 1, bj] << 3);

                    if(inverted) c = (~c) & (0xF);

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

                    if(this.buildWalls) AddWallSection();
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

            wallSection = new List<Vector3>();
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
            MeshData w = mapMesh.Walls;

            for (int i = 0; i < wallSection.Count; i += 4)
            {
                int tid = w.Verts.Count;
                for (int j = 0; j < 4; j++)                
                    w.Verts.Add(wallSection[i + j]);

                w.Tris.Add(tid);
                w.Tris.Add(tid + 1);
                w.Tris.Add(tid + 3);
                w.Tris.Add(tid);
                w.Tris.Add(tid + 3);
                w.Tris.Add(tid + 2);
            }
        }

        void AddVert(int x, int y, bool edge = false)
        {           
            int vi = existCell[y, x];
            
            if (vi != -1)
            {
                mesh.Tris.Add(vi);
            }
            else
            {
                mesh.Tris.Add(mesh.Verts.Count);
                vi = mesh.Verts.Count;
                mesh.Verts.Add(new Vector3(2 * bj + x, 0, 2 * bi + y) * cellSz2);
            }

            existCell[y, x] = vi;         
            
            if(this.buildWalls && edge)
            {
                wallSection.Add(new Vector3(2 * bj + x, 0, 2 * bi + y) * cellSz2);
                wallSection.Add(new Vector3((2 * bj + x) * cellSz2, wallHeight, (2 * bi + y) * cellSz2));
            }
        }
    }   
}

