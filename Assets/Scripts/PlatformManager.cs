using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlatformManager : PlatformGenericSinglton<PlatformManager> {

    public delegate void PlatformManagerUpdateUI(PlatformConfigurationData pcd);
    public static event PlatformManagerUpdateUI OnPlatformManagerUpdateUI;

    public delegate void PlatformManagerUpdateCamera(PlatformConfigurationData pcd);
    public static event PlatformManagerUpdateCamera OnPlatformManagerUpdateCamera;

    GameObject[,] platform;
    GameObject currentSelection;

    public Material myMaterial;
    public int M;
    public int N;

    public bool Program;
    public bool SimulateTest;

    public PlatformConfigurationData configurationData;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        #region Object Selection
        if (Input.GetMouseButtonUp(0))
        {
            if (Program)
            {
                #region Screen To World
                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
                if (!IsPointerOverUIObject())
                {
                    if (hit)
                    {
                        Debug.Log("Hit " + hitInfo.transform.gameObject.name);

                        #region COLOR
                        hitInfo.transform.gameObject.GetComponent<Renderer>().material.color = Color.blue;

                        if (currentSelection != null)
                        {
                            //De-select old one
                            currentSelection.GetComponent<PlatformDataNode>().Selected = false;

                            //Now assign the new selection
                            currentSelection = hitInfo.transform.gameObject;

                            //Do select stuff, set selected to true & update UI
                            currentSelection.GetComponent<PlatformDataNode>().SelectNode();
                        }
                        else
                        {
                            currentSelection = hitInfo.transform.gameObject;
                            currentSelection.GetComponent<PlatformDataNode>().SelectNode();
                        }
                        #endregion
                    }
                    else
                    {
                        Debug.Log("No hit");
                    }
                    #endregion
                }
            }
            #endregion
        }
    }

    private void OnEnable()
    {
        UIManagerV2.BuildPlatformOnClicked += UIManagerV2_OnBuildPlatformClicked;
        UIManagerV2.OnWriteProgramData += UIManagerV2_OnWriteProgramData;
        UIManagerV2.OnReadWriteLinesData += ReadFile;
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void OnDisable()
    {
        UIManagerV2.BuildPlatformOnClicked -= UIManagerV2_OnBuildPlatformClicked;
        UIManagerV2.OnWriteProgramData -= UIManagerV2_OnWriteProgramData;
        UIManagerV2.OnReadWriteLinesData -= ReadFile;
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (platform != null)
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "Programming Scene":
                    //nodes become selectable now
                    Program = true;
                    SimulateTest = false;
                    BuildPlatform();
                    break;
                case "Configuration Scene":
                    //nodes not selectable here
                    Program = false;
                    SimulateTest = false;
                    BuildPlatform();
                    break;
                case "Simulate Scene":
                    Program = false;
                    SimulateTest = true;
                    //BuildPlatform();
                    ReadFile();
                    break;
                case "Main Menu":
                    //go to main menu, but do not built platform
                    Program = false;
                    SimulateTest = false;
                    break;
            }

            if (OnPlatformManagerUpdateUI != null)
            {
                //Change UI based on scene you're in
                OnPlatformManagerUpdateUI(configurationData);
            }

            if (OnPlatformManagerUpdateCamera != null)
            {
                //Update Camera, is called only in Programming & Configuration Scene
                OnPlatformManagerUpdateCamera(configurationData);
            }

        }

    }

    public void UIManagerV2_OnBuildPlatformClicked(PlatformConfigurationData pcd)
    {
        DestroyPlatform();

        configurationData = pcd;

        //Set new M & N
        M = pcd.M;
        N = pcd.N;

        //now build
        BuildPlatform();
    }

    public void BuildPlatform()
    {
        platform = new GameObject[M, N];
        for(int i = 0; i < M; i++)
        {
            for(int j = 0; j < N; j++)
            {
                //Creating cubes
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = new Vector3(i + (i * configurationData.deltaSpacing), 0, j + (j * configurationData.deltaSpacing));
                cube.transform.rotation = Quaternion.identity;
                cube.transform.localScale = new Vector3(1, 0.1f, 1);
                cube.name = string.Format("Cube-{0}-{1}", i, j);
                cube.GetComponent<Renderer>().material = myMaterial;

                //Adding script, initializing data
                cube.AddComponent<PlatformDataNode>();
                PlatformDataNode pdn = cube.GetComponent<PlatformDataNode>();
                pdn.NextColor = Color.white;
                pdn.NextPosition = cube.transform.position.y;
                pdn.node = cube;
                
                pdn.i = i;
                pdn.j = j;

                platform[i, j] = cube;
            }
        }


    }

    public void DestroyPlatform()
    {
        if (platform != null) {
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Destroy(platform[i, j]);
                }
            }
        }       
    }

    //Used to determine if UI is over a game object
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        foreach (var result in results)
        {
            Debug.Log(result.gameObject.name);
        }
        return results.Count > 0;
    }


    public void UIManagerV2_OnWriteProgramData()
    {
        using (StreamWriter outputFile = new StreamWriter(Path.Combine(Application.dataPath, "WriteLines.txt")))
        {
            outputFile.WriteLine(configurationData.ToString());
            for(int i = 0; i < M; i++)
            {
                for(int j = 0; j < N; j++)
                {
                    outputFile.WriteLine(platform[i, j].GetComponent<PlatformDataNode>().ToString());
                }
            }
        }
    }

    public void ReadFile()
    {
        if(platform == null) { 
            //nothing new built, so read from data
            Debug.Log("Reader reached");
            StreamReader reader = new StreamReader(Path.Combine(Application.dataPath, "WriteLines.txt"));
            //sDebug.Log(reader.ReadToEnd());

            using (reader)
            {
                //this should be the PCD line (1st line)
                string line;
                string[] entries;
                int i = 0;
                while((line = reader.ReadLine()) != null)
                {
                    //platform[entries[0], entries[1]] = new PND(
                    entries = line.Split(',');
                    if (i == 0)
                    {
                        //1st line is configuration data
                        Debug.Log(entries[0] + "," + entries[1] + "," + entries[2]+"," + entries[3]);
                        //Convert
                        int M = Convert.ToInt32(entries[0]);
                        int N = Convert.ToInt32(entries[1]);
                        float deltaSpacing = (float) Convert.ToDouble(entries[2]);
                        float RandomHeight = (float) Convert.ToDouble(entries[3]);
                        //Configure
                        PlatformConfigurationData pcd = new PlatformConfigurationData();
                        pcd.M = M;
                        pcd.N = N;
                        pcd.deltaSpacing = deltaSpacing;
                        pcd.RandomHeight = RandomHeight;
                        configurationData = pcd;
                        i++;
                    }
                    else
                    {
                        //rest of lines are now platform data nodes
                        Debug.Log(entries[0] + "," + entries[1] + "," + entries[2]);
                    }
                }

            }
            reader.Close();
            BuildPlatform();
        }
        //otherwise do nothing
    }
}
