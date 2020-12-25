using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBuilder : MonoBehaviour
{
    public int boardSizeX = 9;
    public int boardSizeY = 9;
    public int boardSizeZ = 9;

    public float separation = 0;
    public float objectSize = 1;
    public Pooler pooler;

    private GameObject cursor;
    //public List<Tile> tiles;



    public List<Tile> CreateGrid(List<Tile> tiles)
    {
        //tiles = null;
        if (tiles != null)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].gameObject.SetActive(false);
            }
        }
        tiles = new List<Tile>();


        cursor = new GameObject("cursor");
        cursor.transform.position = this.transform.position;
        float startPosX = 0;
        float startPosY = 0;
        float startPosZ = 0;

        if (boardSizeX % 2 == 0) startPosX = ((boardSizeX - 1) / 2) * (objectSize + separation) + (objectSize / 2 + separation / 2);
        else startPosX = (boardSizeX / 2) * (objectSize + separation);

        if (boardSizeY % 2 == 0) startPosY = ((boardSizeY - 1) / 2) * (objectSize + separation) + (objectSize / 2 + separation / 2);
        else startPosY = (boardSizeY / 2) * (objectSize + separation);

        if (boardSizeZ % 2 == 0) startPosZ = ((boardSizeZ - 1) / 2) * (objectSize + separation) + (objectSize / 2 + separation / 2);
        else startPosZ = (boardSizeZ / 2) * (objectSize + separation);


        cursor.transform.position = new Vector3(cursor.transform.position.x - startPosX, cursor.transform.position.y + startPosY, cursor.transform.position.z + startPosZ);
        float startX = cursor.transform.position.x;
        float startY = cursor.transform.position.y;
        float startZ = cursor.transform.position.z;


        Tile currentTile;
        for (int i = 0; i < boardSizeZ; i++)
        {
            for (int j = 0; j < boardSizeY; j++)
            {
                for (int k = 0; k < boardSizeX; k++)
                {

                    currentTile = pooler.Get(true).GetComponent<Tile>();
                    tiles.Add(currentTile);

                    currentTile.x = k;
                    currentTile.y = j;
                    currentTile.z = i;
                    currentTile.listIndex = tiles.Count - 1;

                    currentTile.transform.position = cursor.transform.position;
                    cursor.transform.position = new Vector3(cursor.transform.position.x + (objectSize + separation), cursor.transform.position.y, cursor.transform.position.z);

                }
                cursor.transform.position = new Vector3(startX, cursor.transform.position.y - (objectSize + separation), cursor.transform.position.z);
            }
            cursor.transform.position = new Vector3(startX, startY, cursor.transform.position.z - (objectSize + separation));

        }

        SetTilesNeighbors(tiles);
        return tiles;
    }


    private void SetTilesNeighbors(List<Tile> tiles)
    {
        if (tiles == null) return;

        Tile tile;
        for (int i = 0; i < tiles.Count; i++)
        {
            tile = tiles[i];
            //tile.leftTiles = tiles.FindAll((t => t.z == tile.z && t.x < tile.x));
            //tile.rightTiles = tiles.FindAll((t => t.z == tile.z && t.x > tile.x));
            //tile.upTiles = tiles.FindAll((t => t.x == tile.x && t.z < tile.z));
            //tile.downTiles = tiles.FindAll((t => t.x == tile.x && t.z > tile.z));

            Tile temp;
            for (int j = 1; j < 6; j++)
            {

                temp = tiles.Find(t => (t.z == tile.z && t.x == tile.x - j));
                if (temp != null && tile != temp) tile.leftTiles.Add(temp);
                temp = null;

                temp = tiles.Find(t => (t.z == tile.z && t.x == tile.x + j));
                if (temp != null && tile != temp) tile.rightTiles.Add(temp);
                temp = null;

                temp = tiles.Find(t => (t.x == tile.x && t.z == tile.z + j));
                if (temp != null && tile != temp) tile.downTiles.Add(temp);
                temp = null;

                temp = tiles.Find(t => (t.x == tile.x && t.z == tile.z - j));
                if (temp != null && tile != temp) tile.upTiles.Add(temp);
                temp = null;

                temp = tiles.Find(t => (t.x == tile.x - j && t.z == tile.z - j && t.x != tile.x && t.z != tile.z));
                if (temp != null && tile != temp) tile.northWestTiles.Add(temp);
                temp = null;
                
                temp = tiles.Find(t => (t.x == tile.x + j && t.z == tile.z - j && t.x != tile.x && t.z != tile.z));
                if (temp != null && tile != temp) tile.northEastTiles.Add(temp);
                temp = null;
                
                temp = tiles.Find(t => (t.x == tile.x + j && t.z == tile.z + j && t.x != tile.x && t.z != tile.z));
                if (temp != null && tile != temp) tile.southEastTiles.Add(temp);
                temp = null;
                
                temp = tiles.Find(t => (t.x == tile.x - j && t.z == tile.z + j && t.x != tile.x && t.z != tile.z));
                if (temp != null && tile != temp) tile.southWestTiles.Add(temp);
                temp = null;
            }
            
        }
    }

}

