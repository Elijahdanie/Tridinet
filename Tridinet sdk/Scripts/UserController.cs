using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A basic script to moving the user around the world
/// </summary>
public class UserController : MonoBehaviour
{
    public float speed;

    public Vector3 camOffset;

    public Animator anim;
    public float rotationSpeed;
    public float slerptime;

    public bool walk;

    private void Update()
    {
        var inputX = Input.GetAxis("Horizontal");
        var InputY = Input.GetAxis("Vertical");
        var mouseX = Input.GetAxis("Mouse X");
        anim.SetFloat("X", inputX);
        anim.SetFloat("Y", InputY);
        anim.SetBool("walk", InputY > 0 ? true : false);
        var moveVector = new Vector2(0, InputY) * speed * Time.deltaTime;
        transform.Translate(transform.InverseTransformDirection(transform.forward * speed * Time.deltaTime * InputY));
        transform.eulerAngles += mouseX * Vector3.up * rotationSpeed * Time.deltaTime;
        var pos = transform.position + (transform.forward * camOffset.z);
        Camera.main.transform.position = pos + Vector3.up * camOffset.y;
        var rot = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        Camera.main.transform.rotation = Quaternion.Euler(0, rot.eulerAngles.y, 0);
    }
}
