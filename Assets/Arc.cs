using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arc : MonoBehaviour
{
    public Vector3 positionA;
    public Vector3 positionB;
    public float power = 10;
    public bool useHighAngle = true;

    private Vector3 _posA;
    private Vector3 _posB;
    private float _time;
    private float _angle;
    private float _rad;
    private float _power;
    private bool _use_high = true;

    private Quaternion _pivot_quat;
    private float _initial_velocity;

    private bool possible_shot = false;
    
    // Start is called before the first frame update
    void Start()
    { 
        _initial_velocity = power;
        ComputeBestAngles();
    }


    public Vector3 PosAtT(float time) {
        float x = _initial_velocity * time * Mathf.Cos(_rad);
        float y = _initial_velocity * time * Mathf.Sin(_rad) + 0.5f*Physics.gravity.y*time*time;
        Vector3 pivoted = _pivot_quat*(new Vector3(0, y, x));
        return pivoted + positionA;
    }
    public Vector3 SlopeAtT(float time) {
        float x = _initial_velocity * Mathf.Cos(_rad);
        float y = _initial_velocity * Mathf.Sin(_rad) + Physics.gravity.y * time;
        Vector3 pivoted = _pivot_quat*(new Vector3(0, y, x));
        return pivoted;
    }

    public Vector3 PosAtPercent(float percent) {
        return PosAtT(percent*_time);
    }
    public Vector3 SlopeAtPercent(float percent) {
        return SlopeAtT(percent*_time);
    }

    public float TimeToLand(float? other_angle=null) {
        Vector3 BA = _posB - _posA;
        float x = new Vector3(BA.x, 0, BA.z).magnitude;
        float t = x / (_initial_velocity * Mathf.Cos(other_angle == null ? _rad : other_angle.Value));
        return t;
    }

    public Vector2? ComputeBestAngles() {
        Vector3 BA = _posB - _posA;
        float x = new Vector3(BA.x, 0, BA.z).magnitude;
        float y = BA.y;

        float g = -Physics.gravity.y;
        float v0 = _initial_velocity;
        float v0_2 = v0*v0;

        float sqrt_val = v0_2 * v0_2 - (g * (g * x * x + 2 * y * v0_2));
        if (sqrt_val < 0) {
            possible_shot = false;
            return null;
        }
        float val = Mathf.Sqrt(sqrt_val);
        float gx = g*x;

        float estimate_angle_high = Mathf.Atan((v0_2 + val)/gx);
        float estimate_angle_low = Mathf.Atan((v0_2 - val) / gx);

        if(_use_high) {
            _rad = estimate_angle_high;
        }
        else {
            _rad = estimate_angle_low;
        }
        _time = TimeToLand();

        possible_shot = true;

        return new Vector2(estimate_angle_low, estimate_angle_high);
    }
    public Quaternion GetPivotQuat() {
        Quaternion q = new Quaternion();
        Vector3 posBA = (_posB - _posA);
        Vector3 dir = new Vector3(posBA.x, 0, posBA.z).normalized;
        q.SetFromToRotation(Vector3.forward, dir);
        return q;
    }

    static int NumGizmos = 20;
    private void OnDrawGizmos () {
        Gizmos.DrawCube(positionA, new Vector3(0.2f,0.2f,0.2f));
        Gizmos.DrawCube(positionB, new Vector3(0.2f, 0.2f, 0.2f));
        if (possible_shot) { 
            for (int i=0; i<NumGizmos; i++) {
                float t = (i*_time)/(NumGizmos-1);
                Gizmos.DrawWireSphere(PosAtT(t), 0.1f);
            }
        }
    }

    private void OnValidate () {
        bool any_changed = false;
        bool positions_changed = false;
        if(positionA != _posA) {
            any_changed = true;
            positions_changed = true;
            _posA = positionA;
        }
        if (positionB != _posB) {
            any_changed = true;
            positions_changed = true;
            _posB = positionB;
        }
        /*if(Mathf.Abs(time - _time) > 0.001) {
            //any_changed = true;
            _time = time;
        }*/
        /*if (Mathf.Abs(angle - _angle) > 0.001) {
            any_changed = true;
            _angle = angle;
            _rad = Mathf.Deg2Rad*_angle;
        }*/
        if(Mathf.Abs(power - _power) > 0.001) {
            any_changed = true;
            _power = power;
            _initial_velocity = _power;
        }
        if(useHighAngle != _use_high) {
            any_changed = true;
            _use_high = useHighAngle;
        }

        if(any_changed) {
            ComputeBestAngles();
            //print(ComputeVelocity());
        }
        if(positions_changed)
            _pivot_quat = GetPivotQuat();

        // If arcrenderer
        ArcRenderer ar;
        if((any_changed || positions_changed) && (ar = GetComponent<ArcRenderer>())) {
            ar.GenerateMesh();
        }
    }
}
