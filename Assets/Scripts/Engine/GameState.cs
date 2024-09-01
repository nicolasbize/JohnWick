using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameState {

    public static bool HasCompletedGame { get; set; } = false;
    public static int PlayerHealth { get; set; } = 27;
    public static int PlayerScore { get; set; } = 0;
    public static bool IsUsingTouchControls { get; set; } = true;// Application.isMobilePlatform;
}
