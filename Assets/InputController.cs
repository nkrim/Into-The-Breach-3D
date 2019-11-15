using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Target.Dir;

public class InputController : MonoBehaviour
{
    // Universal
    // -------------------------------------------------------------------------
    InputState state = InputState.NO_SELECTION;

    // Connections
    // -------------------------------------------------------------------------
    EditorGrid g;
    LevelController lc;

    // Prefabs
    // -------------------------------------------------------------------------
    Target target_prefab;

    // Camera Controller
    // -------------------------------------------------------------------------
    CameraController cc;
    [Header("Camera Controller")]
    public int maxBufferedInput = 2;

    // Level Raycasting
    // -------------------------------------------------------------------------
    [Header("Level Raycasting")]
    public float defaultTargetAlpha = 0.75f;
    public Color neutralColor;
    public Color hostileColor;
    public Color friendlyColor;

    // Targets and Selection
    // -------------------------------------------------------------------------
    Target hovering_target;
    GameObject selected_block; // CHANGE TO LEVEL-SPECIFIC MONOBEHAVIOUR
    GameObject hovering_block; // CHANGE TO LEVEL-SPECIFIC MONOBEHAVIOUR
    Dictionary<Vector3Int, Target> cur_targets;
    Dictionary<Vector2Int, Target> cur_targets_2d;

    // Gizmos
    // -------------------------------------------------------------------------

    public enum InputState {
        TURN_RESOLVING,
        NO_SELECTION,
        FRIENDLY_SELECTED_MOVE,
        FRIENDLY_SELECTED_FIRING,
        BLOCK_SELECTED,
        MENU_OPENED
    }

    // Start is called before the first frame update
    // =========================================================================
    void Start () {
        // Connections
        g = GameObject.FindGameObjectWithTag("EditorGrid").GetComponent<EditorGrid>();
        lc = g.gameObject.GetComponent<LevelController>();

        // Prefab loading
        target_prefab = Resources.Load<Target>("Prefabs/Target");

        // Camera Controller
        cc = Camera.main.GetComponent<CameraController>();
        // Level Raycasting

        // Targets
        hovering_target = GetComponentInChildren<Target>();
        if(!hovering_target) {
            hovering_target = Instantiate<Target>(target_prefab, this.transform);
            hovering_target.gameObject.SetActive(false);
        }
        cur_targets = new Dictionary<Vector3Int, Target>();
        cur_targets_2d = new Dictionary<Vector2Int, Target>();
    }

    // Update is called once per frame
    // =========================================================================
    void Update () {
        LevelRaycast();
        CameraControlInput();

        HandleClick();
    }

    // Set State Function
    // =========================================================================
    bool Select(GameObject selected=null) {
        // Default: hovering_block
        if(selected == null)
            selected = hovering_block;
        // Redundancy check
        if(selected_block == selected)
            return false;

        selected_block = selected;
        return true;
    }
    bool Deselect() {
        ClearTargets();
        if (!selected_block)
            return false;
        selected_block = null;
        return true;
    }
    bool SetState (InputState new_state) {
        if(state == new_state)
            return false;
        state = new_state;

        // FRIENDLY_SELECTED_MOVE
        // ---------------------------------------------------------------------
        if (new_state == InputState.NO_SELECTION) {
            Deselect();
        }
        else if (new_state == InputState.FRIENDLY_SELECTED_MOVE) {
            SetupMoveTargets(selected_block, 5);
        }

        return true;
    }

    // Click Handler
    // =========================================================================
    void HandleClick() {
        int level_layer = LayerMask.NameToLayer("Level");
        int friendly_layer = LayerMask.NameToLayer("Friendly");
        int hostile_layer = LayerMask.NameToLayer("Hostile");

        if (Input.GetMouseButtonDown(0)) {
            // Selection
            if(hovering_block) { 
                Vector3Int hovering_cell = g.LocalToCell(hovering_block.transform.position);
                if(selected_block && hovering_cell == g.LocalToCell(selected_block.transform.position)) {
                    SetState(InputState.NO_SELECTION);
                    return;
                }
                int layer = hovering_block.gameObject.layer;

                // FRIENDLY_SELECTED_MOVE
                if (state == InputState.FRIENDLY_SELECTED_MOVE) {
                    if(cur_targets.ContainsKey(hovering_cell)) {
                        MoveSelectable(selected_block.transform, g.LocalToCell(hovering_cell + Vector3Int.up));
                        SetState(InputState.NO_SELECTION);
                        return;
                    }
                }

                if(layer == friendly_layer) {
                    if (Select()) {
                        SetState(InputState.FRIENDLY_SELECTED_MOVE);
                    }
                }
                else {
                    SetState(InputState.NO_SELECTION);
                }
            }
        }
    }

