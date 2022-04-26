using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City
{
    public static List<Quadrant> GenerateCity(Vector2 bottomLeft, Vector2 topRight, CityOptions options){

        Vector2 center = (bottomLeft + topRight)/2f;

        // utilities
        Vector2 middleLeft = new Vector2(bottomLeft.x, center.y);
        Vector2 middleRight = new Vector2(topRight.x, center.y);
        Vector2 topCenter = new Vector2(center.x, topRight.y);
        Vector2 bottomRight = new Vector2(topRight.x, bottomLeft.y);
        Vector2 bottomCenter = new Vector2(center.x, bottomLeft.y);

        List<Quadrant> quadrants = new List<Quadrant>();

        // northeast
        quadrants.AddRange(Quadrant.FillQuadrant(center, topRight, center, options));
        // northwest
        quadrants.AddRange(Quadrant.FillQuadrant(middleLeft, topCenter, center, options));
        // southeast
        quadrants.AddRange(Quadrant.FillQuadrant(bottomCenter, middleRight, center, options));
        // southwest
        quadrants.AddRange(Quadrant.FillQuadrant(bottomLeft, center, center, options));

        return quadrants;
    }
}

[System.Serializable]
public class CityOptions
{
    public int seed = 0;
    public float cityRadius= 1000f;
    public float roadWidth = 6f;
    public float yardArea = 1000f;
    public float houseArea = 200f;
    public float biomeScale = 1f;
    public float minUrbanArea = 150f;
    public float minResidentialArea = 200f;
    public float minRuralArea = 2000f;
    public float maxAspectRatio = 4/1f;
    [Range(1, 50)]
    public int biomeSamples = 5;
    [Range(0f,1f)]
    public float urbanLowerThreshold = .8f;
    [Range(0f, 1f)]
    public float ruralUpperThreshold = .2f;
    public float mainStFalloff = 20f;
    [Range(0f, 1f)]
    public float mainStImpact = 0.7f;
    public float centerStFalloff = 100f;
    [Range(0f, 1f)]
    public float centerStImpact = 0.4f;
    // residential is anything in between minUrban and maxRural
}