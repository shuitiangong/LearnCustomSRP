using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour {
    private static int _baseColorID = Shader.PropertyToID("_BaseColor");
    private static int _cutoffID = Shader.PropertyToID("_Cutoff");
    private static int _metallicID = Shader.PropertyToID("_Metallic");
    private static int _smoothnessID = Shader.PropertyToID("_Smoothness");

    [SerializeField] 
    private Color baseColor = Color.white;
    [SerializeField, Range(0f, 1f)]
    float cutoff = 0.5f;
    [SerializeField, Range(0f, 1f)]
    float metallic = 0f;
    [SerializeField, Range(0f, 1f)]
    float smoothness = 0.5f;
    private static MaterialPropertyBlock _block;

    private void Awake() {
        OnValidate();
    }
    
    private void OnValidate() {
        if (_block == null) {
            _block = new MaterialPropertyBlock();
        }
        _block.SetColor(_baseColorID, baseColor);
        _block.SetFloat(_cutoffID, cutoff);
        _block.SetFloat(_metallicID, metallic);
        _block.SetFloat(_smoothnessID, smoothness);
        GetComponent<Renderer>().SetPropertyBlock(_block);
    }
}
