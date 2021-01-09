using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public enum Player
{
    None, Red, Blue
}

public enum Direction
{
    Left , Right ,
    Up , Down ,
    NE , SW ,
    NW , SE ,
    Count
}
public enum Axis
{
    Horizontal,
    Vertical,
    DiagonalLeftToRight,
    DiagonalRightToLeft,
    Count
}

public struct DirectionStreak
{
    public int reds;
    public int blues;

    public int redsMax;
    public int bluesMax;

    public bool redsSealed;
    public bool bluesSealed;

    public bool longRed;
    public bool longBlue;
    // public bool emptyNear;



    public DirectionStreak(int reds = 0, int blues =0 , int redsMax = 0, int bluesMax = 0 , bool redsBlocked = false, bool bluesBlocked = false, bool longRed = false, bool longBlue = false)
    {
        this.reds = reds;
        this.blues = blues;
        this.redsMax = redsMax;
        this.bluesMax = bluesMax;
        this.redsSealed = redsBlocked;
        this.bluesSealed = bluesBlocked;
        this.longRed = longRed;
        this.longBlue = longBlue;
    }
}

public class GameHandler : MonoBehaviour
{

    public TileBuilder tileBuilder;
    public List<Tile> tiles;
    private List<Tile> unplayedTiles;
    public Player currentPlayer = Player.Red;
    public bool gameEnded = false;
    public bool gameStarted = false;
    public bool canPlay = true;
    public bool paused = false;

    public GameObject gamecamera;
    public Transform cameraDestentation;
    public float moveSpeed = 2;

    public int attackScoreModifier = 1;
    public int defenceScoreModifier = 2;
    public int emptyScoreModifier = -1;

    public float ballDropSpeed = 0.2f;
    public bool debug = true;
    public bool fullDebug = true;

    public GameObject hideWinnerTextButton;
    public GameObject unhideWinnerTextButton;
    public GameObject gameEndedCarrier;
    public TextMeshProUGUI gameEndingText;
    public TextMeshProUGUI playsCounter;
    private int plays = 0;
    private static GameHandler instance;


    public string leaveConfirmText = "Are you sure you want to Exit?";
    public string restartConfirmText = "Are you sure you want to Restart?";
    public TextMeshProUGUI confirmationText;
    public TextMeshProUGUI confirmationButtonText;
    public Button confirmButton;
    public GameObject confirmMenuWindow;
    public GameObject[] exitAndRestartButtons;

    public static GameHandler Instance
    {
        get
        {
            return instance;
        }
    }


