using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting {
    private const string _bufferName = "Lighting";
    private CommandBuffer _buffer = new CommandBuffer{
        name = _bufferName
    };

    private CullingResults _cullingResults;

    private const int maxDirLightCount = 4;
    private static int dirLightCountID = Shader.PropertyToID("_DirectionalLightCount");
    private static int dirLightColorsID = Shader.PropertyToID("_DirectionalLightColors");
    private static int dirLightDirectionsID = Shader.PropertyToID("_DirectionalLightDirections");
    private static int dirLightShadowDataID = Shader.PropertyToID("_DirectionalLightShadowData");

    private static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    private static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];
    private static Vector4[] dirLightShadowData = new Vector4[maxDirLightCount];
    
    private Shadows _shadows = new Shadows();

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings) {
        this._cullingResults = cullingResults;
        
        _buffer.BeginSample(_bufferName);
        _shadows.Setup(context, cullingResults, shadowSettings);
        SetupLights();
        _shadows.Render();
        _buffer.EndSample(_bufferName);
        context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    private void SetupLights() {
        NativeArray<VisibleLight> visibleLights = _cullingResults.visibleLights;
        for (int i = 0; i < visibleLights.Length; ++i) {
            if (i >= maxDirLightCount) break;
            VisibleLight visibleLight = visibleLights[i];
            SetupDirectionalLight(i, ref visibleLight);
        }
        
        _buffer.SetGlobalInt(dirLightCountID, visibleLights.Length);
        _buffer.SetGlobalVectorArray(dirLightColorsID, dirLightColors);
        _buffer.SetGlobalVectorArray(dirLightDirectionsID, dirLightDirections);
        _buffer.SetGlobalVectorArray(dirLightShadowDataID, dirLightShadowData);
    }
    
    private void SetupDirectionalLight(int index, ref VisibleLight visibleLight) {
        dirLightColors[index] = visibleLight.finalColor;
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        dirLightShadowData[index]=  _shadows.ReserveDirectionalShadows(visibleLight.light, index);
    }

    public void Cleanup() {
        _shadows.Cleanup();
    }
}