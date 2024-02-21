using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sgVehicleController : MonoBehaviour
{
    public Vector2 leftJoystick;
    public Vector2 rightJoystick;

    public HingeJoint[] wheelJoints;
    public JointMotor _jointMotor;
    public bool powerOn = false;
    public float maxVelocity = 10;

    public HingeJoint middleJoint;
    public JointSpring middleJointSpring;
    public float middleJointMoveSpeed = 1;

    public HingeJoint armJoint;
    public JointSpring armJointSpring;
    public float armJointMoveSpeed = 1;

    public HingeJoint scoopJoint;
    public JointSpring scoopJointSpring;
    public float scoopJointMoveSpeed = 1;

    void Update()
    {
        leftJoystick.x = Mathf.Round(Input.GetAxis("Horizontal L") * 10.0f) * 0.1f; 
        leftJoystick.y = Mathf.Round(Input.GetAxis("Vertical L") * 10.0f) * 0.1f;

        rightJoystick.x = Mathf.Round(Input.GetAxis("Horizontal R") * 10.0f) * 0.1f;
        rightJoystick.y = Mathf.Round(Input.GetAxis("Vertical R") * 10.0f) * 0.1f;

        // Current Scoop Joint Spring
        scoopJointSpring = scoopJoint.spring;

        if (rightJoystick.x < -0.2f || rightJoystick.x > 0.2f)
        {
            if (scoopJointSpring.targetPosition > scoopJoint.limits.min && scoopJointSpring.targetPosition < scoopJoint.limits.max)
            {
                scoopJointSpring.targetPosition += scoopJointMoveSpeed * rightJoystick.x * -1;

                if (scoopJointSpring.targetPosition < scoopJoint.limits.min) scoopJointSpring.targetPosition = scoopJoint.limits.min + 0.01f;
                if (scoopJointSpring.targetPosition > scoopJoint.limits.max) scoopJointSpring.targetPosition = scoopJoint.limits.max - 0.01f;

                scoopJoint.spring = scoopJointSpring;
            }
        };

        // Current Arm Joint Spring
        armJointSpring = armJoint.spring;

        if (rightJoystick.y < -0.2f || rightJoystick.y > 0.2f)
        {
            if (armJointSpring.targetPosition > armJoint.limits.min && armJointSpring.targetPosition < armJoint.limits.max)
            {
                armJointSpring.targetPosition += armJointMoveSpeed * rightJoystick.y * -1;

                if (armJointSpring.targetPosition < armJoint.limits.min) armJointSpring.targetPosition = armJoint.limits.min + 0.01f;
                if (armJointSpring.targetPosition > armJoint.limits.max) armJointSpring.targetPosition = armJoint.limits.max - 0.01f;

                armJoint.spring = armJointSpring;
            }
        };

        // Current Middle Joint Spring
        middleJointSpring = middleJoint.spring;

        if (leftJoystick.x < -0.2f || leftJoystick.x > 0.2f)
        {
            if (middleJointSpring.targetPosition > middleJoint.limits.min && middleJointSpring.targetPosition < middleJoint.limits.max)
            {
                middleJointSpring.targetPosition += middleJointMoveSpeed * leftJoystick.x;

                if (middleJointSpring.targetPosition < middleJoint.limits.min) middleJointSpring.targetPosition = middleJoint.limits.min + 0.01f;
                if (middleJointSpring.targetPosition > middleJoint.limits.max) middleJointSpring.targetPosition = middleJoint.limits.max - 0.01f;

                middleJoint.spring = middleJointSpring;
            }
        };

        // Wheels
        if (leftJoystick.y < -0.2f || leftJoystick.y > 0.2f)
        {
            if (wheelJoints[0].motor.targetVelocity < maxVelocity && wheelJoints[0].motor.targetVelocity > -maxVelocity) _jointMotor.targetVelocity += 1 * leftJoystick.y;

            if (wheelJoints[0].motor.targetVelocity < -maxVelocity) _jointMotor.targetVelocity = -maxVelocity + 0.01f;
            if (wheelJoints[0].motor.targetVelocity > maxVelocity) _jointMotor.targetVelocity = maxVelocity - 0.01f;

            powerOn = true;
        }
        else
        {
            powerOn = false;
        }


        if (powerOn) _jointMotor.force = 10;
        else
        {
            if (_jointMotor.targetVelocity > 0) _jointMotor.targetVelocity -= 0.1f;
            if (_jointMotor.targetVelocity < 0) _jointMotor.targetVelocity += 0.1f;

            _jointMotor.force = 0;
        }

//        foreach (HingeJoint _wheelJoint in wheelJoints) _wheelJoint.motor = _jointMotor;
    }
}
