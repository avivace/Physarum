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
    float ThPM = 0.2f;

    int t = 0;

    /** False if the simulation has finished. */
    bool simulationRunning;

    /** True if we are doing the 50 steps where we apply the diffusion equations. */
    bool fiftyStepsPhase;
    /** Local timer to count 50 steps. */
    int localFiftyStepsTime;

    // Start is called before the first frame update
    void Start()
    {
        Initialization();
    }

    // Update is called once per frame
    void Update()
    {
        if (simulationRunning)
        {
            Simulation();
            UpdateTiles();
        }
    }

    /** Call this every time you want to restart the simulation. */
    public void Initialization()
    {
        mapCells = new Cell[mapSizeX, mapSizeY];
        LoadMap();
        t = 0;
        StartFiftyStepsPhase();
        simulationRunning = true;
        DrawTiles();
    }

    /** Popola la mappa con i vari tipi di cella. */   //TODO Qua bisogna caricare l'immagine della mappa
    void LoadMap()
    {
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                if (i == 0 && j == 0) {
                    mapCells[i, j] = new Cell(true, 100, 0, false, CellType.S);
                }
                else if (UnityEngine.Random.value > 0.7f)
                {
                    mapCells[i, j] = new Cell(false, 0, 0, false, CellType.U);
                }
                else if(UnityEngine.Random.value > 0.9f)
                {
                    mapCells[i, j] = new Cell(true, 0, 100, false, CellType.N);
                }
                else
                {
                    mapCells[i, j] = new Cell(true, 0, 0, false, CellType.A);
                }
            }
        }
    }

    /** Execution of the simulation. */
    void Simulation()
    {
        if(fiftyStepsPhase)
        {
            if (localFiftyStepsTime < 50)
                ApplyDiffusionEquations();
            else
                fiftyStepsPhase = false;
            localFiftyStepsTime++;
        } else
        {
            List<Vector2Int> coveredNs = new List<Vector2Int>();
            for (int i = 0; i < mapSizeX; i++)
            {
                for (int j = 0; j < mapSizeY; j++)
                {
                    if (mapCells[i, j].type == CellType.N && mapCells[i, j].PM >= ThPM)
                    {
                        coveredNs.Add(new Vector2Int(i, j));
                    }
                }
            }

            if (coveredNs.Count > 0)
            {
                foreach (Vector2Int cellPos in coveredNs)
                {
                    Cell cell = mapCells[cellPos.x, cellPos.y];

                    //Connect these Ns with the SP
                    //TODO

                    //Change N into SP
                    cell.type = CellType.S;
                    cell.PM = 100;
                }
            }

            if(t <= 5000)
            {
                if (t == 5000)
                {
                    //Change all NS and SP as NS
                    //TODO

                    //Il penultimo NS incapsulato diventa il nuovo SP
                    //TODO
                }
                
                StartFiftyStepsPhase();
            } else if(t < 10000)
            {
                StartFiftyStepsPhase();
            } else
            {
                simulationRunning = false;
            }
        }

        t++;
    }

    private void StartFiftyStepsPhase()
    {
        fiftyStepsPhase = true;
        localFiftyStepsTime = 0;
    }

    /** Equazioni di diffusione. */
    void ApplyDiffusionEquations()
    {
        Cell[,] newMap = CreateNewCellMap(mapSizeX, mapSizeY);

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

                newMap[i, j].PM = GetPM(i, j) + PMP1 * (PMvNN + PMP2 * PMeMN);
            }
        }

        //Calcolo CHA
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
                newMap[i, j].CHA = CON * (GetCHA(i, j) + CAP1 * (CHAvNN + CAP2 * CHAeMN));
            }
        }

        //Update the map
        UpdateMapWithNewDiffusionValues(newMap);
    }

    private void UpdateMapWithNewDiffusionValues(Cell[,] newMap)
    {
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                mapCells[i, j].PM = newMap[i, j].PM;
                mapCells[i, j].CHA = newMap[i, j].CHA;
            }
        }
    }

    /** Create a new filled map with generic cells. */
    private Cell[,] CreateNewCellMap(int mapSizeX, int mapSizeY)
    {
        Cell[,] res = new Cell[mapSizeX, mapSizeY];
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                res[i, j] = new Cell();
            }
        }

        return res;
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
    void DrawTiles()
    {
        Tilemap tilemap = this.GetComponent<Tilemap>();

        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                CellType type = mapCells[i, j].type;
                tilemap.SetTile(new Vector3Int(i, j, 0), tile);
            }
        }
    }

    /** Aggiorna le tile della TileMap con i colori corretti. */
    //TODO Qui vanno settate le sfumature delle tiles e i colori corretti
    void UpdateTiles()
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
