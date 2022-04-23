using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quadrant
{
    // defines a square shape
    public Vector2 start;
    public Vector2 end;

    public Quadrant(Vector2 s, Vector2 e){
        start = s;
        end = e;
    }

    // chop up the quadrant into land plots recursively
    public static List<Quadrant> FillQuadrant(Vector2 start, Vector2 end, CityOptions options){
        // treat the entire quadrant as a single land plot to start with
        return SubdivideQuadrant(start, end, options);
    }

    public static List<Quadrant> SubdivideQuadrant(Vector2 start, Vector2 end, CityOptions options){
        
        // utilities
        float width = Mathf.Abs(end.x - start.x);
        float height = Mathf.Abs(end.y - start.y);
        float area = width * height;
        float aspect = width / height;
        Vector2 center = (start+end)/2f;

        // base cases
        if(start.x > end.x || start.y > end.y){
            return new List<Quadrant>();
        }

        if(area <= options.minQuadrantArea){
            List<Quadrant> quadrant = new List<Quadrant>(){new Quadrant(start,end)};
            return quadrant;
        }

        // divide the quadrant
        List<Quadrant> subQuads = new List<Quadrant>();
        List<Quadrant> subDiv1 = new List<Quadrant>();
        List<Quadrant> subDiv2 = new List<Quadrant>();
        Vector2 sub1End;
        Vector2 sub2Start;

         // randomly choose a slice that preserves the max aspect ratio
        float minSlice = 1/options.maxAspectRatio;
        float maxSlice = (options.maxAspectRatio-1)/options.maxAspectRatio;
        float slice = Random.Range(minSlice, maxSlice);

        // always subdivide to make them more square shaped
        if(width > height){

            sub1End = new Vector2(slice*width+start.x, end.y);
            sub2Start = new Vector2(slice*width+start.x, start.y);       
        }
        else{
            
            sub1End = new Vector2(end.x, slice*height+start.y);
            sub2Start = new Vector2(start.x, slice*height+start.y);
        }

        subDiv1.AddRange(SubdivideQuadrant(start, sub1End, options));
        subDiv2.AddRange(SubdivideQuadrant(sub2Start, end, options)); 

        subQuads.AddRange(subDiv1);
        subQuads.AddRange(subDiv2);
        return subQuads;
    }
}