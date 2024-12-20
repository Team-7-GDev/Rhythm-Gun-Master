using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonStrafing : MonoBehaviour
{
    const string xAxis = "Mouse X"; //Strings in direct code generate garbage, storing and re-using them creates no garbage
    const string horizontal = "Horizontal";

    [Range(0.1f, 9f)][SerializeField] float m_RotSensitivity = 0.65f;
    [Range(1.0f, 10.0f)][SerializeField] float m_MoveSensitivity = 2.0f;
    [Range(0.0f, 3f)][SerializeField] float m_xMovementRange = 1f;


    private float xAccel = 0.0f;
    private float xVelocity = 0.0f;

    private float xPos = 0.0f;
    private float xRotation = 0.0f;


    // Update is called once per frame
    void Update()
    {
        if (GameManager.IsGameOver || GameManager.IsGamePause)
            return;

        xRotation += Input.GetAxis(xAxis) * m_RotSensitivity;
        /* xAccel += Input.GetAxis(horizontal) * Time.deltaTime;
        xVelocity += xAccel * Time.deltaTime; */

        var xQuat = Quaternion.AngleAxis(xRotation, Vector3.up);
        var position = transform.position + Vector3.right * Input.GetAxis(horizontal) * Time.deltaTime * m_MoveSensitivity;

        /* xPos += xVelocity * Time.deltaTime;
        xPos = Mathf.Clamp(xPos, -m_xMovementRange, m_xMovementRange);
        position.x = xPos; */
        position.x = Mathf.Clamp(position.x, -m_xMovementRange, m_xMovementRange);

        transform.rotation = xQuat;
        transform.position = position;
    }
}
