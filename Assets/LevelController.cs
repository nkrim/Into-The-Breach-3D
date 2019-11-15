using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EditorGrid))]
public class LevelController : MonoBehaviour
{
    Dictionary<Vector3Int, Transform> level_blocks;
    Dictionary<Vector3Int, Transform> obstacle_blocks;
    Dictionary<Vector3Int, Transform> selectable_objects;

    EditorGrid g;

    private void Start () {
        g = GetComponent<EditorGrid>();

        level_blocks = new Dictionary<Vector3Int,Transform>();
        obstacle_blocks = new Dictionary<Vector3Int, Transform>();
        selectable_objects = new Dictionary<Vector3Int,Transform>();
        // Fill level_blocks
        init_blocks();
    }

    int init_blocks(Transform parent = null) {
        // Level blocks
        if(!parent)
            parent = this.transform;
        int level_layer = LayerMask.NameToLayer("Level");
        level_blocks.Clear();
        int n = parent.childCount;
        for(int i=0; i<n; i++) {
            Transform child = parent.GetChild(i);
            if(child.gameObject.layer == level_layer)
                level_blocks.Add(g.LocalToCell(child.localPosition), child);
        }

        // Obstacle blocks
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach(GameObject o in obstacles) {
            Transform t = o.transform;
            obstacle_blocks.Add(g.LocalToCell(t.position), t);
        }

        // Selectable blocks
        GameObject[] selectables = GameObject.FindGameObjectsWithTag("Selectable");
        foreach (GameObject s in selectables) {
            Transform t = s.transform;
            selectable_objects.Add(g.LocalToCell(t.position), t);
        }

        return level_blocks.Count + obstacle_blocks.Count + selectable_objects.Count;
    }

    public Transform GetAnyBlock (Vector3Int cell) {
        Transform t;
        if((t = GetLevelBlock(cell)) != null)
            return t;
        if((t = GetObstacle(cell)) != null)
            return t;
        return GetSelectable(cell);
    }

    public bool HasAnyBlock(Vector3Int cell) {
        return  HasLevelBlock(cell)
                || HasObstacle(cell)
                || HasSelectable(cell);
    }

    public Transform GetLevelBlock(Vector3Int cell) {
        Transform t = null;
        level_blocks.TryGetValue(cell, out t);
        return t;
    }
    public bool HasLevelBlock (Vector3Int cell) {
        return level_blocks.ContainsKey(cell);
    }
    public int AddLevelBlock (Transform t, Vector3Int? cell = null) {
        Vector3Int cell_value = cell ?? g.LocalToCell(t.localPosition);
        level_blocks.Add(cell_value, t);
        return level_blocks.Count;
    }
    public bool RemoveLevelBlock (Transform t) {
        Vector3Int cell = g.LocalToCell(t.localPosition);
        return level_blocks.Remove(cell);
    }

    public Transform GetObstacle (Vector3Int cell) {
        Transform t = null;
        obstacle_blocks.TryGetValue(cell, out t);
        return t;
    }
    public bool HasObstacle (Vector3Int cell) {
        return obstacle_blocks.ContainsKey(cell);
    }
    public int AddObstacle (Transform t, Vector3Int? cell = null) {
        Vector3Int cell_value = cell ?? g.LocalToCell(t.localPosition);
        obstacle_blocks.Add(cell_value, t);
        return obstacle_blocks.Count;
    }
    public bool RemoveObstacle (Transform t) {
        Vector3Int cell = g.LocalToCell(t.localPosition);
        return obstacle_blocks.Remove(cell);
    }

    public Transform GetSelectable (Vector3Int cell) {
        Transform t = null;
        selectable_objects.TryGetValue(cell, out t);
        return t;
    }
    public bool HasSelectable (Vector3Int cell) {
        return selectable_objects.ContainsKey(cell);
    }
    public int AddSelectable(Transform t, Vector3Int? cell=null) {
        Vector3Int cell_value = cell ?? g.LocalToCell(t.localPosition);
        selectable_objects.Add(cell_value, t);
        return selectable_objects.Count;
    }
    public bool RemoveSelectable (Transform t) {
        Vector3Int cell = g.LocalToCell(t.localPosition);
        return selectable_objects.Remove(cell);
    }
}
