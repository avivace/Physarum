using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;


public class SimulationManager : MonoBehaviour
{
	//Payload for UI data exchange
	public Payload payload;

	/*** Attached components ***/
	// Per essere usata, l'immagine .png deve aggiungere l'estensione .bytes
	public TextAsset imageAsset;
	public Tile tile;
	public Camera camera;

	/*** Maps variables ***/
	Texture2D tex;

	public TextAsset[] maps;
	public string[] mapsFileNames;

	int mapSizeX;
	int mapSizeY;

	public Cell[,] mapCells;
	public List<Vector2Int> Ss = new List<Vector2Int>();
	public List<Vector2Int> Ns = new List<Vector2Int>();

	/*** Parameters for simulation ***/

	// 0 for the paper simulation, 1 for the experimental one
	public int simulationMode = 0; 

	public float defaultPMForS;
	public float defaultCHAForN;

	public float CON;
	public float PAP;
	public float PMP1;
	public float PMP2;
	public float CAP1;
	public float CAP2;
	public float ThPM;

	public int minAgeToDryOut;

	int currentMapIndex;

	/*** Internal simulation variables ***/
	//Time
	int t = 0;

	/** False if the simulation has finished. */
	bool simulationRunning = false;

	/** True if we are doing the 50 steps where we apply the diffusion equations in paper's simulation. */
	bool fiftyStepsPhase;
	/** Local timer to count 50 steps in paper's simulation. */
	int localFiftyStepsTime;

	//Variables for second part of paper's simulation
	Vector2Int lastEncapsulatedNS;
	Vector2Int secondToLastEncapsulatedNS;

	//Variables used for drawing the tiles
	float biggestCHAvalue;
	float smallestCHAvalue;
	float biggestPMvalue;
	float smallestPMvalue;
	int posIbiggestPMValue;
	int posJbiggestPMValue;

	//Total mass of the mould during the simulation, automatically calculated
	float totalPM = 0;

    //Counter for avoiding infinite loops when trying to create tubes
    int antiCrashCounter = 0;

    int defaultMap = 1;

    /*** UI handler  methods ***/

    void startpause()
	{
		simulationRunning = !simulationRunning;
		// Force reupdate (gets data up to date if we pause before the update tick)
		Application.ExternalCall("vm.$children[0].unityUpdate", 0, t, Ss.Count, Ns.Count, totalPM, simulationMode);
	}

	void stop()
	{
		simulationRunning = false;
		Initialization();
		Application.ExternalCall("vm.$children[0].unityUpdate", 0, t, Ss.Count, Ns.Count, totalPM, simulationMode);
	}

	void payloadHandler(string jsonString)
	{
		string jsonStringTest = "{\"value1\":\"1\"}";
		Payload payload = JsonUtility.FromJson<Payload>(jsonString);
		Debug.Log(payload.value2);
	}

	void changeFrameRate(int targetFps)
	{
		Application.targetFrameRate = targetFps;
	}

	void selectMap(int mapIndex)
	{
		// Should be called on simulation stopped anyway
		simulationRunning = false;
		imageAsset = maps[mapIndex];
		Initialization();
		Application.ExternalCall("vm.$children[0].updateMaps", String.Join(",", mapsFileNames), mapIndex);
		setDefaultParameters(mapIndex);
		currentMapIndex = mapIndex;
		updateParameters();
	}

	// Set parameters from UI
	void setParameters(string values){
		Initialization();
		string[] values2 = values.Split(';');
		defaultPMForS=float.Parse(values2[0]);
		defaultCHAForN=float.Parse(values2[1]);
		CON=float.Parse(values2[2]);
		CAP1=float.Parse(values2[3]);
		CAP2=float.Parse(values2[4]);
		ThPM=float.Parse(values2[5]);
		minAgeToDryOut=int.Parse(values2[6]);
		PMP1=float.Parse(values2[7]);
		PMP2=float.Parse(values2[8]);
		updateParameters();
	}

	// Send updated parameters to UI
	void updateParameters(){
		Application.ExternalCall("vm.$children[0].unityParamUpdate",
			defaultPMForS,
			defaultCHAForN,
			CON,
			CAP1,
			CAP2,
			ThPM,
			minAgeToDryOut,
			PMP1,
			PMP2);
	}

