using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlatformDataNode : MonoBehaviour {
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

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        //Checks to lerp node each frame
        //only moves when NextPosition is updated
        node.transform.position = Vector3.Lerp(node.transform.position,
                new Vector3(node.transform.position.x,
                NextPosition,
                node.transform.position.z), Time.deltaTime);
    }

    public override string ToString()
    {
        return string.Format("{0},{1},{2}", i, j, NextPosition);
    }

    public void readData(int i, int j, float NextPosition)
    {
        this.i = i;
        this.j = j;
        this.NextPosition = NextPosition;
    }

    private void OnEnable()
    {
        UIManagerV2.OnNodeProgramChanged += UIManager_OnNodeProgramChanged;
    }

    private void OnDisable()
    {
        UIManagerV2.OnNodeProgramChanged -= UIManager_OnNodeProgramChanged;
    }

    public void ResetDataNode()
    {
        //set every bool to false?
        //or reset its NextPosition to 0?
        //Not useful to me as of now and it's never called
        //so I leave this blank for now
    }

    public void SelectNode()
    {
        //must be true now that it has been selected
        Selected = true;
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
