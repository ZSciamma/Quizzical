using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    private PlatformList platforms;
    public PlatformList Platforms { 
        get { if (platforms == null) { createPlatforms(); } return platforms; }}
    [Tooltip("Number of pips on each platform (platforms numbered 0 to 10")]
    public int[] platformPips;
    [Tooltip("Starting level (0 to 3) for each platform (platforms numbered 0 to 10")]
    public int[] platformLevels;
    [Tooltip("Color for each pip. Starts at pip number 0. Any pips not specified will get a random color :)")]
    public List<Color> pipColors;
    [Tooltip("Texture for each pip. Starts at pip number 0 :)")]
    public List<Texture> pipTextures;
    Renderer m_Renderer;

    private Dictionary<int, Color> pipColorMap = new Dictionary<int, Color>();

    void Start()
    {

    }

    private void createPlatforms() {
        // Create 2D array of platforms
        Platform[] children = GetComponentsInChildren<Platform>();
        Platform[][] tempPlatforms = new Platform[5][];
        tempPlatforms[0] = new Platform[3] { null, children[0], null};
        tempPlatforms[1] = new Platform[3];
        tempPlatforms[2] = new Platform[3];
        tempPlatforms[3] = new Platform[3];
        tempPlatforms[4] = new Platform[3] {null, children[10], null};
        for (int row = 1; row < 4; row++) {
            tempPlatforms[row][0] = children[3*row-2];
            tempPlatforms[row][1] = children[3*row-1];
            tempPlatforms[row][2] = children[3*row];
        }
        platforms = new PlatformList(tempPlatforms);

        // Create pip color map
        for (int i = 0; i <= Mathf.Max(platformPips); i++) {
            // Set random color
            Color color = new Color(
                Random.Range(0.0f, 1.0f), 
                Random.Range(0.0f, 1.0f),
                Random.Range(0.0f, 1.0f),
                1);

            // Take color from inspector if specified
            if (i < pipColors.Count) {
                color = pipColors[i];
            }
            // Save  color
            color.a = 150;
            pipColorMap.Add(i, color);

        }

        ResetToStartConfig();
    }

    public void ResetToStartConfig() {
        // Set platform pips, heights and colors
        for (int i = 0; i < Platforms.Length; i++) {
            int pipNum = 0;
            int level = 0;
            if (i < platformPips.Length) {
                pipNum = platformPips[i];
            }
            if (i < platformLevels.Length) {
                level = platformLevels[i];
            }
            Platform platform = Platforms.get(i);
            platform.Pips = pipNum;
            platform.setStartLevel(level);
            platform.Color = pipColorMap[pipNum];

            if (0 < i && i < Platforms.Length-1)
            {
                m_Renderer = platform.GetComponent<Renderer>();
                m_Renderer.material.SetTexture("_BaseMap", pipTextures[pipNum - 1]);
            }
        }
    }

    /*
    public PlatformList getPlatforms() {
        if (platforms == null) {
            createPlatforms();
        }
        return platforms;
    }
    */
    // Return true if any platforms are moving right now
    public bool isInMotion() {
        for (int i = 0; i < Platforms.Length; i++) {
            if (Platforms.get(i).isMoving) {
                return true;
            }
        }
        return false;
    }

    // Trigger all platforms with this number of pips
    public void triggerPips(int pips) {
        for (int i = 0; i < Platforms.Length; i++) {
            Platform platform = Platforms.get(i);
            if (platform.Pips == pips) {
                // Trigger motion for this platform
                platform.trigger();
            }
        }
    }

    // Convert platform number to tuple
    public (int, int) convertIndexToTuple(int i) {
        return Platforms.convertIndexToTuple(i);
    }

    public Platform getPlatform((int, int) i) {
        return Platforms.get(i);
    }

    // Returns true if this is the end platform
    public bool isWinPlatform((int, int) i) {
        return i == (4, 1);
    }

    public void PlaySuccessSound() {
        GetComponent<AudioSource>().Play();
        return;
    }
}

// 2D list of platforms that can be accessed either by index
//  or tuple
public class PlatformList {
    private Platform[][] platformList;

    public int Length = 11;

    public PlatformList(Platform[][] list) {
        platformList = list;
    }

    public Platform get(int i) {
        (int, int) platI = convertIndexToTuple(i);
        return get(platI.Item1, platI.Item2);
    }
    
    public Platform get((int, int) i) {
        return get(i.Item1, i.Item2);
    }

    // Returns platform if it exists. Otherwise returns null
    //  Note that some nonexistent platforms are stored as null
    //  in the map.
    public Platform get(int r, int c) {
        // Check not off rectangular grid
        bool isOutsideBounds = r < 0 || r > 4 || c < 0 || c > 2;
        if (isOutsideBounds) {
            return null;
        }
        return platformList[r][c];
    }

    // Convert platform number (1-9) to (row, col)
    public (int, int) convertIndexToTuple(int i) {
        if (i == 0) {
            return (0, 1);
        } else if (i == 10) {
            return (4, 1);
        } else {
            int row = (i+2) / 3;
            int col = (i-1) % 3;
            return (row, col);
        }
    }

    /* : IEnumerable<Platform> 
    public IEnumerator<Platform> GetEnumerator() {
        return platformList.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
    */
}