    private void Awake()
    {
        if (instance == null) instance = this;
    }
    private void Start()
    {
        tiles = tileBuilder.CreateGrid(tiles);
        unplayedTiles = new List<Tile>(tiles);

        if(gamecamera != null && cameraDestentation != null)
        {
            gamecamera.transform.LeanMove(cameraDestentation.position, moveSpeed);
            gamecamera.transform.LeanRotate(cameraDestentation.eulerAngles, moveSpeed).setOnComplete(()=> { gameStarted = true; });
        }
        else
        {
            gameStarted = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitPressed();
        }
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Backspace))
        {
            RestartPressed();
        }
        plays = (tileBuilder.boardSizeX * tileBuilder.boardSizeZ - unplayedTiles.Count);
        playsCounter.text = "Plays: " + plays ;
    }

    #region UI Buttons

    public void OnConfirmRestartPressed()
    {
        //SceneManager.LoadScene(0);
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].Owner = Player.None;
            tiles[i].winningTile = false;
            tiles[i].streakRed = 0;
            tiles[i].streakBlue = 0;
        }

        gameEnded = false;
        gameEndingText.text = "";
        plays = 0;
        currentPlayer = Player.Red;
        hideWinnerTextButton.SetActive(false);
        unhideWinnerTextButton.SetActive(false);
        confirmMenuWindow.SetActive(false);
        paused = false;
        canPlay = true;
        gameEndedCarrier.SetActive(false);
        unplayedTiles = new List<Tile>(tiles);
        for (int i = 0; i < exitAndRestartButtons.Length; i++)
        {
            exitAndRestartButtons[i].SetActive(true);
        }

        StopAllCoroutines();
    }

    public void OnConfirmExitPressed()
    {
        Application.Quit();
    }
    public void RestartPressed()
    {
        if(gameEnded)
        {
            OnConfirmRestartPressed();
            return;
        }
        confirmMenuWindow.SetActive(true);
        confirmationText.text = restartConfirmText;
        confirmationButtonText.text = "Restart";
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmRestartPressed);
        paused = true;
        gameEndedCarrier.SetActive(false);
    }

    public void ExitPressed()
    {
        confirmMenuWindow.SetActive(true);
        confirmationText.text = leaveConfirmText ;
        confirmationButtonText.text = "Exit";
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmExitPressed);
        paused = true;
        gameEndedCarrier.SetActive(false);
    }

    public void OnCancelPressed()
    {
        confirmMenuWindow.SetActive(false);
        paused = false;

        gameEndedCarrier.SetActive(gameEnded);
    }

    #endregion

    public void PlayerPlayed(Tile playedTile) 
    {
        //to do
        //check if a player won
        // if won -- END
        // Next
        //read all tiles, find what is the next tile to play for AI
        //check if AI wins

        unplayedTiles.Remove(playedTile);

        canPlay = false;
        StartCoroutine(WaitThenDo(0.5f, () => { canPlay = true; }));

        //plays++; // check how many plays to know if all tiles played

        

        //Check if player wins
        for (int i = 0; i < tiles.Count; i++)
        {
            if(tiles[i].Owner != Player.None && CheckWinOnTile(tiles[i]))
            {
                //the tile wins
                int winner = -1;
                if(tiles[i].Owner == Player.Red)
                {
                    winner = 1;
                }
                else if(tiles[i].Owner == Player.Blue)
                {
                    winner = 2;
                }
                GameEnd(winner);
                return; // Leave
            }
        }

        if (plays >= tileBuilder.boardSizeX * tileBuilder.boardSizeZ)
        {
            //No More Plays
            GameEnd(-1);
            return;
        }

        currentPlayer = currentPlayer == Player.Blue ? Player.Red : Player.Blue;

        if(currentPlayer == Player.Blue) //AI
        {
            //for now play any tile
            AIPlay();
        }
    }

    private void GameEnd(int playerWon)
    {
        if (playerWon == 1)
        {
            Debug.Log("Red Player Won");
            gameEndingText.text = "Red Player Won";
            gameEndingText.color = Color.red;
        }
        else if (playerWon == 2) 
        {
            Debug.Log("AI/Blue Player Won");
            gameEndingText.text = "AI/Blue Player Won";
            gameEndingText.color = Color.blue;
        }
        else
        {
            Debug.Log("TIE");
            gameEndingText.text = "TIE";
            gameEndingText.color = Color.gray;
        }
        gameEndedCarrier.SetActive(true);
        hideWinnerTextButton.SetActive(true);
        gameEnded = true;
    }

    public bool CheckWinOnTile(Tile tile)
    {
        int counts = 0;

        //check horizontally left and right
        //check vertically top and bottom
        //check diagonally 4 directions

        //Checking Horizontally
        //Left
        if (tile.x >= 4)
        {
            counts = 0;
            List<Tile> leftTiles = tiles.FindAll((x => x.z == tile.z && x.x < tile.x));
            if(leftTiles.Count >= 4)
            {
                for (int i = 0; i < leftTiles.Count; i++)
                {
                    if(leftTiles[i].x > tile.x - 5 && leftTiles[i].x < tile.x)
                    {
                        if (leftTiles[i].Owner == tile.Owner) counts++;
                    }
                }
                if (counts >= 4)
                {
                    Debug.Log("Won on Left count = " + counts);
                    return true;
                }
            }
        }
        //Right
        if (tile.x < tileBuilder.boardSizeX - 5)
        {

            counts = 0;
            List<Tile> rightTiles = tiles.FindAll((x => x.z == tile.z && x.x > tile.x));
            if (rightTiles.Count >= 4)
            {
                for (int i = 0; i < rightTiles.Count; i++)
                {
                    if (rightTiles[i].x < tile.x + 5 && rightTiles[i].x > tile.x)
                    {
                        if (rightTiles[i].Owner == tile.Owner) counts++;
                    }
                }
                if (counts >= 4)
                {
                    Debug.Log("Won on Right count = " + counts);
                    return true;
                }
            }
        }
        //Checking Vertically
        //Top
        if (tile.z >= 4)
        {
            counts = 0;
            List<Tile> upTiles = tiles.FindAll((x => x.x == tile.x && x.z < tile.z));
            if (upTiles.Count >= 4)
            {
                for (int i = 0; i < upTiles.Count; i++)
                {
                    if (upTiles[i].z > tile.z - 5 && upTiles[i].z < tile.z)
                    {
                        if (upTiles[i].Owner == tile.Owner) counts++;
                    }
                }
                if (counts >= 4)
                {
                    Debug.Log("Won Up on count = " + counts);
                    return true;
                }
            }
        }
        //Bot
        if (tile.z < tileBuilder.boardSizeZ - 5)
        {

            counts = 0;
            List<Tile> downTiles = tiles.FindAll((x => x.x == tile.x && x.z > tile.z));
            if (downTiles.Count >= 4)
            {
                for (int i = 0; i < downTiles.Count; i++)
                {
                    if (downTiles[i].z < tile.z + 5 && downTiles[i].z > tile.z)
                    {
                        if (downTiles[i].Owner == tile.Owner) counts++;
                    }
                }
                if (counts >= 4)
                {
                    Debug.Log("Won on Bot count = " + counts);
                    return true;
                }
            }
        }
        //Checking Diagonally 
        //North-West  < ^
        if (tile.x >= 4 && tile.z >= 4)
        {
            counts = 0;
            List<Tile> northWestTiles = new List<Tile>();
            Tile temp;
            for (int i = 1; i < 5; i++)
            {
                temp = tiles.Find(t => (t.x == tile.x - i && t.z == tile.z - i && t.x != tile.x && t.z != tile.z));
                if (temp != null) northWestTiles.Add(temp);
                temp = null;
            }

            if (northWestTiles.Count >= 4)
            {
                for (int i = 0; i < northWestTiles.Count; i++)
                {
                    if (northWestTiles[i].z > tile.z - 5 && northWestTiles[i].x > tile.x - 5 && northWestTiles[i].z < tile.z && northWestTiles[i].x < tile.x)
                    {
                        if (northWestTiles[i].Owner == tile.Owner) counts++;
                    }
                }
                if (counts >= 4)
                {
                    Debug.Log("Won North-West on count = " + counts);
                    return true;
                }
            }
        }
        //North-East
        if (tile.x < tileBuilder.boardSizeX - 5 && tile.z >= 4)
        {
            counts = 0;
            List<Tile> northEastTiles = new List<Tile>();// tiles.FindAll(t => (t.z < tile.z && t.x > tile.x && t.z != tile.z && t.x != tile.x));
            Tile temp;
            for (int i = 1; i < 5; i++)
            {
                temp = tiles.Find(t => (t.x == tile.x + i && t.z == tile.z - i && t.x != tile.x && t.z != tile.z));
                if (temp != null) northEastTiles.Add(temp);
                temp = null;
            }
           // Debug.Log("NE count " + northEastTiles.Count);

            if (northEastTiles.Count >= 4)
            {
                for (int i = 0; i < northEastTiles.Count; i++)
                {
                    if (northEastTiles[i].z > tile.z - 5 && northEastTiles[i].x < tile.x + 5 && northEastTiles[i].z < tile.z && northEastTiles[i].x > tile.x)
                    {
                        if (northEastTiles[i].Owner == tile.Owner) counts++;
                    }
                }
                if (counts >= 4)
                {
                    Debug.Log("Won North-East on count = " + counts);
                    return true;
                }
            }
        }
        //South-East
        if (tile.x < tileBuilder.boardSizeX - 5 && tile.z < tileBuilder.boardSizeZ - 5)
        {
            counts = 0;

            List<Tile> southEastTiles = new List<Tile>();//tiles.FindAll(t => (t.z > tile.z && t.x > tile.x && t.z != tile.z && t.x != tile.x));

            Tile temp;
            for (int i = 1; i < 5; i++)
            {
                temp = tiles.Find(t => (t.x == tile.x + i && t.z == tile.z + i && t.x != tile.x && t.z != tile.z));
                if (temp != null) southEastTiles.Add(temp);
                temp = null;
            }
          //  Debug.Log("SE count "+southEastTiles.Count);
            if (southEastTiles.Count >= 4)
            {
                for (int i = 0; i < southEastTiles.Count; i++)
                {
                    if (southEastTiles[i].z < tile.z + 5 && southEastTiles[i].x < tile.x + 5 && southEastTiles[i].z > tile.z && southEastTiles[i].x > tile.x)
                    {
                        if (southEastTiles[i].Owner == tile.Owner) counts++;
                    }
                }
                if (counts >= 4)
                {
                    Debug.Log("Won South-East on count = " + counts);
                    return true;
                }
            }
        }
        //South-West
        if (tile.x >= 4 && tile.z < tileBuilder.boardSizeZ - 5)
        {
            counts = 0;
            List<Tile> southWestTiles = new List<Tile>();//tiles.FindAll(t => (t.z > tile.z && t.x < tile.x && t.z != tile.z && t.x != tile.x));
            Tile temp;
            for (int i = 1; i < 5; i++)
            {
                temp = tiles.Find(t => (t.x == tile.x - i && t.z == tile.z + i && t.x != tile.x && t.z != tile.z));
                if (temp != null) southWestTiles.Add(temp);
                temp = null;
            }
           // Debug.Log("SW count " + southWestTiles.Count);

            if (southWestTiles.Count >= 4)
            {
                for (int i = 0; i < southWestTiles.Count; i++)
                {
                    if (southWestTiles[i].z < tile.z + 5 && southWestTiles[i].x > tile.x - 5 && southWestTiles[i].z > tile.z && southWestTiles[i].x < tile.x)
                    {
                        if (southWestTiles[i].Owner == tile.Owner) counts++;
                    }
                }
                if (counts >= 4)
                {
                    Debug.Log("Won South-West on count = " + counts);
                    return true;
                }
            }
        }


        return false;
    }
    private void AIPlay()
    {

        Tile tileToPlay = GetBestTile();


        if (tileToPlay != null) 
        {
            StartCoroutine(WaitThenDo(UnityEngine.Random.Range(0.5f, 0.7f), () => 
            {
                tileToPlay.PlayTile(currentPlayer);
                DebugTile(tileToPlay);
            }));
            

            //Debug see neighbours .
        }
        else
        {
            Debug.LogError("No Tiles found from AI search");
        }

    }

    private void ScoreTiles()
    {
        //check every empty tile for possible plays in each direction and give score accordingly
        //Tile with best score is returned
        Tile tile;
        int totalBlues;
        int totalReds;
        int numberOfStreaksAboveTwoPerSideReds;
        int numberOfStreaksAboveTwoPerSideBlues;

        //Set Empty Tiles Scores
        for (int i = 0; i < unplayedTiles.Count; i++)
        {

            if (unplayedTiles[i].Owner != Player.None) continue; //should never pass

            tile = unplayedTiles[i];
            tile.mustDefendTile = false;
            tile.attackScore = 0;
            tile.defenceScore = 0;
            tile.finalScore = 0;
            tile.streakBlue = -1; // debug
            tile.streakRed = -1;
            tile.maxStreakRed = 100; // debug
            tile.maxStreakBlue = 100; // debug
            tile.winningTile = false;

         

            Axis[] allAxis = { Axis.Horizontal, Axis.Vertical, Axis.DiagonalLeftToRight, Axis.DiagonalRightToLeft };
            DirectionStreak streakData;
            numberOfStreaksAboveTwoPerSideReds = 0;
            numberOfStreaksAboveTwoPerSideBlues = 0;


            for (int j = 0; j < allAxis.Length; j++)
            {
                streakData = tile.GetStreakInAxis(allAxis[j]);

                // totalBlues and totalReds are the streaks of blues and reds in each direction (will not count a streak if the colors are not connected)
                totalBlues = streakData.blues;
                totalReds = streakData.reds;
                numberOfStreaksAboveTwoPerSideReds = streakData.longRed ? numberOfStreaksAboveTwoPerSideReds + 1 : numberOfStreaksAboveTwoPerSideReds;
                numberOfStreaksAboveTwoPerSideBlues = streakData.longBlue ? numberOfStreaksAboveTwoPerSideBlues + 1 : numberOfStreaksAboveTwoPerSideBlues;

                if (totalBlues >= 4) // Found Win no need to continue
                {
                    tile.winningTile = true; // This will make the tile picked anyway
                    Debug.Log("Found Winning Tile");
                    return; //leave the scoring
                }


                tile.streakBlue = totalBlues >= tile.streakBlue ? totalBlues : tile.streakBlue;//debuging
                tile.streakRed = totalReds >= tile.streakRed ? totalReds : tile.streakRed;

                int scoreDef = totalReds * defenceScoreModifier;
                int scoreAtt = totalBlues * attackScoreModifier;


                //This ignores the scores
                if (totalReds >= 4 && streakData.redsMax >= 4) // if its 4 we dont care about sealed or not, it must be stopped (Second most important tile after A winning tile)
                {
                    tile.mustDefendTile = true;
                }
                else
                if (totalReds >= 3 && streakData.redsMax >= 4 && streakData.redsSealed == false) //must catch a 3 before player makes 4 // if streak max is less that 4 then its useless to def // also if blocked is worth less
                {
                    //a must def tile
                    //will make it ignore the modifiers
                    tile.mustDefendTile = true;
                }
                else if((totalReds >= 3 && streakData.redsMax < 4)) // debugging
                {
                    Debug.Log("Not scary");
                }

      
                // if a almost win or player played a 3, then score is multiplied by 100
                if (totalReds == 3 && streakData.redsMax >= 4 && streakData.redsSealed == false) scoreDef *= 100;
                else if(totalReds == 3 && streakData.redsMax >= 4 && streakData.redsSealed == true) scoreDef *= 50;

                if (totalBlues == 3 && streakData.bluesMax >= 4 && streakData.bluesSealed == false) scoreAtt *= 100;
                else if(totalBlues == 3 && streakData.bluesMax >= 4 && streakData.bluesSealed == true) scoreDef *= 50; // if we have e r r r b will be worth less than e r r r e  (e = empty)

  

                tile.attackScore = scoreAtt >= tile.attackScore ? scoreAtt : tile.attackScore;
                tile.defenceScore = scoreDef >= tile.defenceScore ? scoreDef : tile.defenceScore;

                
            }

            //Extra points for tiles that are inside different streaks i.e. has 2 up 1 down and 1 left and 2 right , will have 2 a 3-streak so it should be better than a tile that has only 1 3-streak in one direction. 
            tile.attackScore += (numberOfStreaksAboveTwoPerSideBlues * 10);
            tile.defenceScore += (numberOfStreaksAboveTwoPerSideReds * 10);

            int newFinalScore = tile.attackScore >= tile.defenceScore ? tile.attackScore : tile.defenceScore;
            tile.finalScore = newFinalScore >= tile.finalScore ? newFinalScore : tile.finalScore;

        }






        
    }

    private Tile GetBestTile()
    {
        ScoreTiles();

        Tile tile = unplayedTiles[0];
        //now all tiles have score. we collect those who have the same high score in a list to pick a random highest level pick.. 
        //note def modifier if was larger than attack mod, it will always favor defending on winning move (I think).
        for (int i = 0; i < unplayedTiles.Count; i++)
        {
            if (unplayedTiles[i].winningTile) return unplayedTiles[i]; // end of search

            if (unplayedTiles[i].finalScore >= tile.finalScore)
            {
                tile = unplayedTiles[i];
            }
        }
        // now we got the max value ever, now we make a list of any tile that has the same value or even more as a precaution
        // if not a winning tile already
        if (tile.winningTile == false)
        {
            List<Tile> playableTiles = new List<Tile>(); // collecting must plays and best tiles to play
            List<Tile> mustPlayTiles = new List<Tile>(); //
            for (int i = 0; i < unplayedTiles.Count; i++)
            {
                if (unplayedTiles[i].finalScore >= tile.finalScore)
                {
                    playableTiles.Add(unplayedTiles[i]);
                }

                if (unplayedTiles[i].mustDefendTile)
                {
                    mustPlayTiles.Add(unplayedTiles[i]);
                    unplayedTiles[i].mustDefendTile = false;
                }
            }

            if (mustPlayTiles.Count > 0)// If we have those , we dont care about the rest
            {
                tile = mustPlayTiles[0];
                if (debug) Debug.Log("mustPlayTiles " + mustPlayTiles.Count);
                for (int i = 0; i < mustPlayTiles.Count; i++)
                {
                    if (mustPlayTiles[i].streakRed >= tile.streakRed) // get the tile with highest streak to be the most important to play if we will lose
                    {
                        tile = mustPlayTiles[i];
                    }
                }
            }
            else if (playableTiles.Count > 0)
            {
                tile = playableTiles.GetRandomValue();
            }

        }


        // Done
        return tile;
    }

    public void DebugTile(Tile tile, bool playerclicked = false)
    {
        if (!fullDebug) return;
        string tileNames = "AI tile\n";
        if (playerclicked) tileNames = "Player Clk\n";

        tileNames += "StreakRed = " + tile.streakRed + " || Streak Blue = " + tile.streakBlue + "\n";

        if (fullDebug)
        {
            for (int i = 0; i < 8; i++)
            {
                switch (i)
                {
                    case 0:
                        tileNames += "Left Tiles Horizontal: ";
                        for (int k = 0; k < tile.leftTiles.Count; k++)
                        {
                            tileNames += tile.leftTiles[k].Owner + " | ";
                        }
                        tileNames += "\n";
                        break;
                    case 1:
                        tileNames += "Right Tiles Horizontal: ";
                        for (int k = 0; k < tile.rightTiles.Count; k++)
                        {
                            tileNames += tile.rightTiles[k].Owner + " | ";
                        }
                        tileNames += "\n";
                        break;
                    case 2:
                        tileNames += "Up Tiles Vertical: ";
                        for (int k = 0; k < tile.upTiles.Count; k++)
                        {
                            tileNames += tile.upTiles[k].Owner + " | ";
                        }
                        tileNames += "\n";
                        break;
                    case 3:
                        tileNames += "Down Tiles Vertical: ";
                        for (int k = 0; k < tile.downTiles.Count; k++)
                        {
                            tileNames += tile.downTiles[k].Owner + " | ";
                        }
                        tileNames += "\n";
                        break;
                    case 4:
                        tileNames += "towards Top Right Tiles: ";
                        for (int k = 0; k < tile.northEastTiles.Count; k++)
                        {
                            tileNames += tile.northEastTiles[k].Owner + " | ";
                        }
                        tileNames += "\n";
                        break;
                    case 5:
                        tileNames += "towards Down left Tiles: ";
                        for (int k = 0; k < tile.southWestTiles.Count; k++)
                        {
                            tileNames += tile.southWestTiles[k].Owner + " | ";
                        }
                        tileNames += "\n";
                        break;
                    case 6:
                        tileNames += "towards Top left Tiles: ";
                        for (int k = 0; k < tile.northWestTiles.Count; k++)
                        {
                            tileNames += tile.northWestTiles[k].Owner + " | ";
                        }
                        tileNames += "\n";
                        break;
                    case 7:
                        tileNames += "towards Down Right Tiles: ";
                        for (int k = 0; k < tile.southEastTiles.Count; k++)
                        {
                            tileNames += tile.southEastTiles[k].Owner + " | ";
                        }
                        tileNames += "\n";
                        break;
                }
            }
        }
        
        Debug.Log(tileNames);
    }

    IEnumerator WaitThenDo(float wait, System.Action callback)
    {

        yield return new WaitForSeconds(wait);
        callback?.Invoke();
    
    }
}
