using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class UIManagerV2 : MonoBehaviour {
    //Raises event for PlatformManager to Build
    //and for PlatformCameraControl to position camera on center
    public delegate void BuildPlatformOnClick(PlatformConfigurationData pcd);
    public static event BuildPlatformOnClick BuildPlatformOnClicked;
    
    //Pass slider to method in PlatformDataNode
    //In order for it to get the value for its height
    public delegate void NodeProgramChanged(Slider s);
    public static event NodeProgramChanged OnNodeProgramChanged;

    //delegate for reading file
    //bc of how scenes are handled in PlatformManager
    public delegate void OnReadWriteLines();
    public static event OnReadWriteLines OnReadWriteLinesData;

    //Create the text file when the program button is clicked
    //updates file if it's created
    public delegate void WriteProgramData();
    public static event WriteProgramData OnWriteProgramData;

    //All text variables that get updated when changed are made
    public Text PlatformSize;
    public Text DeltaSpacing;
    public Text YRange;
    public Text SelectedNodeName;
    public Text SelectedNodePosition;

    //Two sliders, for spacing and y-range
    public Slider DeltaSlider;
    public Slider Y_Slider;
    public Slider HeightSlider;

    //Two input fields, for dimensions
    public InputField M_Input;
    public InputField N_Input;
    public Dropdown myDropdown;

    String ColorScale;

    private void OnEnable()
    {
        //+= events 
        PlatformDataNode.OnUpdatePlatformDataNodeUI += PlatformDataNode_OnUpdatePlatformDataNodeUI;
        PlatformManager.OnPlatformManagerUpdateUI += PlatformManager_OnPlatformManagerUpdateUI;
    }

    private void OnDisable()
    {
        //-= events
        PlatformDataNode.OnUpdatePlatformDataNodeUI -= PlatformDataNode_OnUpdatePlatformDataNodeUI;
        PlatformManager.OnPlatformManagerUpdateUI += PlatformManager_OnPlatformManagerUpdateUI;
    }

    //On Click methods
    public void OnClick(Button b)
    {
        switch (b.name)
        {
            case "Main Menu Button":
                //Go to main menu
                SceneManager.LoadScene("Main Menu");
                break;
            case "Configuration Button":
                //Go to Configuration scene
                SceneManager.LoadScene("Configuration Scene");
                break;
            case "Program Button":
                //Go to Program scene
                SceneManager.LoadScene("Programming Scene");
                break;
            case "Simulate Button":
                //Go to Simulate scene
                SceneManager.LoadScene("Simulate Scene");
                if(OnReadWriteLinesData != null)
                {
                    OnReadWriteLinesData();
                }
                break;
            case "Exit Button":
                //Close Application
                Application.Quit();
                break;
            case "Build Platform Button":
                //Send field info to PlatformConfiguration Data
                //and build
                if(BuildPlatformOnClicked != null)
                {
                    PlatformConfigurationData pcd = new PlatformConfigurationData();
                    if(M_Input.text != "" && N_Input.text != "")
                    {
                        //Both input fields are filled so assign new values
                        pcd.M = Convert.ToInt32(M_Input.text);
                        pcd.N = Convert.ToInt32(N_Input.text);
                    }
                    if (M_Input.text != "" && N_Input.text == "")
                    {
                        //M input is filled, N input is empty
                        pcd.M = Convert.ToInt32(M_Input.text);
                    }
                    if (M_Input.text == "" && N_Input.text != "")
                    {
                        //N input is filled, M input is empty
                        pcd.N = Convert.ToInt32(N_Input.text);
                    }
                    //Do nothing if both are empty meaning, leave pcd.M and pcd.N as the default values when created
                    //deltaSpacing and RandomHeight get assigned slider values in case they have been changed
                    pcd.deltaSpacing = DeltaSlider.value;
                    pcd.RandomHeight = Y_Slider.value;
                    BuildPlatformOnClicked(pcd);

                    PlatformSize.text = string.Format("Platform Size: {0}x{1}", pcd.M, pcd.N);
                }
                break;
            case "Program Platform Button":
                //Generate txt file
                if (OnWriteProgramData != null)
                {
                    OnWriteProgramData();
                }
                break;

        }
    }

    //On Slider Value Change methods
    public void OnSliderValueChanged(Slider s)
    {
        //All sliders update the UI as they are changed
        //Delta and Y-Axis values aren't passed until Build button is clicked
        switch (s.name)
        {
            case "Delta Slider":
                DeltaSpacing.text = string.Format("Spacing: {0:0.00}f", s.value);
                break;
            case "Y-Axis Slider":
                YRange.text = string.Format("Y-Axis Range: {0}", s.value);
                break;
            case "Height Slider":
                if (OnNodeProgramChanged != null)
                {
                    OnNodeProgramChanged(s);
                    SelectedNodePosition.text = string.Format("Height: {0:0.00}f", s.value);
                }
                break;
        }
    }

    //Only Dropdown method
    public void OnDropDownChange() //Change color scale based on drop down value
    {
        if (myDropdown.value == 0)
        {
            //Gray Scale Selected
            ColorScale = "GrayScale Spectrum";
        }
        if (myDropdown.value == 1)
        {
            //Red Scale Selected
            ColorScale = "RedScale Spectrum";
        }
        if (myDropdown.value == 2)
        {
            //Green Scale Selected
            ColorScale = "GreenScale Spectrum";
        }
        if (myDropdown.value == 3)
        {
            //Blue Scale Selected
            ColorScale = "BlueScale Spectrum";
        }
        if (myDropdown.value == 4)
        {
            //Random Scale Selected
            ColorScale = "Random Spectrum";
        }
        Debug.Log(myDropdown.value + ": " + ColorScale);
    }

    public void PlatformDataNode_OnUpdatePlatformDataNodeUI(PlatformDataNode pdn)
    {
        //UI is updated when a tile is selected
        SelectedNodeName.text = string.Format("Node [{0},{1}]", pdn.i, pdn.j);
        SelectedNodePosition.text = string.Format("Height: {0:0.00}f", pdn.NextPosition);
        HeightSlider.value = pdn.NextPosition;
    }

    public void PlatformManager_OnPlatformManagerUpdateUI(PlatformConfigurationData pcd)
    {
        //check if text fields are null before trying to update
        //YRange text doesn't exist in programming scene
        //and HeightSlider doesn't exist in configuration scene
        if (PlatformSize != null && DeltaSpacing != null)
        {
            PlatformSize.text = string.Format("Platform Size: {0}x{1}", pcd.M, pcd.N);
            DeltaSpacing.text = string.Format("Spacing: {0:0.00}f", pcd.deltaSpacing);
            if(YRange != null)
            {
                YRange.text = string.Format("Y-Axis Range: {0}", pcd.RandomHeight);
            }
            if(HeightSlider != null)
            {
                HeightSlider.maxValue = pcd.RandomHeight;
            }
        }
    }
}
