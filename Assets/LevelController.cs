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
        selectable_objects = new Dictionary<Vector3Int,Transform>();
        // Fill level_blocks
        init_level_blocks();
    }

    int init_level_blocks(Transform parent = null) {
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
        return level_blocks.Count;
    }

    public int AddLevelBlock (Transform t) {
        Vector3Int cell = g.LocalToCell(t.localPosition);
        selectable_objects.Add(cell, t);
        return level_blocks.Count;
    }
    public bool RemoveLevelBlock (Transform t) {
        Vector3Int cell = g.LocalToCell(t.localPosition);
        return level_blocks.Remove(cell);
    }

    public int AddObstacle (Transform t) {
        Vector3Int cell = g.LocalToCell(t.localPosition);
        selectable_objects.Add(cell, t);
        return obstacle_blocks.Count;
    }
    public bool RemoveObstacle (Transform t) {
        Vector3Int cell = g.LocalToCell(t.localPosition);
        return obstacle_blocks.Remove(cell);
    }

    public int AddSelectable(Transform t) {
        Vector3Int cell = g.LocalToCell(t.localPosition);
        selectable_objects.Add(cell, t);
        return selectable_objects.Count;
    }
    public bool RemoveSelectable (Transform t) {
        Vector3Int cell = g.LocalToCell(t.localPosition);
        return selectable_objects.Remove(cell);
    }
}
