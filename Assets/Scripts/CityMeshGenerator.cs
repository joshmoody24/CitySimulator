using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityMeshGenerator : MonoBehaviour
{

    public Material urbanMaterial;
    public Material residentialMaterial;
    public Material ruralMaterial;
    public Material roadMaterial;

    public Transform cityParent;
    public GameObject quadrantPrefab;

    public CityMeshOptions meshOptions;


    public void GenerateCityMesh(List<Quadrant> quads, Vector2 cityCenter, CityOptions options){
        // clear children if any
        foreach(Transform child in cityParent){
            Destroy(child.gameObject);
        }

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
            // these vertices look something like this:
            /*
            .      .
              .  . 
                  
              .  . 
            .      .
            */
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

            float width = Mathf.Abs(q.end.x - q.start.x)-options.roadWidth;
            float height = Mathf.Abs(q.end.y - q.start.y)-options.roadWidth;
            GameObject quadObj = Instantiate(quadrantPrefab, cityParent.transform.position + new Vector3(q.start.x, 0, q.start.y), Quaternion.identity, cityParent);
            Mesh mesh = new Mesh();
            MeshFilter filter = quadObj.GetComponent<MeshFilter>();

            filter.mesh = mesh;

            mesh.Clear();
            
            mesh.vertices = new Vector3[]{
                transform.position + new Vector3(rw, 0, rw),
                transform.position + new Vector3(rw, 0, height),
                transform.position + new Vector3(width, 0, rw),
                transform.position + new Vector3(width, 0, height),
            };

            mesh.uv = new Vector2[] {new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1,1)};

            mesh.triangles = new int[] {0,1,2,3,2,1};

            // randomly assign color
            // Material randomMaterial = debugMaterials[Random.Range(0, debugMaterials.Count)];

            // assign color based on type
            Material mat;
            QuadrantType type = Quadrant.GetQuadrantType(q.start, q.end, cityCenter, options);
            if(type == QuadrantType.Urban){
                mat = urbanMaterial;
            }
            else if(type == QuadrantType.Residential){
                mat = residentialMaterial;
            }
            else{
                mat = ruralMaterial;
            }
            quadObj.GetComponent<MeshRenderer>().material = mat;
        }

        // create the road object
        GameObject road = new GameObject("Road");        
        road.AddComponent<MeshFilter>();
        var mr = road.AddComponent<MeshRenderer>();
        road.AddComponent<MeshCollider>();
        MeshFilter mf = road.GetComponent<MeshFilter>();
        Mesh rmesh = new Mesh();
        mf.mesh = rmesh;
            rmesh.vertices = new Vector3[]{
                transform.position + new Vector3(0, 0, 0),
                transform.position + new Vector3(0, 0, 5),
                transform.position + new Vector3(5, 0, 0),
                transform.position + new Vector3(5, 0, 5),
            };
        rmesh.triangles = new int[] {0,1,2,3,2,1};
        rmesh.vertices = roadVertices.ToArray();
        rmesh.triangles = roadTris.ToArray();
        rmesh.uv = roadUVs.ToArray();
        mr.material = roadMaterial;
        // hack because too lazy to manually deal with normals
        road.transform.localScale = new Vector3(1,-1,1);
    }
}

public class CityMeshOptions {

}