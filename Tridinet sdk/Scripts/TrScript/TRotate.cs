using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TRotate : MonoBehaviour
{
    public Vector3 axis;
    public float Angle;

    private void Update()
    {
        transform.Rotate(axis, Angle);
    }

    internal void setRot(Vector3 axis, float angle)
    {
        this.axis = axis;
        this.Angle = angle;
    }
}