    // Update Call Functions
    // =========================================================================
    void CameraControlInput () {
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

    bool LevelRaycast() {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(r, out RaycastHit hit, 100f, LayerMask.GetMask(new string[] { "Level", "Friendly", "Hostile" }))) {
            int level_layer = LayerMask.NameToLayer("Level");
            int friendly_layer = LayerMask.NameToLayer("Friendly");
            int hostile_layer = LayerMask.NameToLayer("Hostile");
            hovering_block = hit.transform.gameObject;
            int layer = hovering_block.layer;

            // Level tile
            if (layer == level_layer) {
                hovering_target.transform.position = hit.transform.position;
                Target.Dir dir = Target.GetDir(hit.normal.normalized);
                hovering_target.Direction = dir;
            }
            else {
                hovering_target.transform.position = g.transform.localToWorldMatrix * g.CellToLocal(g.LocalToCell(hit.transform.localPosition) + Vector3Int.down);
                hovering_target.Direction = Target.Dir.Up;
            }

            // Set target color
            if (layer == friendly_layer) {
                hovering_target.SetColor(friendlyColor, defaultTargetAlpha);

            }
            else if (layer == hostile_layer)
                hovering_target.SetColor(hostileColor, defaultTargetAlpha);
            else
                hovering_target.SetColor(neutralColor, defaultTargetAlpha);
            // Activate target
            if (!hovering_target.gameObject.activeSelf)
                hovering_target.gameObject.SetActive(true);

            // Return
            return true;
        }
        else if (hovering_target.gameObject.activeSelf) {
            hovering_target.gameObject.SetActive(false);
        }
        // Return false (should have returned true in Physics.Raycast conditional
        return false;
    }

    // Target Functions
    // =========================================================================
    static Vector3Int[] move_deltas = {
        new Vector3Int(0,0,1), Vector3Int.right, new Vector3Int(0,0,-1), Vector3Int.left
    };
    int SetupMoveTargets (GameObject friendly, int steps) {
        float cur_time = Time.time;
        Vector3Int starting_loc = g.LocalToCell(friendly.transform.localPosition) + Vector3Int.down;
        // Setup target to be used as instation
        Target base_target = Instantiate<Target>(target_prefab, this.transform);
        Vector3 base_target_position = g.CellToLocal(starting_loc);
        base_target.transform.position = base_target_position;
        base_target.Direction = Target.Dir.Up;
        base_target.SetColor(friendlyColor, defaultTargetAlpha/2);
        base_target.steps = 0;
        base_target.creation_time = cur_time;
        // @FUTURE: Setup reusability of targets instead of clearing
        ClearTargets();
        AddTarget(base_target, starting_loc);
        // Setup for iteration
        List<List<Vector3Int>> dist_passes = new List<List<Vector3Int>>();
        List<Vector3Int> prev_pass = new List<Vector3Int> { starting_loc };
        for(int i=1; i<=steps; i++) {
            List<Vector3Int> cur_pass = new List<Vector3Int>();
            foreach(Vector3Int loc in prev_pass) {
                foreach(Vector3Int d in move_deltas) {
                    Vector3Int next = loc + d;
                    Vector2Int next_2d = new Vector2Int(next.x, next.z);
                    if(cur_targets_2d.ContainsKey(next_2d))
                        continue;
                    // Flat movement
                    if(lc.HasLevelBlock(next) && !lc.HasAnyBlock(next + Vector3Int.up)) {
                        cur_pass.Add(next);
                        Target t = Instantiate<Target>(base_target, this.transform);
                        t.steps = i;
                        // Set target position
                        t.transform.position = base_target_position + g.CellToLocal(next - starting_loc);
                        // Add to target_locs dictionary
                        AddTarget(t, next);
                    }
                    // TESTING PURPOSE, SHOULD USE BETTER METHODS LATER
                    // Up 1 movement
                    else {
                        next += Vector3Int.up;
                        if (lc.HasLevelBlock(next) && !lc.HasAnyBlock(next + Vector3Int.up)) {
                            cur_pass.Add(next);
                            Target t = Instantiate<Target>(base_target, this.transform);
                            t.steps = i;
                            // Set target position
                            t.transform.position = base_target_position + g.CellToLocal(next - starting_loc);
                            // Add to target_locs dictionary
                            AddTarget(t, next);
                        }
                        // Down 1 movement
                        else {
                            next -= new Vector3Int(0,2,0);
                            if (lc.HasLevelBlock(next) && !lc.HasAnyBlock(next + Vector3Int.up)) {
                                cur_pass.Add(next);
                                Target t = Instantiate<Target>(base_target, this.transform);
                                t.steps = i;
                                // Set target position
                                t.transform.position = base_target_position + g.CellToLocal(next - starting_loc);
                                // Add to target_locs dictionary
                                AddTarget(t, next);
                            }
                        }
                    }
                }
            }
            dist_passes.Add(cur_pass);
            prev_pass = cur_pass;
        }

        return cur_targets.Count;
    }
    bool AddTarget(Target t, Vector3Int? cell=null) {
        Vector3Int cell_value = cell ?? g.LocalToCell(t.transform.position);
        cur_targets.Add(cell_value, t);
        Vector2Int cell_2d = new Vector2Int(cell_value.x, cell_value.z);
        if(cur_targets_2d.TryGetValue(cell_2d, out Target o) && o.transform.position.y+0.001 > t.transform.position.y)
            return false;
        cur_targets_2d.Add(cell_2d, t);
        return true;
    }
    void ClearTargets() {
        foreach(Target t in cur_targets.Values) {
            Destroy(t.gameObject);
        }
        cur_targets.Clear();
        cur_targets_2d.Clear();
    }

    // Interaction
    // =========================================================================
    void MoveSelectable(Transform t, Vector3Int new_cell) {
        lc.RemoveSelectable(t);
        t.position = g.CellToLocal(new_cell);
        lc.AddSelectable(t);

        //// TEST TEST TEST
        Arc arc = GameObject.Find("Arc").GetComponent<Arc>();
        arc.positionA = new_cell;
        arc.UpdateArcAndRenderer();
    }
}
