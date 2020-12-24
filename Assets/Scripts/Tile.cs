using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Tile : MonoBehaviour
{

    private Player owner = Player.None;
    public int attackScore = 0;
    public int defenceScore = 0;
    public int finalScore = 0;
    public bool winningTile = false;
    public bool mustDefendTile = false;
    public int streakRed = 0; //debug
    public int streakBlue = 0; //debug

    public int x;
    public int y;
    public int z;
    public int listIndex = 0;
    public GameObject ball;

    Color playerXColor = Color.red;
    Color playerOColor = Color.blue;
    private MeshRenderer meshRend;


    [HideInInspector] public List<Tile> leftTiles;
    [HideInInspector] public List<Tile> rightTiles;
    [HideInInspector] public List<Tile> upTiles;
    [HideInInspector] public List<Tile> downTiles;
    [HideInInspector] public List<Tile> northWestTiles;
    [HideInInspector] public List<Tile> northEastTiles;
    [HideInInspector] public List<Tile> southWestTiles;
    [HideInInspector] public List<Tile> southEastTiles;




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

    private void OnMouseDown()
    {
        if(Owner == Player.None && GameHandler.Instance.currentPlayer == Player.Red && GameHandler.Instance.gameEnded == false && GameHandler.Instance.gameStarted)
        {
            Owner = GameHandler.Instance.currentPlayer; //also changes mesh
            Invoke("NotifyHandler", 0.5f);
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
    

}
