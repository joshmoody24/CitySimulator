using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator2 : MonoBehaviour
{

    public int mapWidth = 250;
    public int mapHeight = 150;
    public int areaThreshold = 40;
    public int minWidth = 6;

    public int smallBlockWidth = 4;

    public float roadScale = 10f;

    public GameObject road;
    public GameObject yard;
    public GameObject house;

    public Dictionary<(int, int), GameObject> streetData;
    public List<Vector3> houses;

    public Transform mapParent;

    // Start is called before the first frame update
    void Start()
    {
        streetData = new Dictionary<(int, int), GameObject>();
        houses = new List<Vector3>();
        // parent for holding all small objects
        GenerateMap();
        PopulateHouses();
        mapParent.transform.localScale *= roadScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateMap(){
        ChopMap(0,0,mapWidth,mapHeight);
    }

    // subdivide map into 4 smaller rectangles 
    void ChopMap(int x1, int y1, int x2, int y2){

        // base case - fill with box padded with another box (temp)
        int width = (int)Mathf.Abs(x1-x2);
        int height = (int)Mathf.Abs(y1-y2);
        if(width < minWidth || height < minWidth || width*height < areaThreshold){
            FillMapUnit(x1,y1,x2,y2);
            return;
        }

        // recurse

        // chop the map into 4 rectangles
        int xChop = Random.Range(x1+minWidth/2,x2-minWidth/2);
        int yChop = Random.Range(y1+minWidth/2, y2-minWidth/2);

        Debug.Log("XCHOP " + xChop);
        Debug.Log("YCHOP " + yChop);
        ChopMap(x1,y1,xChop,yChop);
        ChopMap(xChop+1,y1,x2,yChop);
        ChopMap(x1,yChop+1,xChop,y2);
        ChopMap(xChop+1,yChop+1,x2,y2);
    }

    void FillMapUnit(int x1, int y1, int x2, int y2){
        // figure out which side is the skinny side
        int width = (int)Mathf.Abs(x1-x2);
        int height = (int)Mathf.Abs(y1-y2);

        // recursively fill in the residential roads
        RecursiveFillResidentialBlock(x1,y1,x2,y2,(width > height));
    }

    void RecursiveFillResidentialBlock(int x1, int y1, int x2, int y2, bool verticalRoads){

        if(x1 > x2) return;
        if(y1 > y2) return;

        int width = (int)Mathf.Abs(x1-x2);
        int height = (int)Mathf.Abs(y1-y2);


        int blockEnd;
        if(verticalRoads){
            // if there's not enough land left over for another recursion, take it all
            int extraSize = 0;
            if(x2-x1 < smallBlockWidth*2){
                extraSize = x2-x1-smallBlockWidth;
            }
            blockEnd = x1+smallBlockWidth + extraSize;
            for(int x = x1; x <= blockEnd; x++){
                for(int y = y1; y <= y1+height; y++){
                    GameObject spawnType;
                    Vector3 spawnPos = new Vector3(x,0,y);
                    if(x == x1 || y == y1){
                        spawnType = road;
                    }
                    else {
                        spawnType = yard;
                        if(extraSize == 0 && (x == x1+1 || x == blockEnd)){
                            houses.Add(new Vector3(x,0,y));
                        }
                    }
                    Instantiate(spawnType, spawnPos, Quaternion.identity, mapParent);
                }
            }
            RecursiveFillResidentialBlock(blockEnd+1, y1, x2, y2, verticalRoads);
            return;
        }
        else {
            // if there's not enough land left over for another recursion, take it all
            int extraSize = 0;
            if(y2-y1 < smallBlockWidth*2){
                extraSize = y2-y1-smallBlockWidth;
            }
            blockEnd = y1+smallBlockWidth + extraSize;
            for(int x = x1; x <= x1+width; x++){
                for(int y = y1; y <= blockEnd; y++){
                    GameObject spawnType;
                    Vector3 spawnPos = new Vector3(x,0,y);
                    if(x == x1 || y == y1){
                        spawnType = road;
                    }
                    else {
                        spawnType = yard;
                        if(extraSize == 0 && (y == y1+1 || y == blockEnd)){
                            houses.Add(new Vector3(x,0,y));
                        }
                    }
                    Instantiate(spawnType, spawnPos, Quaternion.identity, mapParent);
                }
            }
            RecursiveFillResidentialBlock(x1, blockEnd+1, x2, y2, verticalRoads);
            return;
        }

    }

    void PopulateHouses(){
        foreach(Vector3 pos in houses){
            Instantiate(house, pos + new Vector3(0,1,0), Quaternion.identity, mapParent);
        }
    }

    void AddHouseIfPossible(){

    }
}
