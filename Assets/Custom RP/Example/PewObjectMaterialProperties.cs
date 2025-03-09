using UnityEngine;

[DisallowMultipleComponent]
public class PewObjectMaterialProperties : MonoBehaviour {
    private static int _baseColorID = Shader.PropertyToID("_BaseColor");
    private static int _cutoffID = Shader.PropertyToID("_Cutoff");

    [SerializeField] 
    private Color baseColor = Color.white;
    [SerializeField, Range(0f, 1f)]
    float cutoff = 0.5f;
    private static MaterialPropertyBlock _block;

    private void Awake() {
        OnValidate();
    }
    
    private void OnValidate() {
        if (_block == null) {
            _block = new MaterialPropertyBlock();
        }
        _block.SetColor(_baseColorID, baseColor);
        _block.SetFloat(_cutoffID, Random.value);
        GetComponent<Renderer>().SetPropertyBlock(_block);
    }
}
