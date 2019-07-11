using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SimulationManager : MonoBehaviour
{
    /** Per essere usata, l'immagine .png deve aggiungere l'estensione .bytes */
    public TextAsset imageAsset;
    public Tile tile;

    Texture2D tex;

    int mapSizeX;
    int mapSizeY;

    public Cell[,] mapCells;
    public List<Vector2Int> Ss = new List<Vector2Int>();
    public List<Vector2Int> Ns = new List<Vector2Int>();

    public Payload payload;

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

    Vector2Int lastEncapsulatedNS;
    Vector2Int secondToLastEncapsulatedNS;

    void payloadHandler(){
        string jsonString = "{\"value1\":\"1\"}";
        Payload payload = JsonUtility.FromJson<Payload>(jsonString);
        Debug.Log(payload.value2);
        
    }

    // Start is called before the first frame update
    void Start()
    {   
        Application.ExternalCall("vm.$children[0].greet", "Hello from Unity!");
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
        LoadTextureMap();
        InitCellMap();
        t = 0;
        ResetFiftyStepsPhase();
        simulationRunning = true;
        DrawTiles();
    }

    /** Carica la mappa come Texture così può leggerne i pixel. */
    void LoadTextureMap()
    {
        tex = new Texture2D(2, 2);
        tex.LoadImage(imageAsset.bytes);
        mapSizeX = tex.width;
        mapSizeY = tex.height;
    }

    /** Popola la mappa con i vari tipi di cella. */
    void InitCellMap()
    {
        mapCells = new Cell[mapSizeX, mapSizeY];

        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                Color col = tex.GetPixel(i, j);

                if (col.Equals(Color.white))
                {
                    mapCells[i, j] = new Cell(true, 0, 0, false, CellType.A);
                }
                else if (col.Equals(Color.red))
                {
                    mapCells[i, j] = new Cell(false, 0, 0, false, CellType.U);
                }
                else if (col.Equals(Color.black))
                {
                    mapCells[i, j] = new Cell(true, 100, 0, false, CellType.S);
                    Ss.Add(new Vector2Int(i, j));
                }
                else if (col.Equals(new Color(1,1,0,1))) //Yellow
                {
                    mapCells[i, j] = new Cell(true, 0, 100, false, CellType.N);
                    Ns.Add(new Vector2Int(i, j));
                }
            }
        }
    }
        
    /** Execution of the simulation. */
    void Simulation()
    {
        Debug.Log("Simulation running "+t+" "+ localFiftyStepsTime+" "+ fiftyStepsPhase);
        if(fiftyStepsPhase)
        {
            ExecuteFiftyStepsPhase();
        }

        if(!fiftyStepsPhase)
        {
            Debug.Log("Other stuff applied");
            for (int k = Ns.Count - 1; k >= 0; k--)
            {
                Vector2Int v = Ns[k];
                int i = v.x;
                int j = v.y;

                Cell cell = mapCells[i, j];

                if (cell.type == CellType.N && cell.PM >= ThPM)
                {
                    //Connect these Ns with the SP
                    ConnectNToNearestS(i, j);

                    //Change NS into SP
                    cell.type = CellType.S;
                    cell.PM = 100;
                    cell.CHA = 0;
                    Ns.RemoveAt(k);
                    Ss.Add(v);

                    SetLatestEncapsulatedNS(v);
                }
            }

            if(t <= 5000)
            {
                if (t == 5000)
                {
                    Debug.Log("We reached 5000, changing SP in NS");
                    //Change all NS and SP as NS
                    for (int k = Ss.Count - 1; k >= 0; k--)
                    {
                        Vector2Int v = Ss[k];
                        int i = v.x;
                        int j = v.y;

                        Cell cell = mapCells[i, j];

                        cell.type = CellType.N;
                        cell.CHA = 100;
                        Ss.RemoveAt(k);
                        Ns.Add(v);
                    }

                    //Il penultimo NS incapsulato diventa il nuovo SP
                    mapCells[GetSecondToLastCoveredNS().x, GetSecondToLastCoveredNS().y].type = CellType.S;
                    mapCells[GetSecondToLastCoveredNS().x, GetSecondToLastCoveredNS().y].PM = 100;
                    mapCells[GetSecondToLastCoveredNS().x, GetSecondToLastCoveredNS().y].CHA = 0;
                    Ns.Remove(GetSecondToLastCoveredNS());
                    Ss.Add(GetSecondToLastCoveredNS());
                }
                
                ResetFiftyStepsPhase();
                ExecuteFiftyStepsPhase();
            } else if(t < 10000)
            {
                ResetFiftyStepsPhase();
                ExecuteFiftyStepsPhase();
            } else
            {
                simulationRunning = false;
            } 
        }

        t++;
    }

    void ExecuteFiftyStepsPhase()
    {
        if (localFiftyStepsTime < 50)
        {
            ApplyDiffusionEquations();
            localFiftyStepsTime++;
        }
        else
        {
            fiftyStepsPhase = false;
        }
    }

    private void ConnectNToNearestS(int i, int j)
    {
        mapCells[i, j].TE = true;

        if (mapCells[i, j].type != CellType.S)
        {
            int x;
            int y;

            GetHighestNeighbourPM(i, j, out x, out y);

            ConnectNToNearestS(x, y);
        }
    }

    private float GetHighestNeighbourPM(int i, int j, out int x, out int y)
    {
        x = -1;
        y = -1;

        float highestPM = float.MinValue;

        for (int a = i - 1; a < i + 2; a++)
        {
            for (int b = j - 1; b < j + 2; b++)
            {
                float PM = GetPM(a, b);
                if (PM > highestPM)
                {
                    highestPM = PM;
                    x = a;
                    y = b;
                }
            }
        }

        return highestPM;
    }

    private Vector2Int GetSecondToLastCoveredNS()
    {
        return secondToLastEncapsulatedNS;
    }

    private void SetLatestEncapsulatedNS(Vector2Int cellPos)
    {
        if(lastEncapsulatedNS != null)
        {
            secondToLastEncapsulatedNS = lastEncapsulatedNS;
        }

        lastEncapsulatedNS = cellPos;
    }

    private void ResetFiftyStepsPhase()
    {
        fiftyStepsPhase = true;
        localFiftyStepsTime = 0;
    }

    /** Equazioni di diffusione. */
    void ApplyDiffusionEquations()
    {
        Cell[,] newMap = CreateNewCellMap(mapSizeX, mapSizeY);

        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                //Calcolo PM
                if (mapCells[i, j].type != CellType.S && mapCells[i, j].type != CellType.U)
                {
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

                    float PA_WEST = (CalculatePA(GetCHA(i - 1, j), GetCHA(i + 1, j), values));
                    float PA_SOUTH = (CalculatePA(GetCHA(i, j - 1), GetCHA(i, j + 1), values));
                    float PA_EAST = (CalculatePA(GetCHA(i + 1, j), GetCHA(i - 1, j), values));
                    float PA_NORTH = (CalculatePA(GetCHA(i, j + 1), GetCHA(i, j - 1), values));
                    float PA_SOUTHWEST = (CalculatePA(GetCHA(i - 1, j - 1), GetCHA(i + 1, j + 1), values));
                    float PA_SOUTHEAST = (CalculatePA(GetCHA(i + 1, j - 1), GetCHA(i - 1, j + 1), values));
                    float PA_NORTHWEST = (CalculatePA(GetCHA(i - 1, j + 1), GetCHA(i + 1, j - 1), values));
                    float PA_NORTHEAST = (CalculatePA(GetCHA(i + 1, j + 1), GetCHA(i - 1, j - 1), values));

                    float PMvNN = ((1 + PA_WEST) * GetPM(i - 1, j) - (GetAA(i - 1, j) ? 1 : 0) * GetPM(i, j))
                        + ((1 + PA_SOUTH) * GetPM(i, j - 1) - (GetAA(i, j - 1) ? 1 : 0) * GetPM(i, j))
                        + ((1 + PA_EAST) * GetPM(i + 1, j) - (GetAA(i + 1, j) ? 1 : 0) * GetPM(i, j))
                        + ((1 + PA_NORTH) * GetPM(i, j + 1) - (GetAA(i, j + 1) ? 1 : 0) * GetPM(i, j));
                    float PMeMN = ((1 + PA_SOUTHWEST) * GetPM(i - 1, j - 1) - (GetAA(i - 1, j - 1) ? 1 : 0) * GetPM(i, j))
                        + ((1 + PA_SOUTHEAST) * GetPM(i + 1, j - 1) - (GetAA(i + 1, j - 1) ? 1 : 0) * GetPM(i, j))
                        + ((1 + PA_NORTHWEST) * GetPM(i - 1, j + 1) - (GetAA(i - 1, j + 1) ? 1 : 0) * GetPM(i, j))
                        + ((1 + PA_NORTHEAST) * GetPM(i - 1, j + 1) - (GetAA(i - 1, j + 1) ? 1 : 0) * GetPM(i, j));

                    newMap[i, j].PM = GetPM(i, j) + PMP1 * (PMvNN + PMP2 * PMeMN);

                    /*DEBUG ONLY if(mapCells[i, j].type == CellType.N)
                    {
                        Debug.Log("N AT "+i+" "+j+" "+ newMap[i, j].PM);
                    }*/
                } else
                {
                    newMap[i, j].PM = mapCells[i, j].PM;
                }

                //Calcolo CHA
                if (mapCells[i, j].type != CellType.N && mapCells[i, j].type != CellType.U)
                {
                    float CHAvNN = ((GetCHA(i - 1, j)) - (GetAA(i - 1, j) ? 1 : 0) * GetCHA(i, j))
                        + ((GetCHA(i, j - 1)) - (GetAA(i, j - 1) ? 1 : 0) * GetCHA(i, j))
                        + ((GetCHA(i + 1, j)) - (GetAA(i + 1, j) ? 1 : 0) * GetCHA(i, j))
                        + ((GetCHA(i, j + 1)) - (GetAA(i, j + 1) ? 1 : 0) * GetCHA(i, j));
                    float CHAeMN = ((GetCHA(i - 1, j - 1)) - (GetAA(i - 1, j - 1) ? 1 : 0) * GetCHA(i, j))
                        + ((GetCHA(i + 1, j - 1)) - (GetAA(i + 1, j - 1) ? 1 : 0) * GetCHA(i, j))
                        + ((GetCHA(i - 1, j + 1)) - (GetAA(i - 1, j + 1) ? 1 : 0) * GetCHA(i, j))
                        + ((GetCHA(i + 1, j + 1)) - (GetAA(i + 1, j + 1) ? 1 : 0) * GetCHA(i, j));
                    newMap[i, j].CHA = CON * (GetCHA(i, j) + CAP1 * (CHAvNN + CAP2 * CHAeMN));
                } else
                {
                    newMap[i, j].CHA = mapCells[i, j].CHA;
                }
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
                Color col = tex.GetPixel(i, j);
                tilemap.SetTile(new Vector3Int(i, j, 0), tile);
                SetTileColour(col, new Vector3Int(i, j, 0));
            }
        }
    }

    /** Aggiorna le tile della TileMap con i colori corretti. */
    void UpdateTiles()
    {
        Tilemap tilemap = this.GetComponent<Tilemap>();
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                CellType type = mapCells[i, j].type;
                float PMInRange01 = mapCells[i, j].PM / 100f;
                float CHAInRange01 = mapCells[i, j].CHA / 100f;

                if (type == CellType.U)
                {
                    SetTileColour(new Color(1, 0, 0, 1), new Vector3Int(i, j, 0));
                }
                else if(type == CellType.S)
                {
                    SetTileColour(new Color(0, 0, 0, 1), new Vector3Int(i, j, 0));
                }
                else if (type == CellType.N)
                {
                    SetTileColour(new Color(1, 1, 0, 1), new Vector3Int(i, j, 0));
                }
                else if (mapCells[i, j].TE)
                {
                    SetTileColour(new Color(0, 0, 1, 1), new Vector3Int(i, j, 0));
                }
                else
                {
                    SetTileColour(new Color(Mathf.Lerp(0.53f, 1, PMInRange01), Mathf.Lerp(0.8f, 0.27f, PMInRange01), Mathf.Lerp(0.98f, 0, PMInRange01), 1), new Vector3Int(i, j, 0));
                    //DEBUG ONLY FOR CHA SetTileColour(new Color(Mathf.Lerp(0.53f, 1, CHAInRange01), Mathf.Lerp(0.8f, 0.27f, CHAInRange01), Mathf.Lerp(0.98f, 0, CHAInRange01), 1), new Vector3Int(i, j, 0));
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
