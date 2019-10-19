using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Target.Dir;

public class InputController : MonoBehaviour
{
    EditorGrid g;
    LevelController lc;

    // Camera Controller
    CameraController cc;
    [Header("Camera Controller")]
    public int maxBufferedInput = 2;

    // Level Raycasting
    [Header("Level Raycasting")]
    public float defaultTargetAlpha = 0.75f;
    public Color neutralColor;
    public Color hostileColor;
    public Color friendlyColor;
    Target target;
    GameObject hovering_block; // CHANGE TO LEVEL-SPECIFIC MONOBEHAVIOUR 

    public enum InputState {
        TURN_RESOLVING,
        NO_SELECTION,
        FRIENDLY_SELECTED_MOVE,
        FRIENDLY_SELECTED_FIRING,
        BLOCK_SELECTED,
        MENU_OPENED
    }

    // Start is called before the first frame update
    void Start()
    {
        g = GameObject.FindGameObjectWithTag("EditorGrid").GetComponent<EditorGrid>();
        lc = g.gameObject.GetComponent<LevelController>();
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
        if(Physics.Raycast(r, out hit, 100f, LayerMask.GetMask(new string[] {"Level", "Friendly", "Hostile"}))) {
            int level_layer = LayerMask.NameToLayer("Level");
            int friendly_layer = LayerMask.NameToLayer("Friendly");
            int hostile_layer = LayerMask.NameToLayer("Hostile");
            hovering_block = hit.transform.gameObject;
            int layer = hovering_block.layer;
            // Level tile
            if(layer == level_layer) { 
                target.transform.position = hit.transform.position;
                Target.Dir dir = Target.GetDir(hit.normal.normalized);
                target.Direction = dir;
            }
            else {
                target.transform.position = g.transform.localToWorldMatrix * g.CellToLocal(g.LocalToCell(hit.transform.localPosition) + Vector3Int.down);
                target.Direction = Target.Dir.Up;
            }
            // Set target color
            if(layer == friendly_layer)
                target.SetColor(friendlyColor, defaultTargetAlpha);
            else if(layer == hostile_layer)
                target.SetColor(hostileColor, defaultTargetAlpha);
            else
                target.SetColor(neutralColor, defaultTargetAlpha);
            // Activate target
            if (!target.gameObject.activeSelf)
                target.gameObject.SetActive(true);
        }
        else if(target.gameObject.activeSelf) {
            target.gameObject.SetActive(false);
        }
    }
}
