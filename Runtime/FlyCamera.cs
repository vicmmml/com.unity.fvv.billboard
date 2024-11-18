using UnityEngine;
using System.Collections;


public class FlyCamera : MonoBehaviour {

    /*
    Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
    Converted to C# 27-02-13 - no credit wanted.
    Simple flycam I made, since I couldn't find any others made public.  
    Made simple to use (drag and drop, done) for regular keyboard layout  
    wasd : basic movement
    shift : Makes camera accelerate
    space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/
    
    
    float mainSpeed = 25f; //regular speed  30 y p base 0.08 nice
    float shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
    float maxShift = 1000.0f; //Maximum speed when holdin gshift
    float camSens = 0.30f; //How sensitive it with mouse
    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun= 1.0f;
    public bool IsMoving = false;
    private bool looking = false;
    
    void Update () {

        if ((lastMouse - Input.mousePosition) != new Vector3(0, 0, 0)) IsMoving = true;

        if (looking){
            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * camSens;
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * camSens;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
            lastMouse =  Input.mousePosition;

        //Mouse  camera angle done. 
        }
        
    
        //Keyboard commands
        //float f = 0.0f;
        Vector3 p = GetBaseInput();
        if (p.sqrMagnitude > 0){ // only move while a direction key is pressed
        IsMoving = true;
        if (Input.GetKey (KeyCode.LeftShift)){
            totalRun += Time.deltaTime;
            p  = p * totalRun * shiftAdd;
            p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
            p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
            p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
        } else {
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
            p = p * mainSpeed;
        }
        } else 
            IsMoving = false;


        if (Input.GetKeyDown(KeyCode.Mouse1)) //While right mouse clic pressed, the cursor is locked in the center of the view and won't be visible 
        {
            StartLooking();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1)) //While right mouse clic pressed, the cursor will be visible and won´t be locked
        {
            StopLooking();
        }


        p = p * Time.deltaTime;
        Vector3 newPosition = transform.position;

        if (Input.GetKey(KeyCode.Space)){ //If player wants to move on X and Z axis only
            transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        } else {
            transform.Translate(p);
        }
        
    }
    

    private void StartLooking(){
        looking = true;
        //Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void StopLooking(){
        looking = false;
        //Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    
    private Vector3 GetBaseInput() { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey (KeyCode.W)){
            p_Velocity += new Vector3(0, 0 , 0.08f);
        }
        if (Input.GetKey (KeyCode.S)){
            p_Velocity += new Vector3(0, 0, -0.08f);
        }
        if (Input.GetKey (KeyCode.A)){
            p_Velocity += new Vector3(-0.08f, 0, 0);
        }
        if (Input.GetKey (KeyCode.D)){
            p_Velocity += new Vector3(0.08f, 0, 0);
        }
        return p_Velocity;
    }
}
