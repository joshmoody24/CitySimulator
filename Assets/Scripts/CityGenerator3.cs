using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CityMeshGenerator))]
public class CityGenerator3 : MonoBehaviour
{

    public float minQuadrantArea;
    public float height;
    public float width;

    public CityOptions options;

    public List<Quadrant> cityQuadrants;

    private Vector2 cityCenter;

    public float updatesPerSecond = 5f;


    // Start is called before the first frame update
    void Start()
    {
        // seed the random number generator
        Random.seed = options.seed;
        StartCoroutine(DebugUpdate());

    }

    public IEnumerator DebugUpdate(){
        while(true){
            // convert height and width into concrete points
            Random.seed = options.seed;
            Vector2 end = new Vector2(width/2f, height/2f);
            Vector2 start = -end;
            cityCenter = (start + end) / 2f;
            cityQuadrants = City.GenerateCity(start, end, options);
            GetComponent<CityMeshGenerator>().GenerateCityMesh(cityQuadrants, cityCenter, options);
            yield return new WaitForSeconds(1f/updatesPerSecond);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

}
