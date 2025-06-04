using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/CustomRenderPipelineAsset")]
public class CustomRenderPipelineAsset : RenderPipelineAsset {
    [SerializeField] private bool useDynamicBatching = true;
    [SerializeField] private bool useGPUInstancing = true;
    [SerializeField] private bool useSRPBatcher = true;
    [SerializeField] private ShadowSettings shadows = default;
    
    protected override RenderPipeline CreatePipeline() {
        return new CustomRendererPipeline(useDynamicBatching, useGPUInstancing, useSRPBatcher, shadows);
    }
}
