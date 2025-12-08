using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using Color = UnityEngine.Color;

public class TP2Mesh : MonoBehaviour
{

    private MeshFilter myMF;
    public string fileName;
    public int nbTrianglesToRemove;
    public float size;
    
    public bool reduceTriangles;
    public float tolerance;

    private List<int> triangles = new List<int>();
    private List<Vector3> vertices = null;

   
    private class MyCube
    {
        public Vector3 v0;
        public Vector3 v1;
        public Vector3 v2;
        public Vector3 v3;
        public Vector3 v4;
        public Vector3 v5;
        public Vector3 v6;
        public Vector3 v7;  

        public List<Vector3> verticesInsideCube = new List<Vector3>();
        public List<int> verticesIndexes = new List<int>();
        public MyCube(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6, Vector3 v7)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            this.v4 = v4;
            this.v5 = v5;
            this.v6 = v6;
            this.v7 = v7;
        }

        public bool vextexInsideCube( Vector3 Vertex)
        {
            // front face
            if (Vertex.x < v0.x || Vertex.y < v0.y || Vertex.z  < v0.z) { return false; }
            if (Vertex.x > v1.x || Vertex.y < v1.y || Vertex.z < v1.z) { return false; }
            if (Vertex.x < v2.x || Vertex.y > v2.y || Vertex.z < v2.z) { return false; }
            if (Vertex.x > v3.x || Vertex.y > v3.y || Vertex.z < v3.z) { return false; }

            // back face
            if (Vertex.x < v4.x || Vertex.y < v4.y || Vertex.z > v4.z) { return false; }
            if (Vertex.x > v5.x || Vertex.y < v5.y || Vertex.z > v5.z) { return false; }
            if (Vertex.x < v6.x || Vertex.y > v6.y || Vertex.z > v6.z) { return false; }
            if (Vertex.x > v7.x || Vertex.y > v7.y || Vertex.z > v7.z) { return false; }
            
            return true;
        }
    }


    private List<Vector3> grid = new List<Vector3>();

    private List<MyCube> grid2 = new List<MyCube>();

    private float maxX = 0, maxY = 0, maxZ = 0, minX = float.PositiveInfinity, minY = float.PositiveInfinity, minZ = float.PositiveInfinity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myMF = GetComponent<MeshFilter>();
        myMF.mesh.Clear();

        readOFF();

        if (reduceTriangles)
        {
            createGrid();
            calculateNewVertices();
            cleanUnusedVertices();
            myMF.mesh.Clear();
            myMF.mesh.vertices = vertices.ToArray();
            myMF.mesh.triangles = triangles.ToArray();
        }
   

    }


    private void readOFF()
    {
        string path = "Assets\\";
        path += fileName;


        if (!File.Exists(path))
        {
            Debug.LogError("File Doesn't exist in the assets folder");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        string[] parts = lines[1].Split(' ');
        int nbVertices = int.Parse(parts[0]);
        int nbTriangles = int.Parse(parts[1]);

        Vector3[] tempVertices = new Vector3[nbVertices];
        Vector3 center = Vector3.zero;


        int lineIndex = 2;
        float max = 0;

        for (int j = 0; j < tempVertices.Length; lineIndex++, j++)
        {

            parts = lines[lineIndex].Split(' ');

            Vector3 aVertice = new Vector3(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture), float.Parse(parts[2], CultureInfo.InvariantCulture));

            if (Mathf.Abs(aVertice.x) > max) { max = Mathf.Abs(aVertice.x); }
            if (Mathf.Abs(aVertice.y) > max) { max = Mathf.Abs(aVertice.y); }
            if (Mathf.Abs(aVertice.z) > max) { max = Mathf.Abs(aVertice.z); }

            center += aVertice;
            tempVertices[j] = aVertice;

        }


        center = new Vector3(center.x / nbVertices, center.y / nbVertices, center.z / nbVertices);

        for (int j = 0; j < tempVertices.Length; j++)
        {
            tempVertices[j] -= center;
            tempVertices[j] = new Vector3(tempVertices[j].x / max, tempVertices[j].y / max, tempVertices[j].z / max);
            tempVertices[j] = new Vector3(tempVertices[j].x * size, tempVertices[j].y * size, tempVertices[j].z * size);

            if (tempVertices[j].x > maxX) { maxX = tempVertices[j].x; }
            if (tempVertices[j].y > maxY) { maxY = tempVertices[j].y; }
            if (tempVertices[j].z > maxZ) { maxZ = tempVertices[j].z; }

            if (tempVertices[j].x < minX) { minX = tempVertices[j].x; }
            if (tempVertices[j].y < minY) { minY = tempVertices[j].y; }
            if (tempVertices[j].z < minZ) { minZ = tempVertices[j].z; }
            
        }

        vertices = tempVertices.ToList();

        Vector3[] trianglesNormals = new Vector3[nbTriangles];
        for (int i = 0; i < nbTriangles; lineIndex++, i++)
        {
            parts = lines[lineIndex].Split(' ');

            triangles.Add(int.Parse(parts[1]));
            triangles.Add(int.Parse(parts[2]));
            triangles.Add(int.Parse(parts[3]));



            Vector3 A = tempVertices[int.Parse(parts[2])] - tempVertices[int.Parse(parts[1])];
            Vector3 B = tempVertices[int.Parse(parts[3])] - tempVertices[int.Parse(parts[1])];


            Vector3 Normal = Vector3.Cross(A, B);
            trianglesNormals[i] = Normal.normalized;

        }


        Vector3[] verticesNormals = new Vector3[tempVertices.Length];
        for (int i = 0; i < tempVertices.Length; i++)
        {
            int nbNormals = 0;
            Vector3 aVerticeNormal = Vector3.zero;
            for (int j = 0, k = 0; j < trianglesNormals.Length; j++, k += 3)
            {

                if (triangles[k] == i) { aVerticeNormal += trianglesNormals[j]; nbNormals++; }
                if (triangles[k + 1] == i) { aVerticeNormal += trianglesNormals[j]; nbNormals++; }
                if (triangles[k + 2] == i) { aVerticeNormal += trianglesNormals[j]; nbNormals++; }

            }

            verticesNormals[i] = (aVerticeNormal / nbNormals).normalized;
        }




        myMF.mesh.vertices = vertices.ToArray();
        myMF.mesh.triangles = triangles.ToArray();
        myMF.mesh.normals = verticesNormals;

    }


     private void createGrid()
    {
        int nbVertexs = 0;

        for (float z = minZ; z < maxZ  ; z += tolerance)
        {
            for (float y = minY; y < maxY ; y += tolerance)
            {
                for (float x = minX; x < maxX ; x += tolerance)
                {
                      Vector3 v0 = new Vector3(x, y, z);
                      Vector3 v1 = new Vector3(x + tolerance, y, z); 

                      Vector3 v2 = new Vector3(x , y + tolerance, z);
                      Vector3 v3 = new Vector3(x + tolerance, y + tolerance, z );

                      Vector3 v4 = new Vector3(x, y, z + tolerance);
                      Vector3 v5 = new Vector3(x + tolerance, y, z + tolerance);

                      Vector3 v6 = new Vector3(x, y + tolerance, z + tolerance);
                      Vector3 v7 = new Vector3(x + tolerance, y + tolerance, z + tolerance);

                    MyCube temp = new MyCube(v0, v1, v2, v3, v4, v5, v6,v7);
                    grid2.Add(temp);

                   
                }
            }
        }


        foreach(MyCube aCube in grid2)
        {
            int i = 0;
            foreach (Vector3 vertice in vertices)
            {
                if (aCube.vextexInsideCube(vertice))
                {
                    aCube.verticesInsideCube.Add(vertice);
                    aCube.verticesIndexes.Add(i);

                    nbVertexs++;
                }

                i++;
            }


        }

        Debug.Log("nbVertexes = " +  nbVertexs);
        Debug.Log("vertices.count() = " + vertices.Count());

    }

    private void calculateNewVertices()
    {
        foreach (MyCube aCube in grid2)
        {
            if (aCube.verticesIndexes.Count == 0){ continue; }

            Vector3 averageVert = Vector3.zero;

            for (int i = 0; i < aCube.verticesInsideCube.Count; i++)
            {
                averageVert += aCube.verticesInsideCube[i];
                
            }

            averageVert /= aCube.verticesInsideCube.Count();

            vertices[aCube.verticesIndexes[0]] = averageVert;


            for (int i = 1; i < aCube.verticesIndexes.Count; i++)
            {
                for(int j = 0; j < triangles.Count;  j++)
                {
                    if(triangles[j] == aCube.verticesIndexes[i])
                    {
                        triangles[j] = aCube.verticesIndexes[0];
                    }
                }

            }


        }





    }


    private void cleanUnusedVertices()
    {
        Dictionary<int, int> remap = new Dictionary<int, int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        int nextIndex = 0;

        for (int i = 0; i < triangles.Count; i++)
        {
            int oldIndex = triangles[i];

            if (!remap.TryGetValue(oldIndex, out int newIndex))
            {
                newIndex = nextIndex++;
                remap.Add(oldIndex, newIndex);
                newVertices.Add(vertices[oldIndex]);
            }

            newTriangles.Add(newIndex);
        }

        vertices = newVertices;
        triangles = newTriangles;
    }

    


    private void OnDrawGizmos()
    {
        foreach (Vector3 v in grid) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(v, 0.01f);   
        }

        foreach (MyCube cb in grid2)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(cb.v0, 0.01f);
            Gizmos.DrawSphere(cb.v1, 0.01f);
            Gizmos.DrawSphere(cb.v2, 0.01f);
            Gizmos.DrawSphere(cb.v3, 0.01f);

            Gizmos.DrawSphere(cb.v4, 0.01f);
            Gizmos.DrawSphere(cb.v5, 0.01f);
            Gizmos.DrawSphere(cb.v6, 0.01f);
            Gizmos.DrawSphere(cb.v7, 0.01f);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(maxX,maxY,maxZ), 0.01f);   

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(minX, minY, minZ), 0.01f);   
    }



}
