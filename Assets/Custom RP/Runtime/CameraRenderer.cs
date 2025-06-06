﻿using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer {
    private ScriptableRenderContext _context;
    private Camera _camera;
    private const string _bufferName = "Render Camera";
    private CullingResults _cullingResults;
    private static ShaderTagId _unlitShaderTagID = new ShaderTagId("SRPDefaultUnlit");
    private static ShaderTagId _litShaderTagID = new ShaderTagId("CustomLit");
    private string SampleName { get; set; } = _bufferName;

    private CommandBuffer _buffer = new CommandBuffer {
        name = _bufferName
    };

    private Lighting _lighting = new Lighting();
    
    public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, 
        bool useGPUInstancing, ShadowSettings shadowSettings) {
        this._context = context;
        this._camera = camera;
#if UNITY_EDITOR
        PrepareBuffer();
        PrepareForSceneWindow();
#endif
        if (!Cull(shadowSettings.maxDistance)) {
            return;
        }
        
        _buffer.BeginSample(SampleName);
        ExecuteBuffer();
        //为什么要调整到下面？
        //Setup();
        _lighting.Setup(context, _cullingResults, shadowSettings);
        Setup();
        _buffer.EndSample(SampleName);

        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
#if UNITY_EDITOR
        DrawUnsupportedShaders();
        DrawGizmos();
#endif
        _lighting.Cleanup();
        Submit();
    }

    private void Setup() {
        _context.SetupCameraProperties(_camera);
        CameraClearFlags flags = _camera.clearFlags;
        _buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth, 
            flags == CameraClearFlags.Color, 
            flags == CameraClearFlags.Color ? _camera.backgroundColor : Color.clear
        );
        _buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }
    
    private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing) {
        var sortingSettings = new SortingSettings(_camera) {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(_unlitShaderTagID, sortingSettings) {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        drawingSettings.SetShaderPassName(1, _litShaderTagID);
        
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        _context.DrawSkybox(_camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }


    private void Submit() {
        _buffer.EndSample(SampleName);
        ExecuteBuffer();
        _context.Submit();
    }

    private void ExecuteBuffer() {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    private bool Cull(float maxShadowDistance) {
        if (_camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {
            p.shadowDistance = Mathf.Min(maxShadowDistance, _camera.farClipPlane);
            _cullingResults = _context.Cull(ref p);
            return true;
        }

        return false;
    }
}