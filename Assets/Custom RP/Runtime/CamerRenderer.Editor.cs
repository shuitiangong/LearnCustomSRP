#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Profiling;
#endif
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer {
#if UNITY_EDITOR

    private static ShaderTagId[] legacyShaderTagId = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };        
    private static Material errorMaterial;
    
    private void PrepareBuffer() {
        Profiler.BeginSample("Editor Only");
        SampleName = _camera.name;
        _buffer.name = SampleName;
        Profiler.EndSample();
    }
    
    private void PrepareForSceneWindow() {
        if (_camera.cameraType == CameraType.SceneView) {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
    }
    
    private void DrawGizmos() {
        if (Handles.ShouldRenderGizmos()) {
            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }
    }
    
    private void DrawUnsupportedShaders() {
        if (errorMaterial == null) {
            errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        var drawingSettings = new DrawingSettings(legacyShaderTagId[0], new SortingSettings(_camera)) {
            overrideMaterial = errorMaterial
        };
        for (int i = 1; i < legacyShaderTagId.Length; ++i) {
            drawingSettings.SetShaderPassName(i, legacyShaderTagId[i]);
        }
        var filteringSettings = FilteringSettings.defaultValue;
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }
#endif
}
