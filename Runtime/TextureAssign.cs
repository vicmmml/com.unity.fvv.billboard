using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureAssign : MonoBehaviour
{
    public GameObject Cam;
    public RawImage ImageFromConnection;
    private Vector3 relativePos;


    // Update is called once per frame
    void Update()
    {
        this.gameObject.GetComponent<Renderer>().material.mainTexture = ImageFromConnection.texture;

        // Plane rotates only when the camera moves (so the plane always points at it), not when the camera rotates
        relativePos = transform.position - Cam.transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos);
        this.transform.rotation = rotation;

    }
}
