using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    // DIR ENUM
    public enum Dir { Up, Forward, Right, Down, Back, Left }
    public static Dir GetDir(Vector3 v) {
        v = v.normalized;
        if(Mathf.Abs(v.y) >= Mathf.Abs(v.z)) {
            if(Mathf.Abs(v.y) >= Mathf.Abs(v.x))
                return v.y > 0 ? Dir.Up : Dir.Down;
            else
                return v.x > 0 ? Dir.Right : Dir.Left;
        }
        else if(Mathf.Abs(v.z) >= Mathf.Abs(v.x)) {
            return v.z > 0 ? Dir.Forward : Dir.Back;
        }
        else {
            return v.x > 0 ? Dir.Right : Dir.Left;
        }
    }

    // Public Fields
    public float defaultTargetHeight = 0.1f;
    public Dir defaultDirection = Dir.Up;

    // Public properties
    float _target_height;
    public float TargetHeight {
        get {
            return _target_height;
        }
        set {
            _target_height = value;
            SetTargetTransform();
        }
    }
    Dir _direction;
    public Dir Direction {
        get {
            return _direction;
        }
        set {
            _direction = value;
            SetTargetTransform();
        }
    }

    // Linking vars
    SpriteRenderer sr;
    Transform targetTransform;

    // Readonly statics
    static readonly float base_height = 0.5f;

    private void Awake () {
        sr = GetComponentInChildren<SpriteRenderer>();
        if(!sr)
            Debug.LogWarning("Target: SpriteRenderer not found!");
    }

    private void Start () {
        if(transform.childCount == 0)
            Debug.LogWarning("Target missing Target Sprite Child");
        else
            targetTransform = transform.GetChild(0);
        _target_height = defaultTargetHeight;
        _direction = defaultDirection;
        SetTargetTransform();
    }
    private void OnValidate () {
        TargetHeight = defaultTargetHeight;
        Direction = defaultDirection;
    }

    public void SetTargetTransform() {
        if(targetTransform)
            targetTransform.localPosition = new Vector3(0,base_height+_target_height,0);
        Quaternion rot = Quaternion.identity;
        switch (_direction) {
            case Dir.Up: rot = Quaternion.identity; break;
            case Dir.Forward: rot = Quaternion.Euler(90,0,0); break;
            case Dir.Left: rot = Quaternion.Euler(0, 0, 90); break;
            case Dir.Down: rot = Quaternion.Euler(180,0,0); break;
            case Dir.Back: rot = Quaternion.Euler(-90, 0, 0); break;
            case Dir.Right: rot = Quaternion.Euler(0, 0, -90); break;
        }
        transform.rotation = rot;
    }

    public void SetColor(Color c, float alpha=-1) {
        if(sr)
            sr.color = alpha >= 0 ? new Color(c.r, c.g, c.b, alpha) : c;
    }
}
