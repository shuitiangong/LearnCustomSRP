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
    private const int maxShadowedDirectionalLightCount = 4;
    private int ShadowedDirectionalLightCount;
    
    private struct ShadowedDirectionalLight {
        public int visibleLightIndex;
    }

    private ShadowedDirectionalLight[] ShadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];
    private static int dirShadowAtlasID = Shader.PropertyToID("_DirectionalShadowAtlas");
    private static int dirShadowMatricesID = Shader.PropertyToID("_DirectionalShadowMatrices");
    private static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount];
    
    
    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings) {
        _context = context;
        _cullingResults = cullingResults;
        _settings = settings;
        ShadowedDirectionalLightCount = 0;
    }

    public Vector2 ReserveDirectionalShadows(Light light, int visibleLightIndex) {
        if (ShadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
            light.shadows != LightShadows.None && light.shadowStrength > 0f &&
            _cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b)) {
            ShadowedDirectionalLights[ShadowedDirectionalLightCount] = new ShadowedDirectionalLight {
                visibleLightIndex = visibleLightIndex,
            };
            Vector2 res = new Vector2(light.shadowStrength, ShadowedDirectionalLightCount);
            ++ShadowedDirectionalLightCount;
            return res;
        }

        return Vector2.zero;
    }

    public void Render() {
        if (ShadowedDirectionalLightCount > 0) {
            RenderDirectionalShadows();    
        }
        else {
            _buffer.GetTemporaryRT(
                dirShadowAtlasID, 1, 1,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
            );
        }
    }

    private void RenderDirectionalShadows() {
        int atlasSize = (int)_settings.directional.atlasSize;
        _buffer.GetTemporaryRT(dirShadowAtlasID, atlasSize, atlasSize,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        _buffer.SetRenderTarget(dirShadowAtlasID,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        _buffer.ClearRenderTarget(true, false, Color.clear);
        
        _buffer.BeginSample(bufferName);
        ExecuteBuffer();

        int split = ShadowedDirectionalLightCount <= 1 ? 1 : 2;
        int tileSize = atlasSize / split;
        for (int i = 0; i < ShadowedDirectionalLightCount; ++i) {
            RenderDirectionalShadows(i, split,  tileSize);
        }
        _buffer.SetGlobalMatrixArray(dirShadowMatricesID, dirShadowMatrices);
        _buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    private void RenderDirectionalShadows(int index, int split, int tileSize) {
        ShadowedDirectionalLight light = ShadowedDirectionalLights[index];
        var shadowSettings = new ShadowDrawingSettings(_cullingResults, light.visibleLightIndex);
        _cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f,
            out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData 
        );
        shadowSettings.splitData = splitData;
        _buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
        dirShadowMatrices[index] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, SetTileViewport(index, split, tileSize), split);
        ExecuteBuffer();
        _context.DrawShadows(ref shadowSettings);
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
        }
        return m;
    }
    
    private void ExecuteBuffer() {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }
    
    public void Cleanup() {
        _buffer.ReleaseTemporaryRT(dirShadowAtlasID);
        ExecuteBuffer();
    }
}