using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Target.Dir;

public class InputController : MonoBehaviour
{
    // Camera Controller
    CameraController cc;
    [Header("Camera Controller")]
    public int maxBufferedInput = 2;

    // Level Raycasting
    Target target;


    // Start is called before the first frame update
    void Start()
    {
        // Camera Controller
        cc = Camera.main.GetComponent<CameraController>();
        // Level Raycasting
        target = GetComponentInChildren<Target>();
        if(!target) {
            target = Instantiate<Target>(Resources.Load<Target>("Prefabs/Target"), this.transform);
            target.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        LevelRaycast();
        CameraControlInput();
    }

    void CameraControlInput() {
        if(!cc)
            return;
        if(cc.IsTransforming()) {
            int cur_radial = 0;
            if (Input.GetKeyDown(KeyCode.RightArrow)) cur_radial -= 1;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) cur_radial += 1;
            int cur_angular = 0;
            if (Input.GetKeyDown(KeyCode.DownArrow)) cur_angular -= 1;
            if (Input.GetKeyDown(KeyCode.UpArrow)) cur_angular += 1;
            cc.SmoothTransform(cur_radial, cur_angular);
        }
        else {
            int cur_radial = 0;
            if (Input.GetKey(KeyCode.RightArrow)) cur_radial -= 1;
            if (Input.GetKey(KeyCode.LeftArrow)) cur_radial += 1;
            int cur_angular = 0;
            if (Input.GetKey(KeyCode.DownArrow)) cur_angular -= 1;
            if (Input.GetKey(KeyCode.UpArrow)) cur_angular += 1;
            cc.SmoothTransform(cur_radial, cur_angular);
        }
    }

    void LevelRaycast() {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(r, out hit, 100f, LayerMask.GetMask(new string[] {"Level"}))) {
            target.transform.position = hit.transform.position;
            Target.Dir dir = Target.GetDir(hit.normal.normalized);
            target.Direction = dir;
            if(!target.gameObject.activeSelf)
                target.gameObject.SetActive(true);
        }
        else if(target.gameObject.activeSelf) {
            target.gameObject.SetActive(false);
        }
    }
}
