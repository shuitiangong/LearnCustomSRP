using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoFactory : MonoBehaviour {
    public GameObject go;
    // Start is called before the first frame update
    void Start() {
        for (int i = 0; i < 76; ++i) {
            var ng = Instantiate(go, this.transform);
            ng.transform.localPosition = new Vector3(Random.Range(0, 10), 0, Random.Range(0, 10));
            var mr = ng.GetComponent<MeshRenderer>();
            mr.material.SetColor("_BaseColor", new Color(1.0f*i/76, 1-1.0f*i/76, Random.Range(0.0f, 1.0f)));
            ng.SetActive(true);
        }
    }
}
