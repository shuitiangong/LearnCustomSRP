using UnityEngine;
using UnityEngine.Rendering;

public class Lighting {
    private const string _bufferName = "Lighting";
    private CommandBuffer _buffer = new CommandBuffer{
        name = _bufferName
    };

    private static int dirLightColorID = Shader.PropertyToID("_DirectionalLightColor");
    private static int dirLightDirectionID = Shader.PropertyToID("_DirectionalLightDirection");

    public void Setup(ScriptableRenderContext context) {
        _buffer.BeginSample(_bufferName);
        SetupDirectionalLight();
        _buffer.EndSample(_bufferName);
        context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    private void SetupDirectionalLight() {
        Light light = RenderSettings.sun;
        _buffer.SetGlobalVector(dirLightColorID, light.color.linear * light.intensity);
        _buffer.SetGlobalVector(dirLightDirectionID, -light.transform.forward); ;
    }
}