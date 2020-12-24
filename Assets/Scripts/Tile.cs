using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;

public class Tile : MonoBehaviour
{

    [SerializeField] private Player owner = Player.None;
    public int attackScore = 0;
    public int defenceScore = 0;
    public int finalScore = 0;
    public bool winningTile = false;
    public bool mustDefendTile = false;
    public int streakRed = 0; //debug
    public int streakBlue = 0; //debug
    public int maxStreakRed = 0; //debug
    public int maxStreakBlue = 0; //debug

    public int x;
    public int y;
    public int z;
    public int listIndex = 0;
    public GameObject ball;

    Color playerXColor = Color.red;
    Color playerOColor = Color.blue;
    private MeshRenderer meshRend;


    public List<Tile> leftTiles;
    public List<Tile> rightTiles;
    public List<Tile> upTiles;
    public List<Tile> downTiles;
    public List<Tile> northWestTiles;
    public List<Tile> northEastTiles;
    public List<Tile> southWestTiles;
    public List<Tile> southEastTiles;


    public GameObject hoverEffect;

    public Player Owner
    {
        get
        {
            return owner;
        }

       set
        {
            owner = value;

            if(ball != null)
            {
                if (meshRend == null) meshRend = ball.GetComponent<MeshRenderer>();
                else meshRend = ball.GetComponent<MeshRenderer>();
            }
            else
            {
                if (meshRend == null) meshRend = GetComponent<MeshRenderer>();
                else meshRend = GetComponent<MeshRenderer>();
            }


            if (meshRend != null)
            {
                switch (owner)
                {
                    case Player.None:
                        if (ball != null) ball.gameObject.SetActive(false);
                        meshRend.material.color = Color.white;
                        break;
                    case Player.Red:
                        if (ball != null)
                        {
                            ball.gameObject.SetActive(true);
                            Vector3 pos = ball.gameObject.transform.localPosition;
                            ball.transform.localPosition = ball.transform.localPosition + Vector3.up;
                            ball.transform.LeanMoveLocal(pos, GameHandler.Instance.ballDropSpeed);
                        }
                        meshRend.material.color = playerXColor;
                        break;
                    case Player.Blue:
                        if (ball != null)
                        {
                            ball.gameObject.SetActive(true);
                            Vector3 pos = ball.gameObject.transform.localPosition;
                            ball.transform.localPosition = ball.transform.localPosition + Vector3.up;
                            ball.transform.LeanMoveLocal(pos, GameHandler.Instance.ballDropSpeed);
                        }
                        meshRend.material.color = playerOColor;
                        break;
                    default:
                        break;
                }
            }
        }
    }


    public void OnMouseDown()
    {
        if(Owner == Player.None &&
            GameHandler.Instance.currentPlayer == Player.Red && 
            GameHandler.Instance.gameEnded == false &&
            GameHandler.Instance.gameStarted &&
            GameHandler.Instance.paused == false)
        {
            Owner = GameHandler.Instance.currentPlayer; //also changes mesh


            if (hoverEffect != null)
            {
                hoverEffect.SetActive(false);
            }

            Invoke("NotifyHandler", 0.5f);

            if (GameHandler.Instance.debug)
            {
                DirectionStreak fromHori = GetStreakInAxis(Axis.Horizontal);
                DirectionStreak fromVerti = GetStreakInAxis(Axis.Vertical);
                DirectionStreak fromDiagonalRight = GetStreakInAxis(Axis.DiagonalRightToLeft);
                DirectionStreak fromDiagonaLeft = GetStreakInAxis(Axis.DiagonalLeftToRight);

                string values = "";

                values += "R: " + fromHori.reds + " || B: " + fromHori.blues +" STM R: "+ fromHori.redsMax + " || STM B: " + fromHori.bluesMax + " Horizontal \n";
                values += "R: " + fromVerti.reds + " || B: " + fromVerti.blues +" STM R: "+ fromVerti.redsMax + " || STM B: " + fromVerti.bluesMax + " Vertical \n";
                values += "R: " + fromDiagonalRight.reds + " || B: " + fromDiagonalRight.blues +" STM R: "+ fromDiagonalRight.redsMax + " || STM B: " + fromDiagonalRight.bluesMax + " RtL NE > SW\n";
                values += "R: " + fromDiagonaLeft.reds + " || B: " + fromDiagonaLeft.blues + " STM R: " + fromDiagonaLeft.redsMax + " || STM B: " + fromDiagonaLeft.bluesMax + " LtR NW > SE\n";
  

                Debug.Log("Selected Tile Data\n" + values);
                //GameHandler.Instance.DebugTile(this, true);
            }
        }
    }

