using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator3 : MonoBehaviour
{

    public float minQuadrantArea;
    public float height;
    public float width;

    public CityOptions options;

    public GameObject quadrantPrefab;

    public List<Quadrant> cityQuadrants;

    public List<Material> debugMaterials;

    // Start is called before the first frame update
    void Start()
    {
        // convert height and width into concrete points
        Vector2 end = new Vector2(width/2f, height/2f);
        Vector2 start = -end;
        cityQuadrants = City.GenerateCity(start, end, options);

        DrawQuadrants(cityQuadrants);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawQuadrants(List<Quadrant> quads){
        foreach(Quadrant q in quads){
            float width = Mathf.Abs(q.end.x - q.start.x);
            float height = Mathf.Abs(q.end.y - q.start.y);
            GameObject quadObj = Instantiate(quadrantPrefab, new Vector3(q.start.x, 0, q.start.y), Quaternion.identity);
            Mesh mesh = new Mesh();
            MeshFilter filter = quadObj.GetComponent<MeshFilter>();

            filter.mesh = mesh;

            mesh.Clear();
            
            mesh.vertices = new Vector3[]{
                transform.position,
                transform.position + new Vector3(0, 0, height),
                transform.position + new Vector3(width, 0, 0),
                transform.position + new Vector3(width, 0, height),
            };

            mesh.uv = new Vector2[] {new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1,1)};

            mesh.triangles = new int[] {0,1,2,3,2,1};

            // randomly assign color
            Material randomMaterial = debugMaterials[Random.Range(0, debugMaterials.Count)];
            quadObj.GetComponent<MeshRenderer>().material = randomMaterial;
        }
    }
}
