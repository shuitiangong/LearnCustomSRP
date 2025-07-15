#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#define SHADOW_SAMPLER sampler_linear_clamp_compare

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
SAMPLER_CMP(SHADOW_SAMPLER);

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START (_CustomShadows)
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct DirectionalShadowData {
    float strength;
    int tileIndex;
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
    return shadow;
}

#endif

