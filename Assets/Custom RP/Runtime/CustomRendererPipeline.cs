using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRendererPipeline : RenderPipeline {
    public override RenderPipelineGlobalSettings defaultSettings { get; }
    private CameraRenderer _renderer = new CameraRenderer();
    
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras) {
        base.Render(context, cameras);
        foreach (var cam in cameras) {
            _renderer.Render(context, cam);
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