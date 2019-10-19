using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EditorGridSnap : MonoBehaviour
{
    protected EditorGrid editorGrid;
    protected Vector3 oldOffset;

    void Start () {
        editorGrid = GameObject.FindGameObjectWithTag("EditorGrid").GetComponent<EditorGrid>();
        if(editorGrid)
            oldOffset = editorGrid.offset;
    }

    void Update()
    {
        if(!editorGrid && !(editorGrid = GameObject.FindGameObjectWithTag("EditorGrid").GetComponent<EditorGrid>())) {
            Debug.LogWarning("No GameObject with tag `EditorGrid`");
            return;
        }
        if (oldOffset != editorGrid.offset) {
            Vector3 cur_offset = editorGrid.offset;
            Vector3Int temp_cell = editorGrid.LocalToCell(transform.localPosition, oldOffset);
            transform.localPosition = editorGrid.CellToLocal(temp_cell, cur_offset);
            oldOffset = editorGrid.offset;
            transform.hasChanged = false;
        }
        else if (transform.hasChanged) {
            transform.localPosition = editorGrid.GetCellSnappedPosition(transform.localPosition);
            transform.hasChanged = false;
        }
    }
}
