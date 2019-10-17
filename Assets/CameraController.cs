using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour
{
    public float distance = 10f;
    public int radialSteps = 8;
    public int angularSteps = 3;
    public float minAngle = 30;
    public float maxAngle = 90;
    [Header("Current State")]
    public int currentRadialStep = 0;
    public int currentAngularStep = 0;
    [Header("Smooth Transform")]
    public float transformTime = 0.25f;
    public float transformSpeed = 1f;

    protected bool _transforming = false;
    protected float _transform_start;
    protected float _transform_from_radial;
    protected float _transform_from_angular;
    protected float _transform_to_radial;
    protected float _transform_to_angular;

    private void Update () {
        // SMOOTH TRANSFORM UPDATE
        if(_transforming) {
            SmoothTransformStep();
        }
        else {
            // Normalize steps while not transforming
            currentRadialStep = mod(currentRadialStep, radialSteps);
        }
    }

    int signum(int x) {
        if(x == 0)
            return 0;
        return x > 0 ? 1 : -1;
    }
    int mod (int x, int m) {
        int r = x % m;
        return r < 0 ? r + m : r;
    }
    float get_dt() {
        float dt = (Time.time - _transform_start) / transformTime;
        dt = Mathf.SmoothStep(0, 1, dt);
        return dt;
    }

    protected void OnValidate() {
        currentRadialStep = mod(currentRadialStep, radialSteps);
        if(currentAngularStep >= angularSteps)
            currentAngularStep = angularSteps-1;
        else if(currentAngularStep < 0)
            currentAngularStep = 0;
        SetTransform();
    }

    void SetTransform() {
        if(_transforming) {
            _transforming = false;
        }
        GetTransform(out Vector3 out_pos, out Quaternion out_rot);
        transform.position = out_pos;
        transform.rotation = out_rot;
    }

    public bool IsTransforming() {
        return _transforming;
    }
    public void SmoothTransform(int radial_delta, int angular_delta) {
        if(radial_delta == 0 && angular_delta == 0)
            return;
        if(_transforming) {
            float dt = get_dt();
            _transform_from_radial = Mathf.Lerp(_transform_from_radial, _transform_to_radial, dt);
            _transform_from_angular = Mathf.Lerp(_transform_from_angular, _transform_to_angular, dt);
        }
        else {
            _transforming = true;
            _transform_from_radial = GetRadialRadians(currentRadialStep);
            _transform_from_angular = GetAngularRadians(currentAngularStep);
        }
        currentRadialStep += radial_delta;
        currentAngularStep += angular_delta;
        if (currentAngularStep >= angularSteps)
            currentAngularStep = angularSteps - 1;
        else if (currentAngularStep < 0)
            currentAngularStep = 0;

        _transform_to_radial = GetRadialRadians(currentRadialStep);
        _transform_to_angular = GetAngularRadians(currentAngularStep);
        _transform_start = Time.time;
        if(Mathf.Abs(_transform_from_radial-_transform_to_radial) < 0.001 && Mathf.Abs(_transform_from_angular-_transform_to_angular) < 0.001)
            _transforming = false;
    }

    void SmoothTransformStep() {
        float dt = get_dt();
        if (dt >= 1) {
            SetTransform();
        }
        GetTransform(out Vector3 out_pos, out Quaternion out_rot, dt);
        transform.position = out_pos;
        transform.rotation = out_rot;
    }

    float GetRadialRadians(int r) {
        return (float)r / radialSteps * 2 * Mathf.PI;
    }
    float GetAngularRadians(int a) {
        return Mathf.Lerp(Mathf.Deg2Rad * minAngle, Mathf.Deg2Rad * maxAngle, ((float)a / (angularSteps - 1)));
    }

	void GetTransform(out Vector3 out_pos, out Quaternion out_rot, float dt=-1) {
        // Set position
        float radial; 
        float angular;
        if(dt < 0) {
            radial = GetRadialRadians(currentRadialStep);
            angular = GetAngularRadians(currentAngularStep);
        }
        else {
            radial = Mathf.Lerp(_transform_from_radial, _transform_to_radial, dt);
            angular = Mathf.Lerp(_transform_from_angular, _transform_to_angular, dt);
        }

        Vector3 pos = new Vector3(Mathf.Sin(radial), 0, Mathf.Cos(radial));
        Quaternion up_rot = Quaternion.FromToRotation(pos, Vector3.down); // Set rotation quat while flat pos
        pos = new Vector3(0, Mathf.Sin(angular), 0) + Mathf.Cos(angular) * pos;
        pos *= distance;
        out_pos = pos;
        // Set rotation
        out_rot = Quaternion.LookRotation(-pos.normalized, up_rot*-pos.normalized);
    }
}
