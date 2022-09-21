using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CubeAnimation : MonoBehaviour
{
    [Tooltip("Speed of move animation")]
    public float speed;

    private bool animationRunning;
    public bool AnimationRunning { get { return animationRunning; } }
    private float curveDeltaTime;
    private Vector3 moveHorizontalVector; // Normalized translation
    private Vector3 moveVerticalVector;
    private Vector3 rotateVector; // Normalized rotation
    private Vector3 startPosition;
    private Vector3 startRotation;

    // Store end times
    private float horizontalEndTime;
    private float verticalEndTime;
    private float rotationEndTime;
    private float overallEndTime;

    // Curves that move individual parameters of the cube
    public AnimationCurve level0MoveHorizontalCurve;
    public AnimationCurve level0MoveVerticalCurve;
    public AnimationCurve level0RotateCurve;
    public AnimationCurve level1MoveVerticalCurve;
    public AnimationCurve level2MoveVerticalCurve;
    public AnimationCurve level3MoveVerticalCurve;

    // Important: we assume the curves end at the same time
    [Tooltip("y units per level")]
    public float heightPerLevel;
    public float dropPerSec; // Y decrease per second during fall
    [Tooltip("Distance between platform centers")]
    public float platformDistance;

    private AnimationCurve curMoveHorizontalCurve;
    private AnimationCurve curMoveVerticalCurve;
    private AnimationCurve curRotateCurve;

    // Upon movement, start the animation
    public void StartAnimation(Direction dir, int levelsDropped) {
        // Start animation
        animationRunning = true;
        curveDeltaTime = 0.0f;
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
    
        // Set current animation curves;
        curMoveHorizontalCurve = level0MoveHorizontalCurve;
        curRotateCurve = level0RotateCurve;
        switch (levelsDropped) {
            case 0:
                curMoveVerticalCurve = level0MoveVerticalCurve;
                break;
            case 1:
                curMoveVerticalCurve = level1MoveVerticalCurve;
                break;
            case 2:
                curMoveVerticalCurve = level2MoveVerticalCurve;
                break;
            case 3:
                curMoveVerticalCurve = level3MoveVerticalCurve;
                break;
            default:
                Debug.Log("No curve set for level difference");
                break;
        }

        // Set direction and drop
        Vector3 moveDir = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 rotateDir = new Vector3(0.0f, 0.0f, 0.0f);
        switch (dir) {
            case Direction.Forwards:
                moveDir = new Vector3(0.0f, 0.0f, 1.0f);
                rotateDir = new Vector3(1.0f, 0.0f, 0.0f); 
                break;
            case Direction.Backwards:
                moveDir = new Vector3(0.0f, 0.0f, -1.0f);
                rotateDir = new Vector3(-1.0f, 0.0f, 0.0f);
                break;
            case Direction.Right:
                moveDir = new Vector3(1.0f, 0.0f, 0.0f);
                rotateDir = new Vector3(0.0f, 0.0f, -1.0f);
                break;           
            case Direction.Left:
                moveDir = new Vector3(-1.0f, 0.0f, 0.0f);
                rotateDir = new Vector3(0.0f, 0.0f, 1.0f);
                break;
        }
        moveHorizontalVector = moveDir * platformDistance;
        moveVerticalVector = new Vector3(0.0f, -1.0f, 0.0f);
        rotateVector = rotateDir;

        // Store end times
        Keyframe[] horizontalKeys = curMoveHorizontalCurve.keys;
        Keyframe[] verticalKeys = curMoveVerticalCurve.keys;
        Keyframe[] rotationKeys = curRotateCurve.keys;

        horizontalEndTime = horizontalKeys[horizontalKeys.Length - 1].time;
        verticalEndTime = verticalKeys[verticalKeys.Length - 1].time;
        rotationEndTime = rotationKeys[rotationKeys.Length - 1].time;
        
        overallEndTime = Mathf.Max(horizontalEndTime, verticalEndTime, rotationEndTime);
    }

    // Update the animation
    void FixedUpdate()
    {
        // Check if animation is finished
        if (!animationRunning) {
            return;
        } else if (curveDeltaTime > overallEndTime) {
            animationRunning = false;
            return;
        }

        // Find rotation and translation
        curveDeltaTime += Time.deltaTime * speed;
        float moveHorizontalCurveVal = curMoveHorizontalCurve.Evaluate(curveDeltaTime);
        float moveVerticalCurveVal = curMoveVerticalCurve.Evaluate(curveDeltaTime);
        float rotateCurveVal = curRotateCurve.Evaluate(curveDeltaTime);
        //float rotation = rotationCurve.Evaluate(curveDeltaTime);

        // Apply rotation and translation
        if (curveDeltaTime <= rotationEndTime) {
            Vector3 curAnimationRotation = rotateVector * rotateCurveVal;
            transform.eulerAngles = startRotation;
            transform.RotateAround(startPosition, rotateVector, rotateCurveVal * 90);
        }

        // Apply translation after rotation (because rotation
        //  is applied globally)
        if (curveDeltaTime <= horizontalEndTime) {
            Vector3 curAnimationPositionHorizontal = moveHorizontalVector * moveHorizontalCurveVal;
            transform.position = startPosition + curAnimationPositionHorizontal;
        }

        if (curveDeltaTime <= verticalEndTime) {
            float curAnimationPositionY = moveVerticalVector.y * moveVerticalCurveVal;
            transform.position = new Vector3(
                transform.position.x,
                startPosition.y + curAnimationPositionY,
                transform.position.z
            );
        }
    }
}
