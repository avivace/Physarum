using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SimulationManager : MonoBehaviour
{
    public int mapSizeX = 10;
    public int mapSizeY = 10;

    public Cell[,] mapCells;

    public Tile tile;
     
    float CON = 1;
    float PAP = 0.3f;  
    float PMP1 = 0.05f;
    float PMP2 = 0.025f;
    float PMP3 = 1;
    float CAP1 = 0.05f;
    float CAP2 = 0.025f;
    float CAP3 = 1;

    // Start is called before the first frame update
    void Start()
    {
        mapCells = new Cell[mapSizeX, mapSizeY];
        CreateMap();
        InitTiles();
    }

    // Update is called once per frame
    void Update()
    {
        DrawTiles();
    }

    /** Popola la mappa con i vari tipi di cella. */
    //TODO Qui bisogna instanziare correttamente le CELL con i loro valori!
    void CreateMap()
    {
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                if (i == 0 && j == 0) {
                    mapCells[i, j] = new Cell(CellType.S);
                }
                else if (UnityEngine.Random.value > 0.7f)
                {
                    mapCells[i, j] = new Cell(CellType.U);
                }
                else if(UnityEngine.Random.value > 0.9f)
                {
                    mapCells[i, j] = new Cell(CellType.N);
                }
                else
                {
                    mapCells[i, j] = new Cell(CellType.A);
                }
            }
        }
    }

    /** Equazioni di diffusione. */
    //TODO I bounding fuoriescono dalle dimensioni dell'array!
    void Diffusion()
    {
        //Calcolo PM
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                float[] PA = new float[8];
                float[] values = new float[]{
                    mapCells[i - 1, j - 1].CHA, mapCells[i - 1, j].CHA, mapCells[i - 1, j + 1].CHA,
                    mapCells[i, j - 1].CHA, mapCells[i, j].CHA, mapCells[i, j + 1].CHA,
                    mapCells[i + 1, j - 1].CHA, mapCells[i + 1, j].CHA, mapCells[i + 1, j + 1].CHA };

                PA[0] = (CalculatePA(mapCells[i - 1, j].CHA, mapCells[i + 1, j].CHA, values));
                PA[1] = (CalculatePA(mapCells[i, j - 1].CHA, mapCells[i, j + 1].CHA, values));
                PA[2] = (CalculatePA(mapCells[i + 1, j].CHA, mapCells[i - 1, j].CHA, values));
                PA[3] = (CalculatePA(mapCells[i, j + 1].CHA, mapCells[i, j - 1].CHA, values));
                PA[4] = (CalculatePA(mapCells[i - 1, j - 1].CHA, mapCells[i + 1, j + 1].CHA, values));
                PA[5] = (CalculatePA(mapCells[i + 1, j - 1].CHA, mapCells[i - 1, j + 1].CHA, values));
                PA[6] = (CalculatePA(mapCells[i - 1, j + 1].CHA, mapCells[i + 1, j - 1].CHA, values));
                PA[7] = (CalculatePA(mapCells[i + 1, j + 1].CHA, mapCells[i - 1, j - 1].CHA, values));

                float PMvNN = ((1 + PA[0]) * mapCells[i - 1, j].PM - (mapCells[i - 1, j].AA ? 1 : 0) * mapCells[i, j].PM) + (
                     (1 + PA[1]) * mapCells[i, j - 1].PM - (mapCells[i, j - 1].AA ? 1 : 0) * mapCells[i, j].PM) + (
                     (1 + PA[2]) * mapCells[i + 1, j].PM - (mapCells[i + 1, j].AA ? 1 : 0) * mapCells[i, j].PM) + (
                     (1 + PA[3]) * mapCells[i, j + 1].PM - (mapCells[i, j + 1].AA ? 1 : 0) * mapCells[i, j].PM);
                float PMeMN = ((1 + PA[4]) * mapCells[i - 1, j - 1].PM - (mapCells[i - 1, j - 1].AA ? 1 : 0) * mapCells[i, j].PM) + (
                     (1 + PA[5]) * mapCells[i + 1, j - 1].PM - (mapCells[i + 1, j - 1].AA ? 1 : 0) * mapCells[i, j].PM) + (
                     (1 + PA[6]) * mapCells[i - 1, j + 1].PM - (mapCells[i - 1, j + 1].AA ? 1 : 0) * mapCells[i, j].PM) + (
                     (1 + PA[7]) * mapCells[i - 1, j + 1].PM - (mapCells[i - 1, j + 1].AA ? 1 : 0) * mapCells[i, j].PM);

                mapCells[i, j].PM = mapCells[i, j].PM + PMP1 * (PMvNN + PMP2 * PMeMN);
            }
        }

        //calcolo CHA
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                float CHAvNN = ((mapCells[i - 1, j].CHA) - (mapCells[i - 1, j].AA ? 1 : 0) * mapCells[i, j].CHA) + (
                      (mapCells[i, j - 1].CHA) - (mapCells[i, j - 1].AA ? 1 : 0) * mapCells[i, j].CHA) + (
                      (mapCells[i + 1, j].CHA) - (mapCells[i + 1, j].AA ? 1 : 0) * mapCells[i, j].CHA) + (
                      (mapCells[i, j + 1].CHA) - (mapCells[i, j + 1].AA ? 1 : 0) * mapCells[i, j].CHA);
                float CHAeMN = ((mapCells[i - 1, j - 1].CHA) - (mapCells[i - 1, j - 1].AA ? 1 : 0) * mapCells[i, j].CHA) + (
                          (mapCells[i + 1, j - 1].CHA) - (mapCells[i + 1, j - 1].AA ? 1 : 0) * mapCells[i, j].CHA) + (
                          (mapCells[i - 1, j + 1].CHA) - (mapCells[i - 1, j + 1].AA ? 1 : 0) * mapCells[i, j].CHA) + (
                          (mapCells[i + 1, j + 1].CHA) - (mapCells[i + 1, j + 1].AA ? 1 : 0) * mapCells[i, j].CHA);
                mapCells[i, j].CHA = CON * (mapCells[i, j].CHA + CAP1 * (CHAvNN + CAP2 * CHAeMN));
            }
        }
    }

    float CalculatePA(float coord_n, float coord_o, float[] values) {
        float max_value = GetMax(values);
        if (coord_n == max_value)
            return PAP;
        else if (coord_o == max_value)
            return -PAP;
        else
            return 0;
    }

    private float GetMax(float[] values)
    {
        float res = float.MinValue;
        foreach(float v in values)
        {
            if(v > res)
            {
                res = v;
            }
        }

        return res;
    }

    /** Inizializza le tiles grafiche nella TileMap di Unity per visualizzare le varie Celle. */
    void InitTiles()
    {
        Tilemap tilemap = this.GetComponent<Tilemap>();

        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeX; j++)
            {
                CellType type = mapCells[i, j].type;

                if (type == CellType.A)
                {
                    tilemap.SetTile(new Vector3Int(i, j, 0), tile);
                }
                else if (type == CellType.U)
                {
                    tilemap.SetTile(new Vector3Int(i, j, 0), tile);
                }
                else if (type == CellType.S)
                {
                    tilemap.SetTile(new Vector3Int(i, j, 0), tile);
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(i, j, 0), tile);
                }
            }
        }
    }

    /** Aggiorna le tile della TileMap con i colori corretti. */
    void DrawTiles()
    {
        Tilemap tilemap = this.GetComponent<Tilemap>();
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeX; j++)
            {
                CellType type = mapCells[i, j].type;

                if (type == CellType.A)
                {
                    SetTileColour(new Color(0, 1, 0, 1), new Vector3Int(i, j, 0));
                }
                else if (type == CellType.U)
                {
                    SetTileColour(new Color(0, 0, 0, 1), new Vector3Int(i, j, 0));
                }
                else if (type == CellType.S)
                {
                    SetTileColour(new Color(1, 0, 0, 1), new Vector3Int(i, j, 0));
                }
                else
                {
                    SetTileColour(new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1), new Vector3Int(i, j, 0));
                }
            }
        }
    }

    /** Setta il colore della Tile. */
    private void SetTileColour(Color colour, Vector3Int position)
    {
        Tilemap tilemap = this.GetComponent<Tilemap>();
        // Flag the tile, inidicating that it can change colour.
        // By default it's set to "Lock Colour".
        tilemap.SetTileFlags(position, TileFlags.None);

        // Set the colour.
        tilemap.SetColor(position, colour);
    }
}
