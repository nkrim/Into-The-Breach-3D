using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Grid))]
public class EditorGrid : MonoBehaviour
{
    public Vector3 offset;

    protected Grid grid;

    private void Start () {
        grid = GetComponent<Grid>();
    }


    public void SetToCenter() {
        offset = Vector3.zero;
    }
    public void SetToOddCenter() {
        offset = grid.cellSize/2;
    }

    public Vector3Int LocalToCell (Vector3 position, Vector3? other_offset=null) {
        Vector3 cur_offset = other_offset.HasValue ? other_offset.Value : this.offset;
        return grid.LocalToCell(position - cur_offset);
    }
    public Vector3 CellToLocal (Vector3Int cell, Vector3? other_offset=null) {
        Vector3 cur_offset = other_offset.HasValue ? other_offset.Value : this.offset;
        return grid.CellToLocal (cell) + cur_offset;
    }
    public Vector3 GetCellSnappedPosition (Vector3 position, Vector3? other_offset=null) {
        if(!other_offset.HasValue)
            return CellToLocal(LocalToCell(position));
        return CellToLocal(LocalToCell(position, other_offset), other_offset);
    }
}
