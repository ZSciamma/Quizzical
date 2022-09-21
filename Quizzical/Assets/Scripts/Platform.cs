using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public float baseSpeed; // Standard platform speed
    private float curSpeed; // Variable speed based on how far we're moving
    [Tooltip("Set so that platform always stays still, regardless of its number of pips")]
    public bool isStatic = false;

    private Vector3 target;
    private bool triggered;
    public bool isMoving { get { return triggered; } }

    
    private float[] levelHeights = new float[] { -1, 0, 1, 2};

    private int pips;
    public int Pips { get { return pips; } set { pips = value; }}
    private int level = 1;
    public int Level { get { return level; } 
        set { level = value % levelHeights.Length; }}
    private Color color;
    public Color Color { 
        set { 
            color = value;
            GetComponent<Renderer>().material.color = value;
        } }

    public void setStartLevel(int startLevel) {
        level = startLevel;
        transform.position = getLevelPosition(level);
    }

    void FixedUpdate() {
        if (triggered == true)
        {
            // Check if we've arrived
            float arriveDist = 0.09f;
            if ((target - transform.position).magnitude < arriveDist) {
                triggered = false;
                return;
            }

            // Move to next position
            var step = curSpeed * Time.fixedDeltaTime; // calculate distance to move
            Vector3 moveVector = (target - transform.position);
            GetComponent<Rigidbody>().MovePosition(transform.position + moveVector.normalized * step);  
        }
    }

    public void trigger() {
        if (isStatic == true) {
            return;
        }

        triggered = true;
        setMoveTarget();
    }

    private void setMoveTarget() {
        int oldLevel = level;
        level = (oldLevel + 1) % levelHeights.Length;
        target = getLevelPosition(level);

        // Increase speed if we're moving far
        curSpeed = baseSpeed * Mathf.Abs(level - oldLevel);
    }

    // Gets the position of the given level
    private Vector3 getLevelPosition(int level) {
        return new Vector3(
            transform.position.x,
            levelHeights[level],
            transform.position.z);
    }
}
