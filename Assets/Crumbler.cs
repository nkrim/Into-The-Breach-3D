using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crumbler : MonoBehaviour
{
    public Transform toCrumble;
    public ParticleSystem smoke;
    public float crumbleTime = 1;
    public float offsetTime = 0.25f;

    bool crumbling;
    float start_time;

    private void Start () {
        start_time = Time.time + offsetTime;
        crumbling = true;
        if(toCrumble) {
            Material m = toCrumble.GetComponent<MeshRenderer>().sharedMaterial;
            m.SetFloat("Vector1_EAE4A38B", start_time);
            m.SetInt("Vector1_E7BD001B", 1);
        }
        if(smoke) {
            smoke.transform.position = toCrumble.position + 0.5f*Vector3.up;
            smoke.Play();
        }
    }

    private void Update () {
        if(crumbling) {
            float dt = Time.time - start_time;
            if(dt < 0)
                dt = 0;
            else if (dt >= 1) {
                crumbling = false;
                dt = 1;
            }
            smoke.transform.position = (toCrumble.position + 0.65f * Vector3.up) - Vector3.up*dt;
        }
    }
}