	void setDefaultParameters(int mapIndex){
		// Default parameters per map
		string mapName = mapsFileNames[mapIndex];
		if (simulationMode==1){
			// Default parameters for the Experimental model
			switch(mapName){
				case "central_sp.png":
					defaultPMForS = 10000;
					defaultCHAForN = 100;
					CON = 0.95f;
					CAP1 = 0.05f;
					CAP2 = 0.01f;
					ThPM = 20;
					minAgeToDryOut = 1000;
					break;
				case "generic_maze.png":
					defaultPMForS = 10000;
					defaultCHAForN = 100;
					CON = 0.95f;
					CAP1 = 0.05f;
					CAP2 = 0.01f;
					ThPM = 20;
					minAgeToDryOut = 1000;
					break;
				case "maze_gpgpu.png":
					defaultPMForS = 10000;
					defaultCHAForN = 100;
					CON = 0.95f;
					CAP1 = 0.05f;
					CAP2 = 0.01f;
					ThPM = 20;
					minAgeToDryOut = 2000;
					break;
				case "wsn_network_20.png":
					defaultPMForS = 10000;
					defaultCHAForN = 100;
					CON = 0.95f;
					CAP1 = 0.05f;
					CAP2 = 0.01f;
					ThPM = 20;
					minAgeToDryOut = 1000;
					break;
				case "wsn_network_40.png":
					defaultPMForS = 10000;
					defaultCHAForN = 100;
					CON = 0.95f;
					CAP1 = 0.05f;
					CAP2 = 0.01f;
					ThPM = 20;
					minAgeToDryOut = 1000;
					break;
				case "wsn_network_60.png":
					defaultPMForS = 10000;
					defaultCHAForN = 100;
					CON = 0.95f;
					CAP1 = 0.05f;
					CAP2 = 0.01f;
					ThPM = 20;
					minAgeToDryOut = 1000;
					break;
			}
		} else {
			// Default parameters for the Paper model
			switch(mapName){
				case "central_sp.png":
					defaultPMForS = 100;
					defaultCHAForN = 100;
					CON = 0.95f;
					PAP = 0.8f;
					PMP1 = 0.08f;
					PMP2 = 0.01f;
					CAP1 = 0.05f;
					CAP2 = 0.01f;
					ThPM = 0.2f;
					break;
				case "generic_maze.png":
					defaultPMForS = 30000;
					defaultCHAForN = 30000;
					CON = 0.95f;
					PAP = 0.8f;
					PMP1 = 0.08f;
					PMP2 = 0.01f;
					CAP1 = 0.05f;
					CAP2 = 0.01f;
					ThPM = 0.2f;
					break;
				case "maze_gpgpu.png":
					defaultPMForS = 30000;
					defaultCHAForN = 30000;
					CON = 0.95f;
					PAP = 0.8f;
					PMP1 = 0.08f;
					PMP2 = 0.01f;
					CAP1 = 0.05f;
					CAP2 = 0.01f;
					ThPM = 0.2f;
					break;
				case "wsn_network_20.png":
					defaultPMForS = 100;
					defaultCHAForN = 100;
					CON = 0.95f;
					PAP = 0.8f;
					PMP1 = 0.08f;
					PMP2 = 0.01f;
					CAP1 = 0.05f;
					CAP2 = 0.01f;
					ThPM = 0.2f;
					break;
				case "wsn_network_40.png":
					defaultPMForS = 100;
					defaultCHAForN = 100;
					CON = 0.95f;
					PAP = 0.8f;
					PMP1 = 0.08f;
					PMP2 = 0.01f;
					CAP1 = 0.05f;
					CAP2 = 0.01f;
					ThPM = 0.2f;
					break;
				case "wsn_network_60.png":
					defaultPMForS = 100;
					defaultCHAForN = 100;
					CON = 0.95f;
					PAP = 0.8f;
					PMP1 = 0.08f;
					PMP2 = 0.01f;
					CAP1 = 0.05f;
					CAP2 = 0.01f;
					ThPM = 0.2f;
					break;
			}
		}
		updateParameters();
	}

	// Allow changing the simulation mode
	void selectSimulationMode(int mode)
	{
		simulationMode = mode;
		simulationRunning = false;
		Initialization();
		setDefaultParameters(currentMapIndex); // To force update parameters when changing the simulation mode
		updateParameters();
	}

	// Called by either UI (single step) or by Update when the simulation is running
	void simulationStep()
	{
		if (simulationMode == 0)
		{
			PaperSimulation();
		}
		else
		{
			ExperimentalSimulation();
		}
		UpdateTiles();
		// Update the stats every 20 time steps only, avoiding to spam Vue
		// OR when we're paused and manually stepping
		if (!simulationRunning || t%20==0){
			// Update the UI with the current status of the simulation
			Application.ExternalCall("vm.$children[0].unityUpdate", 0, t, Ss.Count, Ns.Count, totalPM, simulationMode);
		}

	}

