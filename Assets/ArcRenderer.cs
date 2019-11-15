using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Arc))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class ArcRenderer : MonoBehaviour
{
    public int lengthSegments = 50;
    public int radialSegments = 8;
    public float arcThickness = 0.25f;

    public static readonly int MaxRadialSegments = 32;
    public static readonly int MaxLengthSegments = 200;

    Arc arc;
    MeshFilter mf;
    MeshRenderer mr;
    Mesh mesh;

    private void Start () {
        arc = GetComponent<Arc>();
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        GenerateMesh();
    }
    private void OnEnable () {
        if(!arc)
            arc = GetComponent<Arc>();
        if(!mf)
            mf = GetComponent<MeshFilter>();
        if(!mr)
            mr = GetComponent<MeshRenderer>();
    }
    private void OnValidate () {
        if(lengthSegments < 1) lengthSegments = 1;
        else if(lengthSegments > MaxLengthSegments) lengthSegments = MaxLengthSegments;
        if(radialSegments < 3) radialSegments = 3;
        else if(radialSegments > MaxRadialSegments) radialSegments = MaxRadialSegments;
        GenerateMesh();
    }

    public float AddRings(in List<Vector3> vertices, in List<Vector3> normals, in List<Vector2> uvs) {
        float total_length = 0;
        float[] lengths = new float[lengthSegments+1];
        for (int segment = 0; segment <= lengthSegments; segment++) {
            float perc = (float)segment / lengthSegments;
            Vector3 slope = arc.SlopeAtPercent(perc);
            Vector3 pos = arc.PosAtPercent(perc);
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, slope.normalized);
            // Contribute to length
            if(segment > 0) {
                float prev_perc = (float)(segment+1) / lengthSegments;
                Vector3 prev_pos = arc.PosAtPercent(prev_perc);
                Vector3 diff = pos - prev_pos;
                float length = diff.magnitude;
                total_length += length;
                lengths[segment] = total_length;
            }
            else
                lengths[segment] = 0;
            // Fill ring
            for (int i=0; i<=radialSegments; i++) {
                float rad = 2 * Mathf.PI * i / radialSegments;
                Vector3 n = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
                Vector3 v = new Vector3(n.x*arcThickness, 0, n.z*arcThickness);
                n = rot*n;
                v = rot*v + pos;
                vertices.Add(v);
                normals.Add(n);
            }
        }
        // Generate UVs
        for(int segment = 0; segment <= lengthSegments; segment++) {
            float length = lengths[segment];
            float uv_y = length/total_length;
            for (int i = 0; i <= radialSegments; i++) {
                float uv_x = (float)i/radialSegments;
                Vector2 uv = new Vector2(uv_x, uv_y);
                uvs.Add(uv);
            }
        }
        return total_length;
    }
    public int[] ConnectRings() {
        int num_indices = 6*radialSegments*lengthSegments;
        int[] indices = new int[num_indices];
        int i_count = 0;
        for(int i=0; i<lengthSegments; i++) {
            int ring_p_0 = i*(radialSegments+1);
            int ring_n_0 = ring_p_0 + (radialSegments+1);
            int v_br, v_bl, v_tr, v_tl;
            for(int j=0; j<radialSegments; j++) {
                v_br = ring_p_0 + j;
                v_bl = v_br + 1;
                v_tr = ring_n_0 + j;
                v_tl = v_tr + 1;
                indices[i_count++] = v_br; // <- Triangle 1: bottom right
                indices[i_count++] = v_bl;
                indices[i_count++] = v_tr;
                indices[i_count++] = v_tl; // <- Triangle 2: top left
                indices[i_count++] = v_tr;
                indices[i_count++] = v_bl;
            }
        }
        return indices;
    }
    /*public void AddCylinder(int segment, in List<Vector3> vertices, in List<Vector3> normals, in List<int> indices) {
        // Determine constants for this ring
        float perc_lo = (float)segment/lengthSegments;
        float perc_hi = (float)(segment+1)/lengthSegments;
        float perc_mid = (float)(2*segment + 1)/(2*lengthSegments);
        Vector3 slope = arc.SlopeAtPercent(perc_mid);
        Vector3 pos_lo = arc.PosAtPercent(perc_lo);
        Vector3 diff = arc.PosAtPercent(perc_hi) - pos_lo;
        Vector3 offset = pos_lo + .5f*diff;
        print(offset);
        float length = diff.magnitude;
        float length_2 = length/2;
        Vector3 scale = new Vector3(arcThickness, 1, arcThickness);
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, slope.normalized);
        // Init vertex creation
        int i0 = vertices.Count;
        int loopback_index_bot = i0, loopback_index_top = i0+1;
        // Initial vertices
        Vector3 init_norm = new Vector3(0, 0, 1);
        Vector3 init_bot = init_norm - length_2*Vector3.up;
        Vector3 init_top = init_bot + length*Vector3.up;
        // Transform
        init_bot.Scale(scale);
        init_top.Scale(scale);
        init_bot = rot*init_bot + offset;
        init_top = rot*init_top + offset;
        init_norm = rot*init_norm;
        // Add
        vertices.Add(init_bot);
        vertices.Add(init_top);
        normals.Add(init_norm);
        normals.Add(init_norm);
        for (int i=1; i<=radialSegments; i++) {
            int v_b_prv = i0 + ((i - 1) * 2);
            int v_t_prv = v_b_prv + 1;
            int v_b_cur, v_t_cur;
            // Any but last segment
            if (i < radialSegments) {
                float rad = 2 * Mathf.PI * i / radialSegments;
                // Generate the new vertices
                Vector3 norm = new Vector3(-Mathf.Sin(rad), 0, Mathf.Cos(rad));
                Vector3 v_bot = norm - length_2*Vector3.up;
                Vector3 v_top = v_bot + length*Vector3.up;
                // Perform transformations
                v_bot.Scale(scale);
                v_top.Scale(scale);
                v_bot = rot*v_bot + offset;
                v_top = rot*v_top + offset;
                norm = rot*norm;
                // Add to lists
                v_b_cur = v_b_prv+2;
                v_t_cur = v_b_prv+3;
                vertices.Add(v_bot);
                vertices.Add(v_top);
                normals.Add(norm);
                normals.Add(norm);
                
            }
            else {
                v_b_cur = loopback_index_bot;
                v_t_cur = loopback_index_top;
            }
            indices.Add(v_b_prv);   // <- Triangle 1 (bottom-right)
            indices.Add(v_t_cur);
            indices.Add(v_b_cur);
            indices.Add(v_b_prv);   // <- Triangle 2 (top-left)
            indices.Add(v_t_prv);
            indices.Add(v_t_cur);
        }
    }*/

    public void GenerateMesh() {
        if(!arc)
            return;

        if (mesh == null) {
            mesh = new Mesh();
            mf.mesh = mesh;
        }
        else
            mesh.Clear();

        if(!arc.IsPossible()) {
            return;
        } 

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        float length = AddRings(vertices, normals, uvs);
        int[] indices = ConnectRings();

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.SetUVs(0, uvs);

        Material mat = mr.sharedMaterial;
        mat.SetFloat("Vector1_580C16DC", length);
        mat.SetFloat("Vector1_C61FC416", arcThickness);
    }
}
