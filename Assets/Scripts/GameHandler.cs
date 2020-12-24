using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum Player
{
    None, Red, Blue
}



public class GameHandler : MonoBehaviour
{

    public TileBuilder tileBuilder;
    public List<Tile> tiles;
    private List<Tile> unplayedTiles;
    public Player currentPlayer = Player.Red;
    public bool gameEnded = false;
    public bool gameStarted = false;

    public GameObject gamecamera;
    public Transform cameraDestentation;
    public float moveSpeed = 2;

    public int attackScoreModifier = 1;
    public int defenceScoreModifier = 2;
    public int emptyScoreModifier = -1;

    public float ballDropSpeed = 0.2f;
    public bool debug = true;

    public TextMeshProUGUI gameEndingText;
    private int plays = 0;
    private static GameHandler instance;

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
            Application.Quit();
        }
    }


    public void PlayerPlayed(Tile playedTile) 
    {
        //to do
        //check if a player won
        // if won -- END
        // Next
        //read all tiles, find what is the next tile to play for AI
        //check if AI wins

        unplayedTiles.Remove(playedTile);

        plays++; // check how many plays to know if all tiles played

        

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

        //Debug.Log("Nope");

        return false;
    }
    private void AIPlay()
    {
        //if(plays < 3) unplayedTiles.GetRandomValue().PlayTile(currentPlayer); //Play randomly in the first 2 rounds

        Tile tileToPlay = ScoreTiles();
        if (tileToPlay != null) 
        {
            tileToPlay.PlayTile(currentPlayer);
        }
        else
        {
            Debug.LogError("No Tiles found from AI search");
        }

    }

    private Tile ScoreTiles()
    {
        //check every empty tile for possible plays in each direction and give score accordingly
        //Tile with best score is returned


        Tile tile = null;
        //Set Empty Tiles Scores
        for (int i = 0; i < unplayedTiles.Count; i++)
        {

            if (tiles[i].Owner != Player.None) continue;

            tile = unplayedTiles[i];
            tile.mustDefendTile = false;
            tile.attackScore = 0;
            tile.defenceScore = 0;
            tile.finalScore = 0;
            tile.streakBlue = 0;
            tile.streakRed = 0;
            tile.winningTile = false;

            int bluesCounterLeft = 0;
            int bluesCounterRight = 0;
            int redsCounterLeft = 0;
            int redsCounterRight = 0;
            int maxStreakBlue = 0;
            int maxStreakRed = 0;
            int totalBlues;
            int totalReds;
            List<Tile> directionList = new List<Tile>();
            int side = 0;
            bool clear = false;
            for (int k = 0; k < 8; k++) // Go through the side lists
            {
                switch (k)
                {
                    case 0:
                        directionList = tile.leftTiles;
                        side = -1;
                        clear = false;
                        break;
                    case 1:
                        directionList = tile.rightTiles;
                        side = 1;
                        clear = true;
                        break;
                    case 2:
                        directionList = tile.upTiles;
                        clear = false;
                        side = 1;
                        break;
                    case 3:
                        directionList = tile.downTiles;
                        side = -1;
                        clear = true;
                        break;
                    case 4:
                        directionList = tile.northEastTiles;
                        clear = false;
                        side = 1;
                        break;
                    case 5:
                        directionList = tile.southWestTiles; 
                        clear = true;
                        side = -1;
                        break;
                    case 6:
                        directionList = tile.northWestTiles;
                        clear = false;
                        side = -1;
                        break;
                    case 7:
                        directionList = tile.southEastTiles;
                        clear = true;
                        side = 1;
                        break;
                }

 


                bool brokenRed = false;//per direction
                bool brokenBlue = false;//per direction
                for (int j = 0; j < directionList.Count; j++)
                {
                    if (tile != directionList[j]) // a tmplist is one of the lists of the neigbouring tiles in each direction, tiles to the left and to the right have their own lists. etc..
                    {
                        if(directionList[j].Owner == Player.None)
                        {
                            brokenBlue = true;
                            brokenRed = true;
                        }
                        if (directionList[j].Owner == Player.Blue && !brokenBlue) //AI
                        {
                            brokenRed = true;
                            if(side == 1)
                            {
                                bluesCounterRight++;
                            }
                            else if(side == -1)
                            {
                                bluesCounterLeft++;
                            }
                            maxStreakBlue++;
                        }
                        if (directionList[j].Owner == Player.Red && !brokenRed) //Human
                        {
                            brokenBlue = true;
                            if (side == 1)
                            {
                                redsCounterRight++;
                            }
                            else if (side == -1)
                            {
                                redsCounterLeft++;
                            }
                            maxStreakRed++;
                        }
                    }
                }




                // Taking the Highest score ever

                if (clear)
                {
                    totalReds = redsCounterLeft + redsCounterRight;
                    totalBlues = bluesCounterLeft + bluesCounterRight;



                    tile.streakBlue = totalBlues >= tile.streakBlue ? totalBlues : tile.streakBlue;//debuging
                    tile.streakRed = totalReds >= tile.streakRed ? totalReds : tile.streakRed;//debuging

                    int scoreDef = totalReds * defenceScoreModifier;
                    int scoreAtt = totalBlues * attackScoreModifier;

                    if (totalBlues >= 4 && maxStreakBlue >= 4)
                    {
                        tile.winningTile = true; // This will make the tile picked anyway
                        Debug.Log("Winning Tile");
                        DebugTile(tile);
                        return tile; // no need to try
                    }
                    if (totalReds >= 3 && maxStreakRed >= 4)
                    {
                        //a must def tile
                        //will make it ignore the modifiers
                        tile.mustDefendTile = true;
                    }
                    // if a almost win or player played a 3, then score is multiplied by 100
                    if (totalReds >= 3 && totalReds < 4) scoreDef = totalReds * 100;
                    if (totalBlues >= 3 && totalBlues < 4) scoreAtt = totalBlues * 100;
                    //This ignores the scores

                    // totalBlues and totalReds are the streaks of blues and reds in each direction
                    // usually first time added can be low not more than 4 on streak, since 5 is a win/lose (if one side) anyway
                    tile.attackScore = tile.attackScore >= scoreAtt ? tile.attackScore : scoreAtt;
                    tile.defenceScore = tile.defenceScore >= scoreDef ? tile.defenceScore : scoreDef;

                    int newFinalScore = tile.attackScore >= tile.defenceScore ? tile.attackScore : tile.defenceScore;


                    tile.finalScore = newFinalScore >= tile.finalScore ? newFinalScore : tile.finalScore;

                    maxStreakRed = 0;
                    maxStreakBlue = 0;
                    bluesCounterLeft = 0;
                    redsCounterLeft = 0;
                    bluesCounterRight = 0;
                    redsCounterRight = 0;
                }

                //clear for next direction..

            } 
            
            #region old
            ////check horizontal
            ////left
            //for (int j = 0; j < tile.leftTiles.Count; j++)
            //{
            //    if(tile != tile.leftTiles[j])
            //    {
            //        if (tile.leftTiles[j].Owner == Player.Blue)
            //        {
            //            tile.attackScore += attackScoreModifier;
            //        }
            //        if(tile.leftTiles[j].Owner == Player.Red)
            //        {
            //            tile.defenceScore += defenceScoreModifier;
            //        }
            //    }
            //}
            ////right
            //counter = 0;
            //broken = false;
            //for (int j = 0; j < tile.rightTiles.Count; j++)
            //{
            //    if (tile != tile.rightTiles[j])
            //    {
            //        if (tile.rightTiles[j].Owner == Player.Blue)
            //        {
            //            tile.attackScore += attackScoreModifier;
            //        }
            //        if (tile.rightTiles[j].Owner == Player.Red)
            //        {
            //            tile.defenceScore += defenceScoreModifier;
            //        }
            //        if (tile.rightTiles[j].Owner == Player.None)
            //        {
            //            tile.defenceScore += emptyScoreModifier;
            //        }
            //    }
            //}
            ////Up
            //counter = 0;
            //broken = false;
            //for (int j = 0; j < tile.upTiles.Count; j++)
            //{
            //    if (tile != tile.upTiles[j])
            //    {
            //        if (tile.upTiles[j].Owner == Player.Blue)
            //        {
            //            tile.attackScore += attackScoreModifier;
            //        }
            //        if (tile.upTiles[j].Owner == Player.Red)
            //        {
            //            tile.defenceScore += defenceScoreModifier;
            //        }
            //        if (tile.upTiles[j].Owner == Player.None)
            //        {
            //            tile.defenceScore += emptyScoreModifier;
            //        }
            //    }
            //}
            ////Down
            //counter = 0;
            //broken = false;
            //for (int j = 0; j < tile.downTiles.Count; j++)
            //{
            //    if (tile != tile.downTiles[j])
            //    {
            //        if (tile.downTiles[j].Owner == Player.Blue)
            //        {
            //            tile.attackScore += attackScoreModifier;
            //        }
            //        if (tile.downTiles[j].Owner == Player.Red)
            //        {
            //            tile.defenceScore += defenceScoreModifier;
            //        }
            //        if (tile.downTiles[j].Owner == Player.None)
            //        {
            //            tile.defenceScore += emptyScoreModifier;
            //        }
            //    }
            //}
            ////NW
            //counter = 0;
            //broken = false;
            //for (int j = 0; j < tile.northWestTiles.Count; j++)
            //{
            //    if (tile != tile.northWestTiles[j])
            //    {
            //        if (tile.northWestTiles[j].Owner == Player.Blue)
            //        {
            //            tile.attackScore += attackScoreModifier;
            //        }
            //        if (tile.northWestTiles[j].Owner == Player.Red)
            //        {
            //            tile.defenceScore += defenceScoreModifier;
            //        }
            //        if (tile.northWestTiles[j].Owner == Player.None)
            //        {
            //            tile.defenceScore += emptyScoreModifier;
            //        }
            //    }
            //}
            ////NE
            //counter = 0;
            //broken = false;
            //for (int j = 0; j < tile.northEastTiles.Count; j++)
            //{
            //    if (tile != tile.northEastTiles[j])
            //    {
            //        if (tile.northEastTiles[j].Owner == Player.Blue)
            //        {
            //            tile.attackScore += attackScoreModifier;
            //        }
            //        if (tile.northEastTiles[j].Owner == Player.Red)
            //        {
            //            tile.defenceScore += defenceScoreModifier;
            //        }
            //        if (tile.northEastTiles[j].Owner == Player.None)
            //        {
            //            tile.defenceScore += emptyScoreModifier;
            //        }
            //    }
            //}
            ////SW
            //counter = 0;
            //broken = false;
            //for (int j = 0; j < tile.southWestTiles.Count; j++)
            //{
            //    if (tile != tile.southWestTiles[j])
            //    {
            //        if (tile.southWestTiles[j].Owner == Player.Blue)
            //        {
            //            tile.attackScore += attackScoreModifier;
            //        }
            //        if (tile.southWestTiles[j].Owner == Player.Red)
            //        {
            //            tile.defenceScore += defenceScoreModifier;
            //        }
            //        if (tile.southWestTiles[j].Owner == Player.None)
            //        {
            //            tile.defenceScore += emptyScoreModifier;
            //        }
            //    }
            //}
            ////SE
            //counter = 0;
            //broken = false;
            //for (int j = 0; j < tile.southEastTiles.Count; j++)
            //{
            //    if (tile != tile.southEastTiles[j])
            //    {
            //        if (tile.southEastTiles[j].Owner == Player.Blue)
            //        {
            //            tile.attackScore += attackScoreModifier;
            //        }
            //        if (tile.southEastTiles[j].Owner == Player.Red)
            //        {
            //            tile.defenceScore += defenceScoreModifier;
            //        }
            //        if (tile.southEastTiles[j].Owner == Player.None)
            //        {
            //            tile.defenceScore += emptyScoreModifier;
            //        }
            //    }
            //}
            #endregion
        }

        tile = unplayedTiles[0];
        //now all tiles have score. we collect them who have the same high score ina list to pick a random highest level pick.. 
        //note def modifier if was larger than attack mod, it will always favor defending on winning move.


        for (int i = 0; i < unplayedTiles.Count; i++)
        {
            if(unplayedTiles[i].winningTile == true)
            {
                tile = unplayedTiles[i];
                break;
            }

            
            if (unplayedTiles[i].finalScore >= tile.finalScore)
            {
                tile = unplayedTiles[i];
            }
        }
        // now we got the max value ever, now we make a list of any tile that has the same value or even more as a precaution
        // if not a winning tile already
        if(tile.winningTile == false)
        {
            List<Tile> playableTiles = new List<Tile>();
            List<Tile> mustPlayTiles = new List<Tile>();
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

            if(mustPlayTiles.Count > 0)
            {
                Debug.Log("mustPlayTiles " + mustPlayTiles.Count);
                tile = mustPlayTiles.GetRandomValue();
            }
            else if(playableTiles.Count > 0)
            {
                Debug.Log("playableTiles " + playableTiles.Count);
                tile = playableTiles.GetRandomValue();
            }

        }

        //print neighbors for debug
        DebugTile(tile);
        // Done
        return tile;
    }

    public void DebugTile(Tile tile)
    {
        if (debug)
        {
            string tileNames = "StreakRed = " + tile.streakRed + " || Streak Blue = " + tile.streakBlue + "\n";

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
            Debug.Log(tileNames);
        }
    }
}
