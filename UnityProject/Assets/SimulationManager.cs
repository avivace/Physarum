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
    void Diffusion()
    {
        //Calcolo PM
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                float[] PA = new float[8];
                float[] values = new float[]{
                    GetCHA(i - 1, j - 1),
                    GetCHA(i - 1, j),
                    GetCHA(i - 1, j + 1),
                    GetCHA(i, j - 1),
                    GetCHA(i, j),
                    GetCHA(i, j + 1),
                    GetCHA(i + 1, j - 1),
                    GetCHA(i + 1, j),
                    GetCHA(i + 1, j + 1) };

                PA[0] = (CalculatePA(GetCHA(i - 1, j), GetCHA(i + 1, j), values));
                PA[1] = (CalculatePA(GetCHA(i, j - 1), GetCHA(i, j + 1), values));
                PA[2] = (CalculatePA(GetCHA(i + 1, j), GetCHA(i - 1, j), values));
                PA[3] = (CalculatePA(GetCHA(i, j + 1), GetCHA(i, j - 1), values));
                PA[4] = (CalculatePA(GetCHA(i - 1, j - 1), GetCHA(i + 1, j + 1), values));
                PA[5] = (CalculatePA(GetCHA(i + 1, j - 1), GetCHA(i - 1, j + 1), values));
                PA[6] = (CalculatePA(GetCHA(i - 1, j + 1), GetCHA(i + 1, j - 1), values));
                PA[7] = (CalculatePA(GetCHA(i + 1, j + 1), GetCHA(i - 1, j - 1), values));

                float PMvNN = ((1 + PA[0]) * GetPM(i - 1, j) - (GetAA(i - 1, j) ? 1 : 0) * GetPM(i, j)) + (
                     (1 + PA[1]) * GetPM(i, j - 1) - (GetAA(i, j - 1) ? 1 : 0) * GetPM(i, j)) + (
                     (1 + PA[2]) * GetPM(i + 1, j) - (GetAA(i + 1, j) ? 1 : 0) * GetPM(i, j)) + (
                     (1 + PA[3]) * GetPM(i, j + 1) - (GetAA(i, j + 1) ? 1 : 0) * GetPM(i, j));
                float PMeMN = ((1 + PA[4]) * GetPM(i - 1, j - 1) - (GetAA(i - 1, j - 1) ? 1 : 0) * GetPM(i, j)) + (
                     (1 + PA[5]) * GetPM(i + 1, j - 1) - (GetAA(i + 1, j - 1) ? 1 : 0) * GetPM(i, j)) + (
                     (1 + PA[6]) * GetPM(i - 1, j + 1) - (GetAA(i - 1, j + 1) ? 1 : 0) * GetPM(i, j)) + (
                     (1 + PA[7]) * GetPM(i - 1, j + 1) - (GetAA(i - 1, j + 1) ? 1 : 0) * GetPM(i, j));

                mapCells[i, j].PM = GetPM(i, j) + PMP1 * (PMvNN + PMP2 * PMeMN);
            }
        }

        //calcolo CHA
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                float CHAvNN = ((GetCHA(i - 1, j)) - (GetAA(i - 1, j) ? 1 : 0) * GetCHA(i, j)) + (
                      (GetCHA(i, j - 1)) - (GetAA(i, j - 1) ? 1 : 0) * GetCHA(i, j)) + (
                      (GetCHA(i + 1, j)) - (GetAA(i + 1, j) ? 1 : 0) * GetCHA(i, j)) + (
                      (GetCHA(i, j + 1)) - (GetAA(i, j + 1) ? 1 : 0) * GetCHA(i, j));
                float CHAeMN = ((GetCHA(i - 1, j - 1)) - (GetAA(i - 1, j - 1) ? 1 : 0) * GetCHA(i, j)) + (
                          (GetCHA(i + 1, j - 1)) - (GetAA(i + 1, j - 1) ? 1 : 0) * GetCHA(i, j)) + (
                          (GetCHA(i - 1, j + 1)) - (GetAA(i - 1, j + 1) ? 1 : 0) * GetCHA(i, j)) + (
                          (GetCHA(i + 1, j + 1)) - (GetAA(i + 1, j + 1) ? 1 : 0) * GetCHA(i, j));
                mapCells[i, j].CHA = CON * (GetCHA(i, j) + CAP1 * (CHAvNN + CAP2 * CHAeMN));
            }
        }
    }

    float GetCHA(int i, int j)
    {
        if (i < 0 || i >= mapSizeX || j < 0 || j >= mapSizeY)
            return 0;
        else
            return mapCells[i, j].CHA;
    }

    float GetPM(int i, int j)
    {
        if (i < 0 || i >= mapSizeX || j < 0 || j >= mapSizeY)
            return 0;
        else
            return mapCells[i, j].PM;
    }

    bool GetAA(int i, int j)
    {
        if (i < 0 || i >= mapSizeX || j < 0 || j >= mapSizeY)
            return false;
        else
            return mapCells[i, j].AA;
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
            for (int j = 0; j < mapSizeY; j++)
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
            for (int j = 0; j < mapSizeY; j++)
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