	/*** Main lifecycle methods ***/

	// Start is called before the first frame update
	void Start()
	{
		mapsFileNames = new string[]
		{
			"central_sp.png",
			"generic_maze.png",
			"maze_gpgpu.png",
			"wsn_network_20.png",
			"wsn_network_40.png",
			"wsn_network_60.png"
		};
		maps = new TextAsset[mapsFileNames.Length];
		for (int i = 0; i < mapsFileNames.Length; i++)
		{
			maps[i] = Resources.Load<TextAsset>(mapsFileNames[i]);
		}

		#if !UNITY_EDITOR && UNITY_WEBGL
			UnityEngine.WebGLInput.captureAllKeyboardInput = false;
		#endif

		// Disable V-Sync
		QualitySettings.vSyncCount = 0;
		// Set the framerate
		// To get a stable 60 fps, 56-57 is enough
		Application.targetFrameRate = 55;

		// Load the map
		selectMap(defaultMap);

		// Let the UI know we're alive and ready
		Application.ExternalCall("vm.$children[0].greet", "Hello from Unity!");
		// Update the UI with every map available and specify the one we're starting with
		Application.ExternalCall("vm.$children[0].updateMaps", String.Join(",", mapsFileNames), defaultMap);
		updateParameters();
	}

	// Gets ready to (re)start a simulation
	// Resets every param
	public void Initialization()
	{
		this.GetComponent<Tilemap>().ClearAllTiles();
		totalPM = 0;
		localFiftyStepsTime = 0;
		fiftyStepsPhase = false;
		antiCrashCounter = 0;
		
		/*
		if (simulationMode == 0)
		{
			defaultPMForS = 100;
			defaultCHAForN = 100;

			CON = 0.95f;
			PAP = 0.7f;
			PMP1 = 0.08f;
			PMP2 = 0.01f;
			CAP1 = 0.05f;
			CAP2 = 0.01f;
			ThPM = 0.2f;

			minAgeToDryOut = 0;
		}
		else if (simulationMode == 1)
		{
			defaultPMForS = 10000;
			defaultCHAForN = 100;

			CON = 0.95f;
			PAP = 0;
			PMP1 = 0;
			PMP2 = 0;
			CAP1 = 0.05f;
			CAP2 = 0.01f;
			ThPM = 20f;

			minAgeToDryOut = 1000;
		}
		*/
		Ss = new List<Vector2Int>();
		Ns = new List<Vector2Int>();
		lastEncapsulatedNS = new Vector2Int();
		secondToLastEncapsulatedNS = new Vector2Int();

		biggestCHAvalue = 0;
		smallestCHAvalue = 0;
		biggestPMvalue = 0;
		smallestPMvalue = 0;
		posIbiggestPMValue = 0;
		posJbiggestPMValue = 0;

		LoadTextureMapAndSetCameraCorrectly();
		InitCellMap();
		t = 0;

		if (simulationMode == 0)
		{
			ResetPaperFiftyStepsPhase();
		}
		else
		{
			foreach (Vector2Int s in Ss)
			{
				mapCells[s.x, s.y].PM = defaultPMForS;
			}
		}


		// When on editor, autostart the simulation
		#if UNITY_EDITOR
			simulationRunning = true;
		#endif

		CreateTiles();

		// Prepare for frame 0
		UpdateTiles();
	}

