using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRendererPipeline : RenderPipeline {
    public override RenderPipelineGlobalSettings defaultSettings { get; }
    private CameraRenderer _renderer = new CameraRenderer();
    private bool _useDynamicBatching;
    private bool _useGPUInstancing;
    private ShadowSettings _shadowSettings;
    
    public CustomRendererPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher, ShadowSettings shadowSettings) {
        this._useDynamicBatching = useDynamicBatching;
        this._useGPUInstancing = useGPUInstancing;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
        this._shadowSettings = shadowSettings;
    }
    
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras) {
        base.Render(context, cameras);
        foreach (var cam in cameras) {
            _renderer.Render(context, cam, _useDynamicBatching, _useGPUInstancing, _shadowSettings);
        }
    }

    protected override void ProcessRenderRequests(ScriptableRenderContext context, Camera camera, List<Camera.RenderRequest> renderRequests) {
        base.ProcessRenderRequests(context, camera, renderRequests);
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras) { }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
    }
}