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

    public static QuadrantType GetQuadrantType(Vector2 start, Vector2 end, Vector2 cityCenter, CityOptions options){
        // sample the middle point (todo: increase accuracy through averaging)
        float sample = GetAverageBiome(start, end, cityCenter, options);
        if(sample > options.urbanLowerThreshold) return QuadrantType.Urban;
        else if(sample < options.ruralUpperThreshold) return QuadrantType.Rural;
        else return QuadrantType.Residential;
    }

    public static float GetAverageBiome(Vector2 start, Vector2 end, Vector2 cityCenter, CityOptions options){
        float avg = 0;
        float incrementX = Mathf.Abs(start.x-end.x)/options.biomeSamples;
        float incrementY = Mathf.Abs(start.y- end.y)/options.biomeSamples;
        for(int i = 0; i < options.biomeSamples; i++){
            for(int j = 0; j < options.biomeSamples; j++){
                float sample = SampleBiomeAtPoint(new Vector2(start.x + incrementX*i + incrementX/2, start.y + incrementY*j + incrementY/2), cityCenter, options);
                avg += sample;
            }
        }
        return avg / (options.biomeSamples*options.biomeSamples);
    }

    public static float SampleBiomeAtPoint(Vector2 point, Vector2 cityCenter, CityOptions options){
        float scale = 1f/options.biomeScale;
        float baseSample = Mathf.PerlinNoise(point.x*scale+options.seed, point.y*scale+options.seed);

        // make it more likely for main street to have urban
        float distanceFromMainSt = Mathf.Abs(cityCenter.x - point.x);
        // convert to 0-1 range
        float normDistanceFromMainSt = (options.mainStFalloff - distanceFromMainSt)/options.mainStFalloff;
        float mainStImpact = Mathf.Clamp(normDistanceFromMainSt, 0f, 1f);

        // make it more likely for center street to have residential
        float distanceFromCenterSt = Mathf.Abs(cityCenter.y - point.y);
        // convert to 0-1 range
        float normDistanceFromCenterSt = (options.centerStFalloff - distanceFromCenterSt)/options.centerStFalloff;
        float centerStImpact = Mathf.Clamp(normDistanceFromCenterSt, 0f, 1f);

        float sample = baseSample + (mainStImpact*options.mainStImpact) + (centerStImpact*options.centerStImpact);

        float dist = Vector2.Distance(point, cityCenter);
        float falloff = options.cityRadius/(dist+options.cityRadius);
        float mask = dist > options.cityRadius ? 0f : 1f;
        return sample * falloff;// * mask;
    }

    // chop up the quadrant into land plots recursively
    public static List<Quadrant> FillQuadrant(Vector2 start, Vector2 end, Vector2 cityCenter, CityOptions options){
        // treat the entire quadrant as a single land plot to start with
        return SubdivideQuadrant(start, end, cityCenter, options);
    }

    public static List<Quadrant> SubdivideQuadrant(Vector2 start, Vector2 end, Vector2 cityCenter, CityOptions options){
        
        // utilities
        float width = Mathf.Abs(end.x - start.x);
        float height = Mathf.Abs(end.y - start.y);
        float area = width * height;
        float aspect = width / height;

        // base cases
        if(start.x > end.x || start.y > end.y){
            return new List<Quadrant>();
        }

        // stop subdiving if the quadrant is small enough for its biome type
        switch(GetQuadrantType(start, end, cityCenter, options)){
            case QuadrantType.Urban:
                if(area <= options.minUrbanArea)
                    return new List<Quadrant>(){new Quadrant(start,end)};
                break;
            case QuadrantType.Residential:
                if(area <= options.minResidentialArea)
                    return ResidentialBlocks(new Quadrant(start,end), options);
                break;
            case QuadrantType.Rural:
                if(area <= options.minRuralArea)
                    return new List<Quadrant>(){new Quadrant(start,end)};
                break;
        }

        // divide the quadrant
        List<Quadrant> subQuads = new List<Quadrant>();
        List<Quadrant> subDiv1 = new List<Quadrant>();
        List<Quadrant> subDiv2 = new List<Quadrant>();
        Vector2 sub1End;
        Vector2 sub2Start;

         // randomly choose a slice that preserves the max aspect ratio
         // size of road compared to this quadrant (0-1 range)
        float roadsize = width > height ? options.roadWidth/width : options.roadWidth/height;
        roadsize *= 2;
        float minSlice = 1/options.maxAspectRatio+roadsize;
        float maxSlice = (options.maxAspectRatio-1)/options.maxAspectRatio-roadsize;

        if(minSlice > maxSlice){
            return new List<Quadrant>(){new Quadrant(start,end)};
        }

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

        subDiv1.AddRange(SubdivideQuadrant(start, sub1End, cityCenter, options));
        subDiv2.AddRange(SubdivideQuadrant(sub2Start, end, cityCenter, options)); 

        subQuads.AddRange(subDiv1);
        subQuads.AddRange(subDiv2);
        return subQuads;
    }

    public static List<Quadrant> ResidentialBlocks(Quadrant residentialQuad, CityOptions options){
        float yardWith = Mathf.Sqrt(options.yardArea);
        float width = residentialQuad.GetWidth();
        float height = residentialQuad.GetHeight();
        List<Quadrant> subBlocks = new List<Quadrant>();
        if(width > height){
            // create blocks that allow square yards approximately {yardWidth} wide
            int numBlocks = GetOptimalBlockCount(residentialQuad, options);
            float blockWidth = width/numBlocks;
            for(int i = 0; i < numBlocks; i++){
                Vector2 s = new Vector2(residentialQuad.start.x + (blockWidth*i), residentialQuad.start.y);
                Vector2 e = new Vector2(residentialQuad.start.x + (blockWidth*i) + blockWidth, residentialQuad.end.y);
                subBlocks.Add(new Quadrant(s,e));
            }
        }
        else {
            int numBlocks = GetOptimalBlockCount(residentialQuad, options);
            float blockHeight = height/numBlocks;
            for(int i = 0; i < numBlocks; i++){
                Vector2 s = new Vector2(residentialQuad.start.x, residentialQuad.start.y + (blockHeight*i));
                Vector2 e = new Vector2(residentialQuad.end.x, residentialQuad.start.y + (blockHeight*i) + blockHeight);
                subBlocks.Add(new Quadrant(s,e));
            }
        }
        return subBlocks;
    }

    // attempt to get the number of blocks required to hold 2 yards
    public static int GetOptimalBlockCount(Quadrant q, CityOptions options){
        float err = float.PositiveInfinity;
        float? prevError = float.PositiveInfinity;
        float quadLong = q.GetHeight() > q.GetWidth() ? q.GetHeight() : q.GetWidth();
        float quadShort = q.GetHeight() < q.GetWidth() ? q.GetHeight() : q.GetWidth();
        int maxIterations = 20;
        for(int i = 1; i <= maxIterations; i++){
            float blockLength = (quadLong / i) - (options.roadWidth*(i-1))/(i*2);
            float avgYardWidth = blockLength/2f;
            prevError = err;
            err = Mathf.Abs(Mathf.Sqrt(options.yardArea) - avgYardWidth);
            if(err > prevError){
                return i-1;
            }
        }
        return -1;
    }

    public float GetWidth(){
        return Mathf.Abs(end.x - start.x);
    }
    public float GetHeight(){
        return Mathf.Abs(end.y - start.y);
    }
}

public enum QuadrantType {Urban, Residential, Rural};