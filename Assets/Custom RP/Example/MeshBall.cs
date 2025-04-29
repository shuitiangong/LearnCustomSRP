using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBall : MonoBehaviour {
    private static int _baseColorID = Shader.PropertyToID("_BaseColor");
    private static int _metallicID = Shader.PropertyToID("_Metallic");
    private static int _smoothnessID = Shader.PropertyToID("_Smoothness");
    
    public Mesh mesh = default;
    public Material material = default;

    private Matrix4x4[] _matrices = new Matrix4x4[1023];
    private Vector4[] _baseColors = new Vector4[1023];
    private float[] _metallic = new float[1023];
    private float[] _smoothness = new float[1023];
    
    private MaterialPropertyBlock _block;

    private void Awake() {
        for (int i = 0; i < _matrices.Length; ++i) {
            _matrices[i] = Matrix4x4.TRS(
                Random.insideUnitSphere * 10f, 
                Quaternion.Euler(
                    Random.value * 360f, Random.value * 360f, Random.value * 360f
                ), 
                Vector3.one
            );
            _baseColors[i] = new Vector4(Random.value, Random.value, Random.value, Random.Range(0.5f, 1f));
            _metallic[i] = Random.value < 0.25f ? 1f : 0f;
            _smoothness[i] = Random.Range(0.05f, 0.95f);
        }
    }

    private void Update() {
        if (_block == null) {
            _block = new MaterialPropertyBlock();
            _block.SetVectorArray(_baseColorID, _baseColors);
            _block.SetFloatArray(_metallicID, _metallic);
            _block.SetFloatArray(_smoothnessID, _smoothness);
        }
        Graphics.DrawMeshInstanced(mesh, 0, material, _matrices, 1023, _block);
    }
}
