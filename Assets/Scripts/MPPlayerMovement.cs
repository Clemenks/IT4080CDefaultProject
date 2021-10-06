using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class MPPlayerMovement : NetworkBehaviour
{
    public float movementSpeed = 3f;
    public float rotationSpeed = 100f;
    public Transform camT;
    CharacterController mpCharController;

    // Start is called before the first frame update
    void Start()
    {
        mpCharController = GetComponent<CharacterController>();
        //Color Changing
        if(IsOwner)
        {
            GetComponent<MeshRenderer>().material.color = new Color(1, 0, 1, 1);
        }
        else
        {
            GetComponent<MeshRenderer>().material.color = Color.red;
            //Disable Other Cameras
            camT.GetComponent<Camera>().enabled = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        { 
            MPMovePlayer();
        }
    }

    void MPMovePlayer()
    {
        transform.Rotate(0, Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime, 0);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        mpCharController.SimpleMove(forward * movementSpeed * Input.GetAxis("Vertical"));

    }
}
