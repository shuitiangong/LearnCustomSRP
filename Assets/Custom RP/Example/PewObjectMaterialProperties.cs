using UnityEngine;

[DisallowMultipleComponent]
public class PewObjectMaterialProperties : MonoBehaviour {
    private  static int _baseColorID = Shader.PropertyToID("_BaseColor");
    
    [SerializeField]
    private Color baseColor = Color.white;
    private static MaterialPropertyBlock _block;

    private void Awake() {
        OnValidate();
    }
    
    private void OnValidate() {
        if (_block == null) {
            _block = new MaterialPropertyBlock();
        }
        _block.SetColor(_baseColorID, baseColor);
        GetComponent<Renderer>().SetPropertyBlock(_block);
    }
}
