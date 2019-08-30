using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SimulationManager : MonoBehaviour
{   
    // Attached components
    // Per essere usata, l'immagine .png deve aggiungere l'estensione .bytes
    public TextAsset imageAsset;
    public Tile tile;
    public Camera camera;

    Texture2D tex;

    int mapSizeX;
    int mapSizeY;

    public Cell[,] mapCells;
    public List<Vector2Int> Ss = new List<Vector2Int>();
    public List<Vector2Int> Ns = new List<Vector2Int>();

    public Payload payload;

    public float defaultPMForS = 100;
    public float defaultCHAForN = 100;

    public float CON = 0.95f;
    public float PAP = 0.7f;
    public float PMP1 = 0.08f;
    public float PMP2 = 0.01f;
    public float CAP1 = 0.05f;
    public float CAP2 = 0.01f;
    public float ThPM = 20;//0.2f;

    public int minAgeToDryOut = 1000;

    int t = 0;

    /** False if the simulation has finished. */
    bool simulationRunning;

    /** True if we are doing the 50 steps where we apply the diffusion equations. */
    bool fiftyStepsPhase;
    /** Local timer to count 50 steps. */
    int localFiftyStepsTime;

    Vector2Int lastEncapsulatedNS;
    Vector2Int secondToLastEncapsulatedNS;

    float biggestCHAvalue;
    float smallestCHAvalue;
    float biggestPMvalue;
    float smallestPMvalue;
    int posIbiggestPMValue;
    int posJbiggestPMValue;

    /** Keep it -1 if you want the simulation to go eternally. */
    public int tToStop = 10000;

    bool testMatteo = true;

    public float totalPM = 0;

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
        //TestProgression();
        if (simulationRunning)
        {
            if (testMatteo)
            {
                SimulationMatteo();
            }
            else
            {
                Simulation();
            }
            UpdateTiles();

            //DEBUG ONLY:
            if(t == tToStop)
                simulationRunning = false;
        }
    }

    /** Call this every time you want to restart the simulation. */
    public void Initialization()
    {
        LoadTextureMap();
        InitCellMap();
        t = 0;
        ResetFiftyStepsPhase();

        if (testMatteo)
        {
            foreach (Vector2Int s in Ss)
            {
                mapCells[s.x, s.y].PM = defaultPMForS;
            }
        }

        simulationRunning = true;
        DrawTiles();
    }

    /** Load the map as texture so we can access each pixel */
    void LoadTextureMap()
    {
        tex = new Texture2D(2, 2);
        tex.LoadImage(imageAsset.bytes);
        mapSizeX = tex.width;
        mapSizeY = tex.height;
        // Center and resize camera so the entire map fills the viewport
        CameraManager cameraManager = camera.GetComponent<CameraManager>();
        /*
        To get a decent viewport:
        Position should be size/2, size/2
        Size should be size/2 + size/10
        */
        cameraManager.MoveCamera(mapSizeX/2, mapSizeY/2);
        camera.orthographicSize = mapSizeX/2 + mapSizeX/20;
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
                    Debug.Log("Starting S is at " + i + " " + j);
                    mapCells[i, j] = new Cell(true, defaultPMForS, 0, false, CellType.S);
                    Ss.Add(new Vector2Int(i, j));
                }
                else if (col.Equals(new Color(1,1,0,1))) //Yellow
                {
                    mapCells[i, j] = new Cell(true, 0, defaultCHAForN, false, CellType.N);
                    Ns.Add(new Vector2Int(i, j));
                }
            }
        }
    }

    void SimulationMatteo()
    {
        Debug.Log("Simulation Matteo running " + t + " " + localFiftyStepsTime + " " + fiftyStepsPhase);

        ApplyMatteoDiffusionEquations();

        for (int k = Ns.Count - 1; k >= 0; k--)
        {
            Vector2Int v = Ns[k];
            int i = v.x;
            int j = v.y;

            Cell cell = mapCells[i, j];

            if (cell.type == CellType.N && cell.PM >= ThPM && cell.direction != Dir.NONE)
            {

                Debug.Log("N connecting " + i + " " + j + " " + cell.PM);
                //Connect these Ns with the SP
                ConnectNToNearestS(i, j);

                //Change NS into SP
                cell.type = CellType.S;
                cell.PM += defaultPMForS;
                cell.CHA = 0;
                Ns.RemoveAt(k);
                Ss.Add(v);
            }
        }

        t++;
    }

    void ApplyMatteoDiffusionEquations()
    {
        Cell[,] newMap = CreateNewCellMap(mapSizeX, mapSizeY);
        totalPM = 0;

        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                if (mapCells[i, j].type != CellType.U)
                {
                    //CALCOLO DIREZIONE
                    if (mapCells[i, j].PM >= 1 && mapCells[i, j].direction == Dir.NONE)
                    {
                        float maxPMNeighbour = 0;
                        int neighbourCoordX = -1;
                        int neighbourCoordY = -1;

                        for (int x = i - 1; x < (i + 2); x++)
                        {
                            for (int y = j - 1; y < (j + 2); y++)
                            {
                                if (GetAA(x, y) && mapCells[x, y].PM > maxPMNeighbour && !(x == i && y == j))
                                {
                                    maxPMNeighbour = mapCells[x, y].PM;
                                    neighbourCoordX = x;
                                    neighbourCoordY = y;
                                }
                            }
                        }

                        if (maxPMNeighbour > 0)
                        {
                            if (neighbourCoordX == i - 1)
                            {
                                if (neighbourCoordY == j - 1)
                                {
                                    mapCells[i, j].direction = Dir.SW;
                                }
                                else if (neighbourCoordY == j)
                                {
                                    mapCells[i, j].direction = Dir.W;
                                }
                                else if (neighbourCoordY == j + 1)
                                {
                                    mapCells[i, j].direction = Dir.NW;
                                }
                            }
                            else if (neighbourCoordX == i)
                            {
                                if (neighbourCoordY == j - 1)
                                {
                                    mapCells[i, j].direction = Dir.S;
                                }
                                else if (neighbourCoordY == j)
                                {
                                    mapCells[i, j].direction = Dir.NONE; //NON DOVREBBE SUCCEDERE
                                }
                                else if (neighbourCoordY == j + 1)
                                {
                                    mapCells[i, j].direction = Dir.N;
                                }
                            }
                            else //+1
                            {
                                if (neighbourCoordY == j - 1)
                                {
                                    mapCells[i, j].direction = Dir.SE;
                                }
                                else if (neighbourCoordY == j)
                                {
                                    mapCells[i, j].direction = Dir.E;
                                }
                                else if (neighbourCoordY == j + 1)
                                {
                                    mapCells[i, j].direction = Dir.NE;
                                }
                            }
                        }
                    }

                    //CALCOLO PM CON CONSERVAZIONE
                    float cellPM = GetPM(i, j);
                    
                    for (int x = i - 1; x < (i + 2); x++)
                    {
                        for (int y = j - 1; y < (j + 2); y++)
                        {
                            if (x == i && y == j)
                            {
                                //Nothing with the current cell
                            }
                            else if (GetAA(x, y))
                            {
                                if (GetCHA(x, y) == 0 && GetCHA(i, j) == 0 && !mapCells[i,j].didHaveCha && !mapCells[x, y].didHaveCha) //Uniform expansion case
                                {
                                    if (GetPM(i, j) > GetPM(x, y) && GetPM(i, j) >= 12)
                                    {
                                        if (x == i || y == j) //NN neighbours
                                        {
                                            cellPM -= 2;
                                        }
                                        else //MN neighbours
                                        {
                                            cellPM -= 1;
                                        }
                                    }
                                    else if (GetPM(i, j) < GetPM(x, y) && GetPM(x, y) >= 12)
                                    {
                                        if (x == i || y == j) //NN neighbours
                                        {
                                            cellPM += 2;
                                        }
                                        else //MN neighbours
                                        {
                                            cellPM += 1;
                                        }
                                    }
                                }
                                else //CHA exists case
                                {
                                    if(GetCHA(i, j) > 0)
                                        mapCells[i, j].didHaveCha = true;

                                    if (x == i || y == j) //NN neighbours
                                    {
                                        if (GetCHA(x, y) < GetCHA(i, j))
                                        {
                                            if (GetPM(x, y) >= 12)
                                            {
                                                cellPM += 2;
                                            }
                                        }
                                        else if (GetCHA(x, y) > GetCHA(i, j))
                                        {
                                            if (GetPM(i, j) >= 12)
                                            {
                                                cellPM -= 2;
                                            }
                                        }
                                    }
                                    else //MN neighbours
                                    {
                                        if (GetCHA(x, y) < GetCHA(i, j))
                                        {
                                            if (GetPM(x, y) >= 12)
                                            {
                                                cellPM += 1;
                                            }
                                        }
                                        else if (GetCHA(x, y) > GetCHA(i, j))
                                        {
                                            if (GetPM(i, j) >= 12)
                                            {
                                                cellPM -= 1;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    totalPM += cellPM;
                    newMap[i, j].PM = cellPM;
                }
                else
                {
                    newMap[i, j].PM = mapCells[i, j].PM;
                }

                //Calcolo CHA
                if (mapCells[i, j].type != CellType.N && mapCells[i, j].type != CellType.U)
                {
                    if (mapCells[i, j].age > minAgeToDryOut)
                    {
                        newMap[i, j].CHA = 0;
                    }
                    else
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

                        if (newMap[i, j].CHA > defaultCHAForN)
                        {
                            newMap[i, j].CHA = defaultCHAForN;
                        }
                        if (newMap[i, j].CHA < 0)
                        {
                            newMap[i, j].CHA = 0;
                        }
                    }
                }
                else
                {
                    newMap[i, j].CHA = mapCells[i, j].CHA;
                }

                if(mapCells[i, j].PM > 0) //Must be touched by the mold
                    mapCells[i, j].age++;
            }
        }

        //Update the map
        UpdateMapWithNewDiffusionValues(newMap);

        //SHRINKING PROCESS 
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                for (int x = i - 1; x < (i + 2); x++)
                {
                    for (int y = j - 1; y < (j + 2); y++)
                    {
                        if (x == i && y == j)
                        {
                            //Nothing with the current cell
                        }
                        else if (GetAA(x, y))
                        {
                            if (GetAge(x, y) > minAgeToDryOut
                                && IsCellAInDirectionOfCellB(i, j, x, y)
                                && !mapCells[x, y].TE
                                && mapCells[x, y].type != CellType.N
                                && mapCells[x, y].type != CellType.S)
                            {
                                newMap[i,j].PM += GetPM(x, y);
                            }
                            else if (GetAge(i, j) > minAgeToDryOut
                                && IsCellAInDirectionOfCellB(x, y, i, j)
                                && !mapCells[i, j].TE
                                && mapCells[i, j].type != CellType.N
                                && mapCells[i, j].type != CellType.S)
                            {
                                newMap[i,j].PM -= GetPM(i, j);
                            }
                        }
                    }
                }
            }
        }

        //Update the map
        UpdateMapWithNewDiffusionValues(newMap);

        Debug.Log("TOTALPM " + totalPM+" "+ (Math.Round(totalPM / defaultPMForS)) + " "+(totalPM%defaultPMForS));
    }

    private bool IsCellAInDirectionOfCellB(int Ax, int Ay, int Bx, int By)
    {
        if (mapCells[Bx, By].direction == Dir.N && Ax == Bx && Ay == By+1)
            return true;
        else if (mapCells[Bx, By].direction == Dir.NW && Ax == Bx - 1 && Ay == By + 1)
            return true;
        else if (mapCells[Bx, By].direction == Dir.NE && Ax == Bx + 1 && Ay == By + 1)
            return true;
        else if (mapCells[Bx, By].direction == Dir.W && Ax == Bx - 1 && Ay == By)
            return true;
        else if (mapCells[Bx, By].direction == Dir.E && Ax == Bx + 1 && Ay == By)
            return true;
        else if (mapCells[Bx, By].direction == Dir.S && Ax == Bx && Ay == By - 1)
            return true;
        else if (mapCells[Bx, By].direction == Dir.SW && Ax == Bx - 1 && Ay == By - 1)
            return true;
        else if (mapCells[Bx, By].direction == Dir.SE && Ax == Bx + 1 && Ay == By - 1)
            return true;

        return false;
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

            
            if(Ns.Count == 0) //All N connected?
            {
                Debug.Log("No N left, stopping the simulation.");
                simulationRunning = false;
                return;
            }

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
                    cell.PM = defaultPMForS;
                    cell.CHA = 0;
                    Ns.RemoveAt(k);
                    Ss.Add(v);

                    /* DEBUG Passo 5000 se i nodi sono stati tutti raggiunti. Loop a 5050
                    SetLatestEncapsulatedNS(v);
                    if (Ns.Count == 0)
                        t = 5000;
                    */
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
                        cell.CHA = defaultCHAForN;
                        Ss.RemoveAt(k);
                        Ns.Add(v);
                    }

                    //Il penultimo NS incapsulato diventa il nuovo SP
                    mapCells[GetSecondToLastCoveredNS().x, GetSecondToLastCoveredNS().y].type = CellType.S;
                    mapCells[GetSecondToLastCoveredNS().x, GetSecondToLastCoveredNS().y].PM = defaultPMForS;
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

    int antiCrashCounter = 0;
    private void ConnectNToNearestS(int i, int j)
    {
        Debug.Log("TUBO CONNESSO " + i + " " + j + " " + mapCells[i, j].CHA + " " + mapCells[i, j].direction + " " + mapCells[i, j].type);

        mapCells[i, j].TE = true;
        
        if (mapCells[i, j].type != CellType.S)
        {
            int x;
            int y;

            if(testMatteo)
                GetNeighbourBasedOnDirection(i, j, out x, out y);
            else
                GetHighestNeighbourPM(i, j, out x, out y);

            if (antiCrashCounter < 500)
            {
                Debug.Log("Connecting, highest near PM: "+ x+" "+y);
                antiCrashCounter++; 
                ConnectNToNearestS(x, y);
            } else
            {
                Debug.Log("Infinite Loop encountered: "+ antiCrashCounter);
                simulationRunning = false;
            }
        }
    }

    private void GetNeighbourBasedOnDirection(int i, int j, out int x, out int y)
    {
        x = -1;
        y = -1;

        Dir dir = mapCells[i, j].direction;

        if(dir == Dir.SW)
        {
            x = i - 1;
            y = j - 1;
        }
        else if (dir == Dir.W)
        {
            x = i - 1;
            y = j;
        }
        else if (dir == Dir.NW)
        {
            x = i - 1;
            y = j + 1;
        }
        else if (dir == Dir.N)
        {
            x = i;
            y = j + 1;
        }
        else if (dir == Dir.S)
        {
            x = i;
            y = j - 1;
        }
        else if (dir == Dir.SE)
        {
            x = i + 1;
            y = j - 1;
        }
        else if (dir == Dir.E)
        {
            x = i + 1;
            y = j;
        }
        else if (dir == Dir.NE)
        {
            x = i + 1;
            y = j + 1;
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
                if (a == i && b == j)
                {
                    continue;
                } else { 
                    float PM = GetPM(a, b);
                    if (PM > highestPM)
                    {
                        highestPM = PM;
                        x = a;
                        y = b;
                    }
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
                    GetCHA(i - 1, j),
                    GetCHA(i, j - 1),
                    GetCHA(i + 1, j),
                    GetCHA(i, j + 1),
                    GetCHA(i - 1, j - 1),
                    GetCHA(i + 1, j - 1),
                    GetCHA(i - 1, j + 1),
                    GetCHA(i + 1, j + 1) };

                    float maxCHA = GetMax(values);

                    float PA_WEST = 0;
                    float PA_SOUTH = 0;
                    float PA_EAST = 0;
                    float PA_NORTH = 0;
                    float PA_SOUTHWEST = 0;
                    float PA_SOUTHEAST = 0;
                    float PA_NORTHEAST = 0;
                    float PA_NORTHWEST = 0;
                    
                    if (maxCHA == GetCHA(i - 1, j))
                    {
                        PA_WEST = PAP;
                        PA_EAST = -PAP;
                    } else if (maxCHA == GetCHA(i, j - 1))
                    {
                        PA_SOUTH = PAP;
                        PA_NORTH = -PAP;
                    } else if (maxCHA == GetCHA(i + 1, j))
                    {
                        PA_EAST = PAP;
                        PA_WEST = -PAP;
                    } else if (maxCHA == GetCHA(i, j + 1))
                    {
                        PA_NORTH = PAP;
                        PA_SOUTH = -PAP;
                    } else if (maxCHA == GetCHA(i - 1, j - 1))
                    {
                        PA_SOUTHWEST = PAP;
                        PA_NORTHEAST = -PAP;
                    } else if (maxCHA == GetCHA(i + 1, j - 1))
                    {
                        PA_SOUTHEAST = PAP;
                        PA_NORTHWEST = -PAP;
                    } else if (maxCHA == GetCHA(i - 1, j + 1))
                    {
                        PA_NORTHWEST = PAP;
                        PA_SOUTHEAST = -PAP;
                    } else if (maxCHA == GetCHA(i - 1, j + 1))
                    {
                        PA_NORTHEAST = PAP;
                        PA_SOUTHWEST = -PAP;
                    }

                    float PMvNN = ((1 + PA_WEST) * GetPM(i - 1, j) - (GetAA(i - 1, j) ? 1 : 0) * GetPM(i, j))
                        + ((1 + PA_SOUTH) * GetPM(i, j - 1) - (GetAA(i, j - 1) ? 1 : 0) * GetPM(i, j))
                        + ((1 + PA_EAST) * GetPM(i + 1, j) - (GetAA(i + 1, j) ? 1 : 0) * GetPM(i, j))
                        + ((1 + PA_NORTH) * GetPM(i, j + 1) - (GetAA(i, j + 1) ? 1 : 0) * GetPM(i, j));
                    float PMeMN = ((1 + PA_SOUTHWEST) * GetPM(i - 1, j - 1) - (GetAA(i - 1, j - 1) ? 1 : 0) * GetPM(i, j))
                        + ((1 + PA_SOUTHEAST) * GetPM(i + 1, j - 1) - (GetAA(i + 1, j - 1) ? 1 : 0) * GetPM(i, j))
                        + ((1 + PA_NORTHWEST) * GetPM(i - 1, j + 1) - (GetAA(i - 1, j + 1) ? 1 : 0) * GetPM(i, j))
                        + ((1 + PA_NORTHEAST) * GetPM(i + 1, j + 1) - (GetAA(i + 1, j + 1) ? 1 : 0) * GetPM(i, j));

                    newMap[i, j].PM = GetPM(i, j) + PMP1 * (PMvNN + PMP2 * PMeMN);
                }
                else
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

                    if (newMap[i, j].CHA > defaultCHAForN)
                    {
                        newMap[i, j].CHA = defaultCHAForN;
                    }
                    if (newMap[i, j].CHA < 0)
                    {
                        newMap[i, j].CHA = 0;
                    }
                }
                else
                {
                    newMap[i, j].CHA = mapCells[i, j].CHA;
                }
            }
        }

        //Update the map
        UpdateMapWithNewDiffusionValues(newMap);
    }

    /*private float DistFromNearestS(int i, int j)
    {
        float res = float.MaxValue;
        foreach(Vector2Int s in Ss)
        {
            float dist = Vector2.Distance(s, new Vector2Int(i, j));
            if(dist < res)
            {
                res = dist;
            }
        }

        return res;
    }*/

    private void UpdateMapWithNewDiffusionValues(Cell[,] newMap)
    {
        smallestCHAvalue = float.MaxValue;
        biggestCHAvalue = float.MinValue;
        smallestPMvalue = float.MaxValue;
        biggestPMvalue = float.MinValue;

        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                mapCells[i, j].PM = newMap[i, j].PM;
                mapCells[i, j].CHA = newMap[i, j].CHA;

                if (smallestCHAvalue > mapCells[i, j].CHA)
                    smallestCHAvalue = mapCells[i, j].CHA;

                if (biggestCHAvalue < mapCells[i, j].CHA)
                    biggestCHAvalue = mapCells[i, j].CHA;

                if (smallestPMvalue > mapCells[i, j].PM)
                    smallestPMvalue = mapCells[i, j].PM;

                if (biggestPMvalue < mapCells[i, j].PM)
                {
                    biggestPMvalue = mapCells[i, j].PM;
                    posIbiggestPMValue = i;
                    posJbiggestPMValue = j;
                }
            }
        }

        Debug.Log("BIGGEST PM IS "+ biggestPMvalue+" at "+ posIbiggestPMValue+" "+ posJbiggestPMValue);
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

    private CellType GetType(int i, int j)
    {
        if (i < 0 || i >= mapSizeX || j < 0 || j >= mapSizeY || mapCells[i, j].type == CellType.U)
            return CellType.U;
        else
            return mapCells[i, j].type;
    }

    float GetCHA(int i, int j)
    {
        if (i < 0 || i >= mapSizeX || j < 0 || j >= mapSizeY || mapCells[i, j].type == CellType.U)
            return 0;
        else
            return mapCells[i, j].CHA;
    }

    float GetPM(int i, int j)
    {
        if (i < 0 || i >= mapSizeX || j < 0 || j >= mapSizeY || mapCells[i,j].type == CellType.U)
        {
            return 0;
        }
        else
        {
            return mapCells[i, j].PM;
        }
    }

    bool GetAA(int i, int j)
    {
        if (i < 0 || i >= mapSizeX || j < 0 || j >= mapSizeY || mapCells[i, j].type == CellType.U)
            return false;
        else
            return mapCells[i, j].AA;
    }

    int GetAge(int i, int j)
    {
        if (i < 0 || i >= mapSizeX || j < 0 || j >= mapSizeY || mapCells[i, j].type == CellType.U)
            return 0;
        else
            return mapCells[i, j].age;
    }

    /*float CalculatePA(float cha1, float oppositeCHA, float[] values, int i , int j) {
        if (i < 0 || i >= mapSizeX || j < 0 || j >= mapSizeY || mapCells[i, j].type == CellType.U)
            return 0;

        float max_value = GetMax(values, false);
        if (cha1 == max_value)
            return PAP;
        else if (oppositeCHA == max_value)
            return -PAP;
        else
            return 0;
    }*/

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
        //Debug.Log("PMs "+ smallestPMvalue+" "+ biggestPMvalue+" || "+ posIbiggestPMValue+" "+ posJbiggestPMValue);
        Tilemap tilemap = this.GetComponent<Tilemap>();
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                CellType type = mapCells[i, j].type;
                float PMInRange01 = mapCells[i, j].PM / (biggestPMvalue - smallestPMvalue);
                //float PMInRange01 = mapCells[i, j].PM / defaultPMForS;
                float CHAInRange01 = mapCells[i, j].CHA / (biggestCHAvalue - smallestCHAvalue);
                //float CHAInRange01 = mapCells[i, j].CHA / defaultCHAForN;

                if (type == CellType.U)
                {
                    SetTileColour(new Color(1, 0, 0, 1), new Vector3Int(i, j, 0));
                }
                /*else if(type == CellType.S)
                {
                    SetTileColour(new Color(0, 0, 0, 1), new Vector3Int(i, j, 0));
                }*/
                else if (type == CellType.N)
                {
                    SetTileColour(new Color(1, 1, 0, 1), new Vector3Int(i, j, 0));
                }
                /*else if (mapCells[i, j].TE)
                {
                    //SetTileColour(new Color(0, 0, 1, 1), new Vector3Int(i, j, 0));
                    SetTileColour(new Color(1, 0, 1, 1), new Vector3Int(i, j, 0));
                }*/
                else
                {
                    //SetTileColour(new Color(Mathf.Lerp(0.53f, 1, PMInRange01), Mathf.Lerp(0.8f, 0.27f, PMInRange01), Mathf.Lerp(0.98f, 0, PMInRange01), 1), new Vector3Int(i, j, 0));

                    if (testMatteo)
                    {
                        //COLORED FOR CONSERVATION MAP
                        if (mapCells[i, j].PM == 0)
                        {
                            SetTileColour(new Color(1, 1, 1, 1), new Vector3Int(i, j, 0));
                        }
                        else if (mapCells[i, j].PM < 12)
                        {
                            SetTileColour(new Color(0, 0, 1, 1), new Vector3Int(i, j, 0));
                        }
                        else if (mapCells[i, j].PM < 50)
                        {
                            SetTileColour(new Color(0, 0.75f, 1, 1), new Vector3Int(i, j, 0));
                        }
                        else if (mapCells[i, j].PM < 75)
                        {
                            SetTileColour(new Color(0, 1, 0, 1), new Vector3Int(i, j, 0));
                        }
                        else if (mapCells[i, j].PM < 100)
                        {
                            SetTileColour(new Color(1, 1, 0, 1), new Vector3Int(i, j, 0));
                        }
                        else if (mapCells[i, j].PM >= 100)
                        {
                            SetTileColour(new Color(1, 0.27f, 0, 1), new Vector3Int(i, j, 0));
                        }
                    }
                    else
                    {
                        //COLORED FOR NO CONSERVATION MAP
                        if(mapCells[i, j].PM == 0)
                        {
                            SetTileColour(new Color(1,1,1,1), new Vector3Int(i, j, 0));
                        } else if (mapCells[i, j].PM < 0.1)
                        {
                            SetTileColour(new Color(0, 0, 1, 1), new Vector3Int(i, j, 0));
                        } else if (mapCells[i, j].PM < 1)
                        {
                            SetTileColour(new Color(0, 0.75f, 1, 1), new Vector3Int(i, j, 0));
                        } else if (mapCells[i, j].PM < 10)
                        {
                            SetTileColour(new Color(0, 1, 0, 1), new Vector3Int(i, j, 0));
                        } else if (mapCells[i, j].PM < 100)
                        {
                            SetTileColour(new Color(1, 1, 0, 1), new Vector3Int(i, j, 0));
                        } else if (mapCells[i, j].PM >= 100)
                        {
                            SetTileColour(new Color(1, 0.27f, 0, 1), new Vector3Int(i, j, 0));
                        }
                    }

                    //DEBUG DIRECTION MAP
                    /*if (mapCells[i, j].direction == Dir.N)
                    {
                        SetTileColour(new Color(0, 0, 0, 1), new Vector3Int(i, j, 0));
                    }
                    else if (mapCells[i, j].direction == Dir.NW)
                    {
                        SetTileColour(new Color(1, 0, 0, 1), new Vector3Int(i, j, 0));
                    }
                    else if (mapCells[i, j].direction == Dir.NE)
                    {
                        SetTileColour(new Color(0, 1, 0, 1), new Vector3Int(i, j, 0));
                    }
                    else if (mapCells[i, j].direction == Dir.W)
                    {
                        SetTileColour(new Color(0, 0, 1, 1), new Vector3Int(i, j, 0));
                    }
                    else if (mapCells[i, j].direction == Dir.E)
                    {
                        SetTileColour(new Color(1, 1, 0, 1), new Vector3Int(i, j, 0));
                    }
                    else if (mapCells[i, j].direction == Dir.S)
                    {
                        SetTileColour(new Color(1, 0, 1, 1), new Vector3Int(i, j, 0));
                    }
                    else if (mapCells[i, j].direction == Dir.SW)
                    {
                        SetTileColour(new Color(0, 1, 1, 1), new Vector3Int(i, j, 0));
                    }
                    else if (mapCells[i, j].direction == Dir.SE)
                    {
                        SetTileColour(new Color(0.5f, 0.5f, 0.5f, 1), new Vector3Int(i, j, 0));
                    }*/
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
