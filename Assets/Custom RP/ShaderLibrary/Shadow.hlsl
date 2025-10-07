#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#define SHADOW_SAMPLER sampler_linear_clamp_compare

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
SAMPLER_CMP(SHADOW_SAMPLER);

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

CBUFFER_START (_CustomShadows)
    int _CascadeCount;
    float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
    float4 _ShadowDistanceFade;
CBUFFER_END

struct DirectionalShadowData {
    float strength;
    int tileIndex;
};

struct ShadowData {
    int cascadeInex;
    float strength;
};

float SampleDirectionalShadowAtlas(float3 positionSTS) {
    return SAMPLE_TEXTURE2D_SHADOW(
        _DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS
    );
}

float GetDirectionalShadowAttenuation(DirectionalShadowData data, Surface surfaceWS) {
    if (data.strength <= 0.0) return 1.0;
    float3 positionSTS = mul(_DirectionalShadowMatrices[data.tileIndex], float4(surfaceWS.position, 1.0)).xyz;
    float shadow = SampleDirectionalShadowAtlas(positionSTS);
    return lerp(1.0, shadow, data.strength);
}

float FadeShadowStrength(float distance, float scale, float fade) {
    return saturate((1.0 - distance * scale) * fade);
}

ShadowData GetShadowData(Surface surfaceWS) {
    ShadowData data;
    //由于unity阴影的裁剪球比实际要计算的稍大，所以在相机拉到比较远的地方的时候会突然消失（临界点就是超出最大距离不该投射阴影的时候）
    //这里直接用裁剪球来做距离裁剪
    //data.strength = surfaceWS.depth < _ShadowDistance ? 1.0 : 0.0;
    data.strength = FadeShadowStrength(surfaceWS.depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y);
    int i;
    for (i = 0; i<_CascadeCount; ++i) {
        float4 sphere = _CascadeCullingSpheres[i];
        float distanceSqr = DistancesSquared(surfaceWS.position, sphere.xyz);
        if (distanceSqr < sphere.w) {
            if (i == _CascadeCount - 1) {
                //级联渐变
                data.strength *= FadeShadowStrength(distanceSqr, 1.0/sphere.w, _ShadowDistanceFade.z);
            }
            break;
        }
    }

    if (i == _CascadeCount) {
        data.strength = 0.0;
    }
    
    data.cascadeInex = i;
    return data;
}

#endif

