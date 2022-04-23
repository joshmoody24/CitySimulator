using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{

    public int maxIterations = 100;
    private int iterations;

    public int minBlockHeight = 16;
    public int maxBlockHeight = 20;
    public int blockWidth = 4;
    public int minBlockRepetitions = 3;
    public int maxBlockRepetitions = 6;

    public int roadWidth = 2;

    public GameObject wall;
    public GameObject road;
    public GameObject house;
    public GameObject yard;

    // look up what's in a given location
    // 0 = road
    // 1 = house
    Dictionary<(int, int, int), GameObject> data; 

    // Start is called before the first frame update
    void Start()
    {
        iterations = 0;
        data = new Dictionary<(int, int, int), GameObject>();
        IterateCityGeneration(0,0);
    }

    // Update is called once per frame
    void Update()
    {
    }

    // generate
    void IterateCityGeneration(int xPos, int yPos){
        GenerateCityCell(xPos, yPos, 1, 1);
        GenerateCityCell(xPos, yPos, 1, -1);
        GenerateCityCell(xPos, yPos, -1, 1);
        GenerateCityCell(xPos, yPos, -1, -1);
    }

    // xDir and yDir should be either 1 or -1
    void GenerateCityCell(int xPos, int yPos, int xDir = 1, int yDir = 1, int zPos = 0, int depth = 0){
        Debug.Log(xDir + ", " + yDir + "@" + xPos + ", " + yPos);
        if(depth > maxIterations) return;

        // randomly determine orientation (whether blocks are vertical or horizontal)
        bool vertical = true;
        float random = Random.Range(0f,1f);
        if(random < .5f){
            vertical = false;
        }

        // determine road length
        int blockHeight = Random.Range(minBlockHeight, maxBlockHeight);
        // decoupled in case we want to add range later
        int blockWidth = this.blockWidth;

        // switch block height and width if horiziontal
        if(!vertical){
            int temp = blockHeight;
            blockHeight = blockWidth;
            blockWidth = temp;
        }

        // number of times the little skinny block replicates itself
        int blockRepetitions = Random.Range(minBlockRepetitions, maxBlockRepetitions);

        // hit that generation yo
        for (int blockRep = 0; blockRep < blockRepetitions; blockRep++){

            int blockX;
            int blockY;

            if(vertical){
                blockX = xPos + blockRep*(blockWidth+roadWidth)*xDir + xDir*(roadWidth/2);
                blockY = yPos + yDir*(roadWidth/2);
            } else {
                blockX = xPos + xDir*(roadWidth/2);
                blockY = yPos + blockRep*(blockHeight+roadWidth)*yDir + yDir*(roadWidth/2);
            }

            if(xDir < 0) blockX -= 1;
            if(yDir < 0) blockY -= 1;

            GenerateMiniBlock(blockX, blockY, blockWidth, blockHeight, xDir, yDir);
        }

        // generate 3 more city cells at its corners (until max depth is reached)
        if(depth < maxIterations){
                int newCell1XOffset = (blockWidth+roadWidth)*xDir;
                int newCell1YOffset = (blockHeight+roadWidth)*yDir;
                if(vertical) newCell1XOffset *= blockRepetitions;
                else newCell1YOffset *= blockRepetitions;
                GenerateCityCell(xPos + newCell1XOffset, yPos + newCell1YOffset, xDir, yDir, zPos, depth+1);
                GenerateCityCell(xPos, yPos + newCell1YOffset, xDir, yDir, zPos, depth+1);
                GenerateCityCell(xPos + newCell1XOffset, yPos, xDir, yDir, zPos, depth+1);
        }
    }

    void GenerateMiniBlock(int xPos, int yPos, int blockWidth, int blockHeight, int xDir = 1, int yDir = 1, int zPos = 0){
        // generate the block interior
        for(int x = -roadWidth; x < blockWidth+roadWidth; x++){
            for(int y = -roadWidth; y < blockHeight+roadWidth; y++){
                int spawnPosX = xPos + x*xDir;
                int spawnPosY = yPos + y*yDir;

                // road or house?
                if(x < 0 || x >= blockWidth || y < 0 || y >= blockHeight){
                    RegisterAndSpawnBlock(spawnPosX, spawnPosY, zPos, road);
                }
                else {
                    RegisterAndSpawnBlock(spawnPosX, spawnPosY, zPos, house);
                }
            }
        }
    }

    GameObject RegisterAndSpawnBlock(int x, int y, int z, GameObject block){
        if(data.ContainsKey((x,y,z))) return null;
        // register the spawn with the dictionary
        data[(x, y, z)] = block;
        Vector3 blockPos = new Vector3(x,y,z);
        return Instantiate(block, blockPos, Quaternion.identity);
    }
}
