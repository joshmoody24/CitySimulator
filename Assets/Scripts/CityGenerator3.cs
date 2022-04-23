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

    public Transform cityParent;

    public List<Quadrant> cityQuadrants;

    public Material urbanMaterial;
    public Material residentialMaterial;
    public Material ruralMaterial;
    public List<Material> debugMaterials;

    private Vector2 cityCenter;


    // Start is called before the first frame update
    void Start()
    {
        // seed the random number generator
        Random.seed = options.seed;

        // convert height and width into concrete points
        Vector2 end = new Vector2(width/2f, height/2f);
        Vector2 start = -end;
        cityCenter = (start + end) / 2f;
        cityQuadrants = City.GenerateCity(start, end, options);
        DrawQuadrants(cityQuadrants);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DrawQuadrants(List<Quadrant> quads){
        // clear children if any
        foreach(Transform child in cityParent){
            Destroy(child.gameObject);
        }

        foreach(Quadrant q in quads){
            float width = Mathf.Abs(q.end.x - q.start.x)-options.roadWidth;
            float height = Mathf.Abs(q.end.y - q.start.y)-options.roadWidth;
            GameObject quadObj = Instantiate(quadrantPrefab, cityParent.transform.position + new Vector3(q.start.x, 0, q.start.y), Quaternion.identity, cityParent);
            Mesh mesh = new Mesh();
            MeshFilter filter = quadObj.GetComponent<MeshFilter>();

            filter.mesh = mesh;

            mesh.Clear();

            float halfRoad = options.roadWidth/2f;
            
            mesh.vertices = new Vector3[]{
                transform.position + new Vector3(halfRoad, 0, halfRoad),
                transform.position + new Vector3(halfRoad, 0, height - halfRoad),
                transform.position + new Vector3(width - halfRoad, 0, halfRoad),
                transform.position + new Vector3(width - halfRoad, 0, height - halfRoad),
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
    }
}