	/** Load the map as texture so we can access each pixel */
	void LoadTextureMapAndSetCameraCorrectly()
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
		cameraManager.MoveCamera(mapSizeX / 2, mapSizeY / 2);
		camera.orthographicSize = mapSizeX / 2 + mapSizeX / 20;
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
				else if (col.Equals(new Color(1, 1, 0, 1))) //Yellow
				{
					mapCells[i, j] = new Cell(true, 0, defaultCHAForN, false, CellType.N);
					Ns.Add(new Vector2Int(i, j));
				}
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (simulationRunning)
		{
			simulationStep();
		} else {
			updateParameters();
		}
	}

	void ExperimentalSimulation()
	{
		ApplyExperimentalDiffusionEquations();

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
                antiCrashCounter = 0;
            }
		}

		t++;
	}

	void ApplyExperimentalDiffusionEquations()
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
								if (GetCHA(x, y) == 0 && GetCHA(i, j) == 0 && !mapCells[i, j].didHaveCha && !mapCells[x, y].didHaveCha) //Uniform expansion case
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
								newMap[i, j].PM += GetPM(x, y);
							}
							else if (GetAge(i, j) > minAgeToDryOut
									 && IsCellAInDirectionOfCellB(x, y, i, j)
									 && !mapCells[i, j].TE
									 && mapCells[i, j].type != CellType.N
									 && mapCells[i, j].type != CellType.S)
							{
								newMap[i, j].PM -= GetPM(i, j);
							}
						}
					}
				}
			}
		}

		//Update the map
		UpdateMapWithNewDiffusionValues(newMap);

		//Debug.Log("TOTALPM " + totalPM+" "+ (Math.Round(totalPM / defaultPMForS)) + " "+(totalPM%defaultPMForS));
	}

	/** Do I reach cell A if I follow the direction of cell B? */
	private bool IsCellAInDirectionOfCellB(int Ax, int Ay, int Bx, int By)
	{
		if (mapCells[Bx, By].direction == Dir.N && Ax == Bx && Ay == By + 1)
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
	void PaperSimulation()
	{
		Debug.Log("Simulation running " + t + " " + localFiftyStepsTime + " " + fiftyStepsPhase);
		if (fiftyStepsPhase)
		{
			ExecutePaperFiftyStepsPhase();
		}

		if (!fiftyStepsPhase)
		{
			Debug.Log("Other stuff applied");


			if (Ns.Count == 0) //All N connected?
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
                    antiCrashCounter = 0;

                    /* DEBUG Passo 5000 se i nodi sono stati tutti raggiunti. Loop a 5050
					SetLatestEncapsulatedNS(v);
					if (Ns.Count == 0)
						t = 5000;
					*/
                }
			}

			if (t <= 5000)
			{
				if (t == 5000)
				{
					Debug.Log("We reached 5000, changing SP in NS");
					if (Ss.Count == 1)
					{
						Debug.Log("Error: there's only one S and we have reached step 5050. We cannot change it into an N!");

						// Restart?
						Initialization();
					}
					else
					{
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
				}

				ResetPaperFiftyStepsPhase();
				ExecutePaperFiftyStepsPhase();
			}
			else if (t < 10000)
			{
				ResetPaperFiftyStepsPhase();
				ExecutePaperFiftyStepsPhase();
			}
			else
			{
				simulationRunning = false;
			}
		}

		t++;
	}

	void ExecutePaperFiftyStepsPhase()
	{
		if (localFiftyStepsTime < 50)
		{
			ApplyPaperDiffusionEquations();
			localFiftyStepsTime++;
		}
		else
		{
			fiftyStepsPhase = false;
		}
	}

	private void ConnectNToNearestS(int i, int j)
	{
		Debug.Log("Tube connected: " + i + " " + j + " " + mapCells[i, j].CHA + " " + mapCells[i, j].direction + " " + mapCells[i, j].type);

		mapCells[i, j].TE = true;

		if (mapCells[i, j].type != CellType.S)
		{
			int x;
			int y;

			if (simulationMode == 0)
				GetHighestNeighbourPM(i, j, out x, out y);
			else
				GetNeighbourBasedOnDirection(i, j, out x, out y);

			if (antiCrashCounter < 500)
			{
				Debug.Log("Connecting, highest near PM: " + x + " " + y);
				antiCrashCounter++;
				ConnectNToNearestS(x, y);
			}
			else
			{
				Debug.Log("Infinite Loop encountered: " + antiCrashCounter);

				// Restart?
				Initialization();
			}
		}
	}

	private void GetNeighbourBasedOnDirection(int i, int j, out int x, out int y)
	{
		x = -1;
		y = -1;

		Dir dir = mapCells[i, j].direction;

		if (dir == Dir.SW)
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
				}
				else
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

	private void ResetPaperFiftyStepsPhase()
	{
		fiftyStepsPhase = true;
		localFiftyStepsTime = 0;
	}

	/** Equazioni di diffusione del paper. */
	void ApplyPaperDiffusionEquations()
	{
        totalPM = 0;
        Cell[,] newMap = CreateNewCellMap(mapSizeX, mapSizeY);

		for (int i = 0; i < mapSizeX; i++)
		{
			for (int j = 0; j < mapSizeY; j++)
			{
				//Calcolo PM
				if (mapCells[i, j].type != CellType.S && mapCells[i, j].type != CellType.U)
				{
					float[] values = new float[]
					{
						GetCHA(i - 1, j),
						GetCHA(i, j - 1),
						GetCHA(i + 1, j),
						GetCHA(i, j + 1),
						GetCHA(i - 1, j - 1),
						GetCHA(i + 1, j - 1),
						GetCHA(i - 1, j + 1),
						GetCHA(i + 1, j + 1)
					};

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
					}
					else if (maxCHA == GetCHA(i, j - 1))
					{
						PA_SOUTH = PAP;
						PA_NORTH = -PAP;
					}
					else if (maxCHA == GetCHA(i + 1, j))
					{
						PA_EAST = PAP;
						PA_WEST = -PAP;
					}
					else if (maxCHA == GetCHA(i, j + 1))
					{
						PA_NORTH = PAP;
						PA_SOUTH = -PAP;
					}
					else if (maxCHA == GetCHA(i - 1, j - 1))
					{
						PA_SOUTHWEST = PAP;
						PA_NORTHEAST = -PAP;
					}
					else if (maxCHA == GetCHA(i + 1, j - 1))
					{
						PA_SOUTHEAST = PAP;
						PA_NORTHWEST = -PAP;
					}
					else if (maxCHA == GetCHA(i - 1, j + 1))
					{
						PA_NORTHWEST = PAP;
						PA_SOUTHEAST = -PAP;
					}
					else if (maxCHA == GetCHA(i - 1, j + 1))
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
                totalPM += newMap[i, j].PM;

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

		//Debug.Log("BIGGEST PM IS "+ biggestPMvalue+" at "+ posIbiggestPMValue+" "+ posJbiggestPMValue);
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
		if (i < 0 || i >= mapSizeX || j < 0 || j >= mapSizeY || mapCells[i, j].type == CellType.U)
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
	void CreateTiles()
	{
		Tilemap tilemap = this.GetComponent<Tilemap>();

		for (int i = 0; i < mapSizeX; i++)
		{
			for (int j = 0; j < mapSizeY; j++)
			{
				Color col = tex.GetPixel(i, j);
				tilemap.SetTile(new Vector3Int(i, j, 0), tile);
				SetTileColour(col, new Vector3Int(i, j, 0));
			}
		}
	}

	float ConvertRange(
    int originalStart, int originalEnd, // original range
    int newStart, int newEnd, // desired range
    int value) // value to convert
	{
    	float scale = (float)(newEnd - newStart) / (originalEnd - originalStart);
    	return (float)(newStart + ((value - originalStart) * scale));
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
				float PMInRange01 = mapCells[i, j].PM / (biggestPMvalue - smallestPMvalue);
				//float PMInRange01 = mapCells[i, j].PM / defaultPMForS;
				float CHAInRange01 = mapCells[i, j].CHA / (biggestCHAvalue - smallestCHAvalue);
				//float CHAInRange01 = mapCells[i, j].CHA / defaultCHAForN;

				if (type == CellType.U)
				{
					SetTileColour(new Color(130f/255f, 153f/255f, 170f/255f, 1), new Vector3Int(i, j, 0));
				}
				else if(type == CellType.S)
				{
					SetTileColour(new Color(204f/255f, 0f/255f, 204f/255f, 1), new Vector3Int(i, j, 0));
				}
				else if (type == CellType.N)
				{
                    SetTileColour(new Color(255f/255f, 99f/255f, 71f/255f, 1), new Vector3Int(i, j, 0));
				}
				else if (mapCells[i, j].TE)
				{	
					if (simulationMode == 1){
							SetTileColour(new Color(16f/255f, 52f/255f, 166f/255f, 1), new Vector3Int(i, j, 0));
						}else {
							SetTileColour(new Color(6f/255f, 103f/255f, 255f/255f, 1), new Vector3Int(i, j, 0));
						}
				}
				else
				{

					if (simulationMode == 1)
					{
						float q1 = mapCells[i, j].PM;
						if (q1 >= 100){
							q1 = 100;
						}
						
						if (q1 == 0){
							SetTileColour(new Color(1, 1, 1, 1), new Vector3Int(i, j, 0));
						} else {
							float q2 = q1/10;
							SetTileColour(new Color((0.8f-q2)*1.25f , (q2-0.20f)*1.25f, 1, 1), new Vector3Int(i, j, 0));
						}
					}
					else
					{
						float q1 = mapCells[i, j].PM;
						if (q1 >= 1000){
							q1 = 1000;
						}
						
						if (q1 < 15){	
							float q2 = q1/40;
							SetTileColour(new Color(1-q2, 1-q2, 1, 1), new Vector3Int(i, j, 0));
						}	
						else {
							float q2 = q1/50;
							SetTileColour(new Color(1-q2, q2, 1, 1), new Vector3Int(i, j, 0));
						}
					}
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