    private void OnMouseEnter()
    {
        if (hoverEffect != null && 
            Owner == Player.None &&
            GameHandler.Instance.gameStarted &&
            GameHandler.Instance.paused == false &&
            GameHandler.Instance.gameEnded == false) 
        {
            hoverEffect.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        if (hoverEffect != null)
        {
            hoverEffect.SetActive(false);
        }
    }

    private void NotifyHandler()
    {
        GameHandler.Instance.PlayerPlayed(this);
    }


    public void PlayTile(Player player)
    {
        Owner = player;
        Invoke("NotifyHandler", 0.5f);
        //GameHandler.Instance.PlayerPlayed(this);
    }
    
    public DirectionStreak GetStreakInDirection(Direction dir) // Vector 3 : X = Reds , Y = Blues
    {
        DirectionStreak counts = new DirectionStreak();


        Player firstEncounter = Player.None;
        List<Tile> tmpDirLst = null;
        switch (dir)
        {
            case Direction.Left:
                {
                    tmpDirLst = new List<Tile>(leftTiles);
                    break;
                }
            case Direction.Right:
                {
                    tmpDirLst = new List<Tile>(rightTiles);
                    break;
                }
            case Direction.Up:
                {
                    tmpDirLst = new List<Tile>(upTiles);
                    break;
                }
            case Direction.Down:
                {
                    tmpDirLst = new List<Tile>(downTiles);
                    break;
                }
            case Direction.NE:
                {
                    tmpDirLst = new List<Tile>(northEastTiles);
                    break;
                }
            case Direction.SW:
                {
                    tmpDirLst = new List<Tile>(southWestTiles);
                    break;
                }
            case Direction.NW:
                {
                    tmpDirLst = new List<Tile>(northWestTiles);
                    break;
                }
            case Direction.SE:
                {
                    tmpDirLst = new List<Tile>(southEastTiles);
                    break;
                }
        }

        if(tmpDirLst != null)
        {
            if (tmpDirLst.Count > 0)
            {
                firstEncounter = tmpDirLst[0].Owner;
                bool broken = false;
                if (firstEncounter != Player.None)
                {
                    //counts.emptyNear = false;
                    for (int i = 0; i < tmpDirLst.Count; i++)
                    {


                        if (tmpDirLst[i].Owner == Player.None)
                        {
                            switch (firstEncounter)
                            {
                                case Player.Red:
                                    counts.redsMax++;
                                    counts.redsBlocked = false;
                                    break;
                                case Player.Blue:
                                    counts.bluesMax++;
                                    counts.bluesBlocked = false;
                                    break;
                            }
                            broken = true;
                            //break;
                        }
                        else
                        if (tmpDirLst[i].Owner == firstEncounter) 
                        {
                            if (broken) break;

                            switch (firstEncounter)
                            {
                                case Player.Red: 
                                    counts.reds++;
                                    counts.redsMax++;

                                    break;
                                case Player.Blue:
                                    counts.blues++;
                                    counts.bluesMax++;
                                    break;
                            }
                        }
                        else
                        if (tmpDirLst[i].Owner != firstEncounter)
                        {
                            switch (firstEncounter)
                            {
                                case Player.Red:
                                    if (!broken) counts.redsBlocked = true;
                                    break;
                                case Player.Blue:
                                    if (!broken) counts.bluesBlocked = true;
                                    break;
                            }
                            break;
                        }
                    }
                }
                else //First is empty ... Will just calculate streak
                {
                    //counts.emptyNear = true;

                    //int maxEmpty = 0;
                    //bool needEmpty = true;
                    for (int i = 0; i < tmpDirLst.Count; i++)
                    {
                        if (tmpDirLst[i].Owner == Player.None && firstEncounter == Player.None)
                        {
                            counts.redsMax++;
                            counts.bluesMax++;
                            //maxEmpty++;
                        }
                        else
                        if (tmpDirLst[i].Owner != Player.None && firstEncounter == Player.None)
                        {
                            firstEncounter = tmpDirLst[i].Owner;
                            switch (firstEncounter)
                            {
                                case Player.Red:
                                    //counts.reds++;
                                    counts.redsMax++;
                                    break;
                                case Player.Blue:
                                    //counts.blues++;
                                    counts.bluesMax++;
                                    break;
                            }
                            //needEmpty = false;
                        }
                        else
                        if (tmpDirLst[i].Owner == Player.None && firstEncounter != Player.None)
                        {
                            switch (firstEncounter)
                            {
                                case Player.Red:
                                    counts.redsMax++;
                                    break;
                                case Player.Blue:
                                    counts.bluesMax++;
                                    break;
                            }
                        }
                        else
                        if (tmpDirLst[i].Owner == firstEncounter && firstEncounter != Player.None)
                        {
                           // needEmpty = false;
                            switch (firstEncounter)
                            {
                                case Player.Red:
                                    //counts.reds++;
                                    counts.redsMax++;
                                    break;
                                case Player.Blue:
                                    //counts.blues++;
                                    counts.bluesMax++;
                                    break;
                            }
                        }
                        else
                        if (tmpDirLst[i].Owner != firstEncounter && firstEncounter != Player.None)
                        {
                            break;
                        }

                        //if (maxEmpty >= 3 && needEmpty) break;
                    }
                }
            }
        }


        return counts;
    }

    public DirectionStreak GetStreakInAxis(Axis axis)
    {
        Direction left = Direction.Count;
        Direction right = Direction.Count;

        switch (axis)
        {
            case Axis.Horizontal:
                left = Direction.Left;
                right = Direction.Right;
                break;
            case Axis.Vertical:
                left = Direction.Up;
                right = Direction.Down;
                break;
            case Axis.DiagonalLeftToRight:
                left = Direction.NW;
                right = Direction.SE;
                break;
            case Axis.DiagonalRightToLeft:
                left = Direction.NE;
                right = Direction.SW;
                break;
            case Axis.Count:
                return new DirectionStreak();
        }
        if (left == Direction.Count || right == Direction.Count)
        {
            Debug.LogError("Error Getting Direction");
            return new DirectionStreak();
        }

        DirectionStreak countsLeft = GetStreakInDirection(left);
        DirectionStreak countsRight = GetStreakInDirection(right);


        countsLeft.reds += countsRight.reds;
        countsLeft.blues += countsRight.blues;
        countsLeft.redsMax += countsRight.redsMax;
        countsLeft.bluesMax += countsRight.bluesMax;

        return countsLeft;

    }

}
