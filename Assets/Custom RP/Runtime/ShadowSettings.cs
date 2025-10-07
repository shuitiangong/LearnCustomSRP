using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShadowSettings {
    public enum MapSize {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192
    }

    [Serializable]
    public struct Directional {
        public MapSize atlasSize;
        
        [Range(1, 4)]
        public int cascadeCount;
        [Range(0.001f, 1f)]
        public float cascadeRatio;
        [Range(0f, 1f)]
        public float cascadeRatio1, cascadeRatio2, cascadeRatio3;
        public Vector3 CascadeRatios => new Vector3(cascadeRatio1, cascadeRatio2, cascadeRatio3);
    }

    public Directional directional = new Directional {
        atlasSize = MapSize._1024,
        cascadeCount = 4,
        cascadeRatio1 = 0.1f,
        cascadeRatio2 = 0.25f,
        cascadeRatio3 = 0.5f,
        cascadeRatio = 0.1f
    };
    
    [Min(0.001f)] 
    public float maxDistance = 100f;
    [Range(0.001f, 1f)]
    public float distanceFade = 0.1f;
    
}


