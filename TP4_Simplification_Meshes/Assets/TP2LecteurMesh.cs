using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

public class TP2Mesh : MonoBehaviour
{

    private MeshFilter myMF;
    public string fileName;
    public int nbTrianglesToRemove;
    public float size;

    private List<int> triangles = new List<int>();
    private List<Vector3> vertices = null;

    private class MyCube
    {
       List<Vector3> points = new List<Vector3>();
    }

    private List<Vector3> grid = new List<Vector3>();
    
    private float maxX = 0, maxY = 0, maxZ = 0, minX = float.PositiveInfinity, minY = float.PositiveInfinity, minZ = float.PositiveInfinity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myMF = GetComponent<MeshFilter>();
        myMF.mesh.Clear();

        readOFF();
        //exportFile();
        createGrid();
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

       


        vertices = tempVertices.ToList();

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

        Debug.Log("Max = " + new Vector3(maxX,maxY,maxZ));
       Debug.Log("Min = " + new Vector3(minX, minY,minZ));

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




        myMF.mesh.vertices = tempVertices;
        myMF.mesh.triangles = triangles.ToArray();
        myMF.mesh.normals = verticesNormals;

    }


    private void exportFile()
    {
        int nbTriToRemove = nbTrianglesToRemove;
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

        if (nbTriToRemove > nbTriangles)
        {
            Debug.LogError("number of triangles to remove is bigger than the amount of triangles in the file");
            return;
        }


        int nbRemovedVertices = 0;
        int j = 0, k = 0;
        for (; j < nbTriToRemove; j++, k += 3)
        {

            for (int i = 0; i < vertices.Count; i++)
            {
                if (triangles[k] == i && !float.IsNegativeInfinity(vertices[i].x))
                { vertices[i] = Vector3.negativeInfinity; nbRemovedVertices++; }

                if (triangles[k + 1] == i && !float.IsNegativeInfinity(vertices[i].x))
                { vertices[i] = Vector3.negativeInfinity; nbRemovedVertices++; }

                if (triangles[k + 2] == i && !float.IsNegativeInfinity(vertices[i].x))
                { vertices[i] = Vector3.negativeInfinity; nbRemovedVertices++; }

            }

            triangles[k] = -1;
            triangles[k + 1] = -1;
            triangles[k + 2] = -1;
        }



        for (; j < nbTriangles; j++, k += 3)
        {

            for (int i = 0; i < vertices.Count; i++)
            {
                if (triangles[k] == i && float.IsNegativeInfinity(vertices[i].x))
                {
                    triangles[k] = -1;
                    triangles[k + 1] = -1;
                    triangles[k + 2] = -1;

                    nbTriToRemove++;
                    break;
                }

                if (triangles[k + 1] == i && float.IsNegativeInfinity(vertices[i].x))
                {
                    triangles[k] = -1;
                    triangles[k + 1] = -1;
                    triangles[k + 2] = -1;

                    nbTriToRemove++;
                    break;
                }

                if (triangles[k + 2] == i && float.IsNegativeInfinity(vertices[i].x))
                {
                    triangles[k] = -1;
                    triangles[k + 1] = -1;
                    triangles[k + 2] = -1;

                    nbTriToRemove++;
                    break;
                }

            }

        }




        if (nbVertices - nbRemovedVertices == 0)
        {

            triangles.Clear();

            nbVertices = 0;
            nbTriangles = 0;
        }
        else
        {
            nbVertices -= nbRemovedVertices;
            nbTriangles -= nbTriToRemove;
        }

        string[] newFile = new string[2 + nbVertices + nbTriangles];
        newFile[0] = "OFF";
        newFile[1] = nbVertices + " " + nbTriangles + " 0";


        int nbRemoved = 0;
        int verticeIndex = 0;
        int lineIndex = 2;


        foreach (Vector3 v in vertices)
        {
            if (float.IsNegativeInfinity(v.x))
            {
                nbRemoved++;
            }
            else
            {
                newFile[lineIndex] = v.x.ToString(CultureInfo.InvariantCulture) + " " + v.y.ToString(CultureInfo.InvariantCulture) + " " + v.z.ToString(CultureInfo.InvariantCulture);
                lineIndex++;

                for (int y = 0; y < triangles.Count; y++)
                {
                    if (triangles[y] == verticeIndex)
                    {
                        triangles[y] -= nbRemoved;
                    }
                }
            }

            verticeIndex++;
        }


        for (int y = 0; y < triangles.Count; y += 3)
        {
            if (triangles[y] != -1)
            {
                newFile[lineIndex] = "3 " + triangles[y] + " " + triangles[y + 1] + " " + triangles[y + 2];
                lineIndex++;
            }
        }


        string newPath = "Assets\\Export.off";
        File.WriteAllLines(newPath, newFile);


    }

    private void createGrid()
    {
        float step = 0.05f;
        for (float z = minZ; z < maxZ + step; z += step) {
            for (float y = minY; y < maxY + step; y += step)
            {
                for (float x = minX; x < maxX + step; x+= step)
                {
                    grid.Add(new Vector3(x, y, z));
                    //Debug.Log("new Vector3(x, y, z) = " + new Vector3(x, y, z));

                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        foreach (Vector3 v in grid) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(v, 0.01f);   
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(maxX,maxY,maxZ), 0.01f);   

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(minX, minY, minZ), 0.01f);   
    }



}
