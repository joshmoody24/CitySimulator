using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CityMeshGenerator : MonoBehaviour
{

    public Material urbanMaterial;
    public Material residentialMaterial;
    public Material ruralMaterial;
    public Material roadMaterial;

    public Transform cityParent;
    public GameObject quadrantPrefab;

    public CityMeshOptions meshOptions;

    public GameObject housePrefab;


    public void GenerateCityMesh(List<Quadrant> quads, Vector2 cityCenter, CityOptions options){
        // clear children if any
        foreach(Transform child in cityParent){
            Destroy(child.gameObject);
        }

        for(int i = 0; i < quads.Count; i++){
            var smol = TrimQuadrant(quads[i], options.roadWidth);
            Material mat = GetQuadrantMaterial(smol, cityCenter, options);
            Mesh mesh = MeshFromQuadrant(smol, mat);
            Vector3 spawnPos = new Vector3(smol.start.x, 0, smol.start.y);
            DrawMesh("Quadrant", spawnPos, mesh, mat, cityParent);
        }

        Mesh rmesh = GenerateRoadMesh(quads, options);
        GameObject road = DrawMesh("Road", cityCenter, rmesh, roadMaterial, cityParent);
        // hack because too lazy to manually deal with normals
        road.transform.localScale = new Vector3(1,-1,1);
    }

    public Mesh GenerateRoadMesh(List<Quadrant> quads, CityOptions options){
        // the road mesh is one big connected mesh
        List<Vector3> roadVertices = new List<Vector3>();
        List<int> roadTris = new List<int>();
        List<Vector2> roadUVs = new List<Vector2>();

        foreach(Quadrant q in quads){
            // not to be confused with width and height later (this one includes road)
            float qWidth = q.end.x - q.start.x;
            float qHeight = q.end.y - q.start.y;
            float aspect = qWidth/qHeight;
            // add the vertices to the road list (which will be created later)

            var bl = new Vector3(q.start.x, 0, q.start.y);
            var br = new Vector3(q.start.x, 0, q.end.y);
            var tl = new Vector3(q.end.x, 0, q.start.y);
            var tr = new Vector3(q.end.x, 0, q.end.y);
            float rw = options.roadWidth;

            int prevIndex = roadVertices.Count-1;
            roadVertices.AddRange(new List<Vector3>(){
                // outer ring
                bl,
                br,
                tl,
                tr,
                // inner ring
                bl + new Vector3(rw, 0, rw),
                br + new Vector3(rw, 0, -rw),
                tl + new Vector3(-rw, 0, rw),
                tr + new Vector3(-rw, 0, -rw),
            });

            // magic number heck
            roadTris.AddRange(new List<int>(){
                prevIndex+1,prevIndex+5,prevIndex+6,
                prevIndex+1,prevIndex+6,prevIndex+2,
                prevIndex+2,prevIndex+6,prevIndex+8,
                prevIndex+2,prevIndex+8,prevIndex+4,
                prevIndex+4,prevIndex+8,prevIndex+7,
                prevIndex+4,prevIndex+7,prevIndex+3,
                prevIndex+3,prevIndex+7,prevIndex+5,
                prevIndex+3,prevIndex+5,prevIndex+1,
            });

            float rwUvH = rw/qHeight;
            float rwUvW = rw/qWidth;
            roadUVs.AddRange(new List<Vector2>(){
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),
                // inner
                new Vector2(rwUvW, rwUvH),
                new Vector2(1-rwUvW, rwUvH),
                new Vector2(rwUvW, 1-rwUvH),
                new Vector2(1-rwUvW, 1-rwUvH),
            });
        }
        Mesh rmesh = new Mesh();
        rmesh.vertices = roadVertices.ToArray();
        rmesh.triangles = roadTris.ToArray();
        rmesh.uv = roadUVs.ToArray();
        rmesh.RecalculateNormals();
        return rmesh;
    }

    // returns a slightly smaller quadrant and the trimmed portion as a mesh
    public Quadrant TrimQuadrant(Quadrant quad, float trimDistance){
        Vector2 offset = new Vector2(trimDistance, trimDistance);
        Quadrant trimmed = new Quadrant(quad.start+offset, quad.end-offset);
        return trimmed;
    }

    public static GameObject DrawMesh(string name, Vector3 position, Mesh mesh, Material m, Transform parent = null){
            GameObject obj = new GameObject(name);
            obj.transform.parent = parent;
            obj.transform.position = position;
            MeshFilter filter = obj.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.AddComponent<MeshRenderer>();
            MeshCollider mc = obj.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
            mr.material = m;
            filter.mesh = mesh;
            return obj;
    }

    public Mesh MeshFromQuadrant(Quadrant q, Material mat){
            float width = Mathf.Abs(q.end.x - q.start.x);
            float height = Mathf.Abs(q.end.y - q.start.y);
            Mesh mesh = new Mesh();

            mesh.vertices = new Vector3[]{
                transform.position + new Vector3(0, 0, 0),
                transform.position + new Vector3(0, 0, height),
                transform.position + new Vector3(width, 0, 0),
                transform.position + new Vector3(width, 0, height),
            };

            mesh.uv = new Vector2[] {new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1,1)};

            mesh.triangles = new int[] {0,1,2,3,2,1};
            mesh.RecalculateNormals();
            return mesh;
    }

    public Material GetQuadrantMaterial(Quadrant q, Vector2 cityCenter, CityOptions options){
        // assign color based on type
        QuadrantType type = Quadrant.GetQuadrantType(q.start, q.end, cityCenter, options);
        if(type == QuadrantType.Urban){
            return urbanMaterial;
        }
        else if(type == QuadrantType.Residential){
            return residentialMaterial;
        }
        else{
            return ruralMaterial;
        }
    }

    public void SpawnHouses(List<Quadrant> quads, Vector2 cityCenter, CityOptions options){
        List<Quadrant> residentialQuads = quads.Where(x => {
            float biome = Quadrant.GetAverageBiome(x.start, x.end, cityCenter, options);
            return biome < options.urbanLowerThreshold && biome > options.ruralUpperThreshold;
        }).ToList<Quadrant>();
        foreach(Quadrant quad in residentialQuads){
            Quadrant q = TrimQuadrant(quad, options.roadWidth);
            foreach(Quadrant yard in ChopBlockIntoYards(q, options)){
                Instantiate(housePrefab, new Vector3(yard.start.x + yard.GetWidth()/2, 0, yard.start.y + yard.GetHeight()/2), Quaternion.identity, cityParent);
            }
        }
    }

    public List<Quadrant> ChopBlockIntoYards(Quadrant q, CityOptions options){
        List<Quadrant> yards = new List<Quadrant>();
        if(q.GetHeight() > q.GetWidth()){
            int numChops = GetOptimalYardCount(q, options);
            float yardWidth = q.GetWidth() / 2;
            float yardHeight = q.GetHeight()/numChops;
            for(int i = 0; i < numChops; i++){
                yards.Add(new Quadrant(new Vector2(q.start.x, q.start.y + i*yardHeight), new Vector2(q.start.x + yardWidth, q.start.y + i*yardHeight + yardHeight)));
                yards.Add(new Quadrant(new Vector2(q.start.x + yardWidth, q.start.y + i*yardHeight), new Vector2(q.end.x, q.start.y + i*yardHeight + yardHeight)));
            }
        }
        else{
            int numChops = GetOptimalYardCount(q, options);
            float yardWidth = q.GetWidth() / numChops;
            float yardHeight = q.GetHeight() / 2;
            for(int i = 0; i < numChops; i++){
                yards.Add(new Quadrant(new Vector2(q.start.x + i*yardWidth, q.start.y), new Vector2(q.start.x + i*yardWidth + yardWidth, q.start.y + yardHeight)));
                yards.Add(new Quadrant(new Vector2(q.start.x + i*yardWidth, q.start.y+yardHeight), new Vector2(q.start.x + i*yardWidth + yardWidth, q.end.y)));
            }
        }
        return yards;
    }

    
    public static int GetOptimalYardCount(Quadrant q, CityOptions options){
        float err = float.PositiveInfinity;
        float? prevError = float.PositiveInfinity;
        bool heightIsLonger = q.GetHeight() > q.GetWidth();
        float quadLong = heightIsLonger ? q.GetHeight() : q.GetWidth();
        float quadShort = !heightIsLonger ? q.GetHeight() : q.GetWidth();
        int maxIterations = 20;
        for(int i = 1; i <= maxIterations; i++){
            float yardLength = quadLong / i;
            prevError = err;
            err = Mathf.Abs(options.yardArea - yardLength*(quadShort/2));
            if(err > prevError){
                return i;
            }
        }
        return -1;
    }
}

public class CityMeshOptions {

}