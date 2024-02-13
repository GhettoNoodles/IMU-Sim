using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class orbitCamera : MonoBehaviour
{
    public Transform player;

    public float turnSpeed = 4.0f;
    private float height = 5f;
    public float distance = 7f;

    private Vector3 offsetX;
    private Vector3 offsetY;

    void Start()
    {
        offsetX = new Vector3(0, height, distance * -4);
        offsetY = new Vector3(0, 0, distance);
    }

    void LateUpdate()
    {
        offsetX = Quaternion.AngleAxis(-Input.GetAxis("Xbox_One_DPad_Horizontal") * turnSpeed, Vector3.up) * offsetX;
        offsetY = Quaternion.AngleAxis(Input.GetAxis("Xbox_One_DPad_Vertical") * turnSpeed, Vector3.right) * offsetY;
        transform.position = player.position + offsetX + offsetY;
        transform.LookAt(player.position);
    }
}
