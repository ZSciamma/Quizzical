using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Direction { None, Left, Right, Forwards, Backwards }

public class PlayerController : MonoBehaviour
{
  public Grid grid;
  [Tooltip("Object containing the game over UI canvas, so that it can be shown/hidden")]
  public GameObject gameOverCanvas;
  [Tooltip("Defines how the player can move.\nUnrestrained: no rules.\nNoMoveUp: can move only down or the same level\nOnlyMoveSideways: can only move on the same level.")]
  public MoveMode motionRule;
  //[Tooltip("Number of seconds you have to wait between moves")]
  //public float moveCooldown;
  [Tooltip("Index of the platform (0 to 10) on which you start. 0 is start, and 10 is end.")]
  public int startPlatform;
  [Tooltip("True if you want teleportation, false for animations")]
  public bool shouldTeleport;
  [Tooltip("Seconds to wait between completing level and starting the next one")]
  public float levelCompletionTimer;
  
  public enum MoveMode { Unrestrained, NoMoveUp, OnlyMoveSideways}  

  private bool isCubeMoving; // True if the cube is physicall moving right now
  private (int, int) curPlatform = (2, 1);
  //private float lastMoveTime = -100;

  private bool levelComplete = false;
  private bool gameOver = false;

  private Vector3 curMoveTarget;

  private float levelEndTime; // Time level ended (either game over or success)

  void Start()
  {
    ResetToStartPos();
  }

  private void ResetToStartPos() {
    // Teleport cube to first platform
    curPlatform = grid.convertIndexToTuple(startPlatform);
    Platform firstPlatform = grid.getPlatform(curPlatform);
    Vector3 firstPos = getRestingPositionOnPlatform(firstPlatform);
    transform.position = firstPos;   

    // Hide GameOver button
    gameOver = false;
    gameOverCanvas.SetActive(false); 

    // Play sound effect
    playMoveSoundEffect();
  }

  void Update() 
  {
    if (!levelComplete && !gameOver) {
      checkForMovement();
      checkIfMoveJustFinished();
    } else {
      handleLevelEndedIfNecessary();
    }
  }

  void checkForMovement() {
    // Check we're not in the middle of a move
    bool cantMove = isCubeMoving || grid.isInMotion();
    if (cantMove) {
      return;
    }
    /*
    // Check if move cooldown elapsed
    float curTime = Time.time;
    if (curTime < lastMoveTime + moveCooldown) {
      return;
    }
    */

    // Find direction of movement
    Direction moveDir = getMoveDirection();
    if (moveDir == Direction.None) {
        return;
    }

    // Find next platform, it it's an allowed move
    (int, int)? nextIndex = findPlatformIndexInDirection(moveDir);
    bool moveAllowed = 
      nextIndex.HasValue &&
      isMoveToPlatformAllowed(nextIndex.Value);
    if (!moveAllowed) {
      return;
    }

    // Get location of next platform
    Platform prevPlatform = grid.getPlatform(curPlatform);
    curPlatform = nextIndex.Value;
    Platform nextPlatform = grid.getPlatform(curPlatform);
    curMoveTarget = getRestingPositionOnPlatform(nextPlatform);
    int levelDifference = prevPlatform.Level - nextPlatform.Level;

    // Start movement
    startMovement(moveDir, levelDifference);
  }

  // Starts movement to platform, setting up appropriate variables
  private void startMovement(Direction moveDir, int levelDifference) {
    // Trigger motion
    if (!shouldTeleport) {
      var rb = GetComponent<Rigidbody>();
      rb.isKinematic = true;
      GetComponent<CubeAnimation>().StartAnimation(moveDir, levelDifference);
    } else {
      transform.position = curMoveTarget;
    }

    // Register that motion is in progress
    isCubeMoving = true;
    //lastMoveTime = Time.time;
  }

  private Direction getMoveDirection() {
    Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    if (move.z > 0) {
      return Direction.Forwards;
    } else if (move.z < 0) {
      return Direction.Backwards;
    } else if (move.x > 0) {
      return Direction.Right;
    } else if (move.x < 0) {
      return Direction.Left;
    }
    return Direction.None;
  }

