using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBall : MonoBehaviour {
    private static int _baseColorID = Shader.PropertyToID("_BaseColor");
    public Mesh mesh = default;
    public Material material = default;

    private Matrix4x4[] _matrices = new Matrix4x4[1023];
    private Vector4[] _baseColors = new Vector4[1023];

    private MaterialPropertyBlock _block;

    private void Awake() {
        for (int i = 0; i < _matrices.Length; ++i) {
            _matrices[i] = Matrix4x4.TRS(
                Random.insideUnitSphere * 10f, Quaternion.identity, Vector3.one
            );
            _baseColors[i] = new Vector4(Random.value, Random.value, Random.value, 1f);
        }
    }

    private void Update() {
        if (_block == null) {
            _block = new MaterialPropertyBlock();
            _block.SetVectorArray(_baseColorID, _baseColors);
        }
        Graphics.DrawMeshInstanced(mesh, 0, material, _matrices, 1023, _block);
    }
}
