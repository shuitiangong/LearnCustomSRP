using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomShaderGUI : ShaderGUI {
    private MaterialEditor _editor;
    private Object[] _materials;
    private MaterialProperty[] _properties;
    private bool showPresets;
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        base.OnGUI(materialEditor, properties);
        _editor = materialEditor;
        _materials = materialEditor.targets;
        _properties = properties;

        EditorGUILayout.Space();
        showPresets = EditorGUILayout.Foldout(showPresets, "Presets", true);
        if (showPresets) {
            OpaquePreset();
            ClipPreset();
            FadePreset();
            TransparentPreset();    
        }
    }

    private bool SetProperty(string name, float value) {
        MaterialProperty property = FindProperty(name, _properties, false);
        if (property != null) {
            property.floatValue = value;
            return true;
        }

        return false;
    }

    private void SetProperty(string name, string keyword, bool value) {
        if (SetProperty(name, value ? 1f : 0f)) {
            SetKeyword(keyword, value);
        }
    }

    private void SetKeyword(string keyword, bool enabled) {
        if (enabled) {
            foreach (Material m in _materials) {
                m.EnableKeyword(keyword);
            }
        }
        else {
            foreach (Material m in _materials) {
                m.DisableKeyword(keyword);
            }
        }
    }

    bool Clipping {
        set => SetProperty("_Clipping", "_CLIPPING", value);
    }

    bool PremultiplyAlpha {
        set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
    }

    BlendMode SrcBlend {
        set => SetProperty("_SrcBlend", (float)value);
    }

    BlendMode DstBlend {
        set => SetProperty("_DstBlend", (float)value);
    }

    bool ZWrite {
        set => SetProperty("_ZWrite", value ? 1f : 0f);
    }

    bool HasProperty(string name) => FindProperty(name, _properties, false) != null;
    bool HasPremultiplyAlpha => HasProperty("_PremulAlpha");
    
    RenderQueue RenderQueue {
        set {
            foreach (Material m in _materials) {
                m.renderQueue = (int)value;
            }
        }
    }

    private bool PresetButton(string name) {
        if (GUILayout.Button(name)) {
            _editor.RegisterPropertyChangeUndo(name);
            return true;
        }

        return false;
    }

    private void OpaquePreset() {
        if (PresetButton("Opaque")) {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.Geometry;
        }
    }
    
    private void ClipPreset () {
        if (PresetButton("Clip")) {
            Clipping = true;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.AlphaTest;
        }
    }
    
    private void FadePreset () {
        if (PresetButton("Fade")) {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.SrcAlpha;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }
    
    private void TransparentPreset () {
        if (HasPremultiplyAlpha && PresetButton("Transparent")) {
            Clipping = false;
            PremultiplyAlpha = true;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }
}