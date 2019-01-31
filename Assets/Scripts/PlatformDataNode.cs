using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlatformDataNode : MonoBehaviour
{
    public GameObject node;
    public Color NextColor;
    public float NextPosition;

    public bool Program;
    public bool Selected;
    public bool Simulate;

    public int i;
    public int j;

    public delegate void UpdatePlatformDataNodeUI(PlatformDataNode pdn);
    public static event UpdatePlatformDataNodeUI OnUpdatePlatformDataNodeUI;

    float Timescale = 0.1f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Checks to lerp node each frame
        //only moves when NextPosition is updated
        if (Program)
        {
            transform.position = Vector3.Lerp(transform.position,
                    new Vector3(transform.position.x,
                    NextPosition,
                    transform.position.z), Time.deltaTime);
        }

        if (Simulate)
        {
            NextColor = Color.blue;
            transform.gameObject.GetComponent<Renderer>().material.color =
                Color.Lerp(
                    transform.gameObject.GetComponent<Renderer>().material.color,
                    NextColor,
                    Timescale);


            transform.position = Vector3.Lerp(transform.position,
                    new Vector3(transform.position.x,
                    NextPosition,
                    transform.position.z), Timescale);
        }

        if (!Simulate && !Program)
        {
            NextColor = Color.white;
            transform.gameObject.GetComponent<Renderer>().material.color =
                Color.Lerp(
                transform.gameObject.GetComponent<Renderer>().material.color,
                NextColor,
                Timescale);


            transform.position = Vector3.Lerp(transform.position,
                    new Vector3(transform.position.x,
                    0,
                    transform.position.z), Timescale);
        }
    }

    public override string ToString()
    {
        return string.Format("{0},{1},{2}", i, j, NextPosition);
    }

    private void OnEnable()
    {
        UIManagerV2.OnNodeProgramChanged += UIManager_OnNodeProgramChanged;
    }

    private void OnDisable()
    {
        UIManagerV2.OnNodeProgramChanged -= UIManager_OnNodeProgramChanged;
    }

    public void SelectNode()
    {
        //must be true now that it has been selected
        Selected = true;
        Program = true;
        //update UI when a node has been selected
        if (OnUpdatePlatformDataNodeUI != null)
        {
            OnUpdatePlatformDataNodeUI(this);
        }
    }

    public void UIManager_OnNodeProgramChanged(Slider s)
    {
        if (Selected)
        {
            //only update position if it is a selected node
            NextPosition = s.value;
        }
    }
}