  // Get the platform index in the direction of movement
  private (int, int)? findPlatformIndexInDirection(Direction moveDir) {
    (int, int) nextIndex = curPlatform;
    switch (moveDir) {
      case Direction.Forwards:
        nextIndex = (curPlatform.Item1 + 1, curPlatform.Item2);
        break;
      case Direction.Backwards:
        nextIndex = (curPlatform.Item1 - 1, curPlatform.Item2 );
        break;
      case Direction.Left:
        nextIndex = (curPlatform.Item1, curPlatform.Item2 - 1);
        break;
      case Direction.Right:
        nextIndex = (curPlatform.Item1, curPlatform.Item2 + 1);
        break;
    }

    // Check the next platform exists
    if (grid.getPlatform(nextIndex) == null) {
      return null;
    }

    return nextIndex;
  }

  // Returns true if we're allowed to move to this platform
  //  Checks next platform is at an allowed height
  private bool isMoveToPlatformAllowed((int, int) nextPlat) {
    int curLevel = grid.getPlatform(curPlatform).Level;
    int nextLevel = grid.getPlatform(nextPlat).Level;
    switch (motionRule) {
      case MoveMode.Unrestrained:
        return true;
      case MoveMode.NoMoveUp:
        return curLevel >= nextLevel;
      case MoveMode.OnlyMoveSideways:
        return curLevel == nextLevel;
    }
    return true;
  }

  // Get the position of the cube if it were to rest on the platform
  private Vector3 getRestingPositionOnPlatform(Platform nextPlatform) {
    float height = (transform.localScale.y + grid.transform.localScale.y)/2;
    Vector3 position = nextPlatform.transform.position + new Vector3(0, height, 0);
    return position;
  }

  // Move towards target
  private void checkIfMoveJustFinished() {
    // Check if we're waiting for a move to finish
    if (!isCubeMoving) {
      return;
    }

    // Check if animation curve is done
    if (!shouldTeleport) {
      if (GetComponent<CubeAnimation>().AnimationRunning) {
        return; 
      }
      var rb = GetComponent<Rigidbody>();
      rb.isKinematic = false;
    }

    // Trigger platform
    int curPips = grid.getPlatform(curPlatform).Pips;
    grid.triggerPips(curPips);

    // End move
    isCubeMoving = false;

    // Play move sound effect
    playMoveSoundEffect();

    // Handle game over or win
    handlePossibleEndGames();

    // Play win sound effect if the level is complete
    if (levelComplete) {
      grid.PlaySuccessSound();
    }
  }

  // Play Plop sound
  private void playMoveSoundEffect() {
    GetComponent<AudioSource>().Play();
  }

  // Handle end conditions (e.g. game over, win)
  private void handlePossibleEndGames() {
    // Check for win
    if (grid.isWinPlatform(curPlatform)) {
      // Pause before finishing level
      levelComplete = true;
      levelEndTime = Time.time;
      return;
    }

    // Check for game over
    // Check if there's anywhere left to go. If not, enable reset
    Direction[] moveDirections = new Direction[]{
      Direction.Left, Direction.Right, Direction.Forwards, Direction.Backwards
    };

    // Check every possible move
    foreach (Direction dir in moveDirections) {
      // Check if this possible move would be allowed
      (int, int)? platform = findPlatformIndexInDirection(dir);
      if (platform.HasValue && isMoveToPlatformAllowed(platform.Value)) {
        return;
      }
    }

    // Set game over
    gameOver = true;
    levelEndTime = Time.time;
  }

  // Called by the GameOver button to reset the level
  public void ResetLevel() {
    ResetToStartPos();
    grid.ResetToStartConfig();
  }

  // Handle end of level if enough time has passed
  //  If succes, play next level
  //  If failure, offer reset button
  private void handleLevelEndedIfNecessary() {
    if (levelEndTime + levelCompletionTimer <= Time.time) {
      // Won level: start next level
      if (levelComplete) {
        SceneLoader loader = FindObjectOfType<SceneLoader>();
        loader.LoadNextScene();
        return;
      }

      // Lost level: offer reset
      if (gameOver) {
        gameOverCanvas.SetActive(true);
      }
    }
  }
}
