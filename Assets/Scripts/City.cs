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
        quadrants.AddRange(Quadrant.FillQuadrant(center, topRight, options));
        // northwest
        quadrants.AddRange(Quadrant.FillQuadrant(middleLeft, topCenter, options));
        // southeast
        quadrants.AddRange(Quadrant.FillQuadrant(bottomCenter, middleRight, options));
        // southwest
        quadrants.AddRange(Quadrant.FillQuadrant(bottomLeft, center, options));

        return quadrants;
    }
}

[System.Serializable]
public class CityOptions
{
    public float minQuadrantArea = 1000f;
    public float maxAspectRatio = 4/1f;
}
