using UnityEngine;
using UnityEngine.Rendering;

public class Shadows {
    const string bufferName = "Shadows";

    private CommandBuffer _buffer = new CommandBuffer {
        name = bufferName
    };

    private ScriptableRenderContext _context;
    private CullingResults _cullingResults;
    private ShadowSettings _settings;
    private const int MaxShadowedDirectionalLightCount = 4;
    private const int MaxCascades = 4;
    private int _shadowedDirectionalLightCount;
    
    private struct ShadowedDirectionalLight {
        public int visibleLightIndex;
    }

    private ShadowedDirectionalLight[] _shadowedDirectionalLights = new ShadowedDirectionalLight[MaxShadowedDirectionalLightCount];
    private static int _dirShadowAtlasID = Shader.PropertyToID("_DirectionalShadowAtlas");
    private static int _dirShadowMatricesID = Shader.PropertyToID("_DirectionalShadowMatrices");
    private static int _cascadeCountID = Shader.PropertyToID("_CascadeCount");
    private static int _cascadeCullingSpheresID = Shader.PropertyToID("_CascadeCullingSpheres");
    private static int _shadowDistanceFadeID = Shader.PropertyToID("_ShadowDistanceFade");
    
    private static Vector4[] _cascadeCullingSpheres = new Vector4[MaxCascades];
    private static Matrix4x4[] _dirShadowMatrices = new Matrix4x4[MaxShadowedDirectionalLightCount * MaxCascades];
    
    
    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings) {
        _context = context;
        _cullingResults = cullingResults;
        _settings = settings;
        _shadowedDirectionalLightCount = 0;
    }

    public Vector2 ReserveDirectionalShadows(Light light, int visibleLightIndex) {
        if (_shadowedDirectionalLightCount < MaxShadowedDirectionalLightCount 
            && light.shadows != LightShadows.None 
            && light.shadowStrength > 0f 
            && _cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b)) {
            _shadowedDirectionalLights[_shadowedDirectionalLightCount] = new ShadowedDirectionalLight {
                visibleLightIndex = visibleLightIndex,
            };
            Vector2 res = new Vector2(
                light.shadowStrength, 
                _settings.directional.cascadeCount * _shadowedDirectionalLightCount
            );
            ++_shadowedDirectionalLightCount;
            return res;
        }

        return Vector2.zero;
    }

    public void Render() {
        if (_shadowedDirectionalLightCount > 0) {
            RenderDirectionalShadows();    
        }
        else {
            _buffer.GetTemporaryRT(
                _dirShadowAtlasID, 1, 1,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
            );
        }
    }

    private void RenderDirectionalShadows() {
        int atlasSize = (int)_settings.directional.atlasSize;
        _buffer.GetTemporaryRT(_dirShadowAtlasID, atlasSize, atlasSize,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        _buffer.SetRenderTarget(_dirShadowAtlasID,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        _buffer.ClearRenderTarget(true, false, Color.clear);
        
        _buffer.BeginSample(bufferName);
        ExecuteBuffer();

        int tiles = _shadowedDirectionalLightCount * _settings.directional.cascadeCount;
        int split;
        if (tiles <= 1) split = 1;
        else if (tiles <= 4) split = 2;
        else split = 4;
        
        int tileSize = atlasSize / split;
        for (int i = 0; i < _shadowedDirectionalLightCount; ++i) {
            RenderDirectionalShadows(i, split,  tileSize);
        }
        _buffer.SetGlobalInt(_cascadeCountID, _settings.directional.cascadeCount);
        _buffer.SetGlobalVectorArray(_cascadeCullingSpheresID, _cascadeCullingSpheres);
        _buffer.SetGlobalMatrixArray(_dirShadowMatricesID, _dirShadowMatrices);
        float f = 1f - _settings.directional.cascadeRatio;
        _buffer.SetGlobalVector(_shadowDistanceFadeID, new Vector4(1f / _settings.maxDistance, 1f / _settings.distanceFade, 1f / (1f - f*f)));
        _buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    private void RenderDirectionalShadows(int index, int split, int tileSize) {
        ShadowedDirectionalLight light = _shadowedDirectionalLights[index];
        var shadowSettings = new ShadowDrawingSettings(_cullingResults, light.visibleLightIndex);

        
        int cascadeCount = _settings.directional.cascadeCount;
        int tileOffset = index * cascadeCount;
        Vector3 ratios = _settings.directional.CascadeRatios;

        for (int i = 0; i < cascadeCount; ++i) {
            _cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                light.visibleLightIndex, i, cascadeCount, ratios, tileSize, 0f,
                out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData 
            );
            shadowSettings.splitData = splitData;

            if (index == 0) {
                Vector4 cullingSphere = splitData.cullingSphere;
                cullingSphere.w *= cullingSphere.w;
                _cascadeCullingSpheres[i] = cullingSphere;
            }
            
            int tileIndex = tileOffset + i;
            _dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(
                projectionMatrix * viewMatrix, 
                SetTileViewport(tileIndex, split, tileSize), 
                split
            );
            _buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            ExecuteBuffer();
            _context.DrawShadows(ref shadowSettings);
        }
    }

    private Vector2 SetTileViewport(int index, int split, float tileSize) {
        Vector2 offset = new Vector2(index % split, index / split);
        _buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
        return offset;
    }

    private Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split) {
        //Why are Z buffers reversed?
        /*
         * It is most intuitive to have zero represent zero depth and one represent maximum depth.
         * That's what OpenGL does. But due to the way precision is limited in the depth buffer and
         * the fact that it is stored nonlinearly we make better use of the bits by reversing that.
         * Other graphics API used the reversed approach.
         * We usually we don't need to worry about it, except when we're explicitly working with clip space.
         */
        if (SystemInfo.usesReversedZBuffer) {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }
        float scale = 1f / split;
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);
        return m;
    }
    
    private void ExecuteBuffer() {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }
    
    public void Cleanup() {
        _buffer.ReleaseTemporaryRT(_dirShadowAtlasID);
        ExecuteBuffer();
    }
}