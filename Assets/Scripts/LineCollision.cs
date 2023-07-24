using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LineCollision : MonoBehaviour
{
    // matchdata
    public MatchInteraction _matchInteraction;
    public MatchData _matchData;

    // color
    public Color c_hovered = new Color(0.93f, 0.51f, 0.93f);
    public Color c_selected = new Color(0.93f, 0.51f, 0.93f);
    Color c_original;
    

    // manage line interaction
    LineRenderer lr;
    public GameObject start_pt;
    public GameObject end_pt;

    Material start_normal;
    Material end_normal;
    public Material transparent;


    bool hasUpdated = false;
    bool hasHoverUpdated = false;


    void Start()
    {
        lr = this.GetComponent<LineRenderer>();
        _matchInteraction = GameObject.Find("MatchDataManager").GetComponent<MatchInteraction>();
        _matchData = GameObject.Find("MatchDataManager").GetComponent<MatchData>();

        start_normal = start_pt.GetComponent<Renderer>().material;
        end_normal = end_pt.GetComponent<Renderer>().material;

        c_original = lr.startColor;
        c_hovered = lr.startColor;
        c_selected = lr.startColor;
        c_hovered.a = 0.2f;
        c_selected.a = 0.5f;
        CreateMesh();

        _matchInteraction.selected_shot = "";
    }

    // Update is called once per frame
    void Update()
    {
        // when no shot is selected
        if (_matchInteraction.hovered_shot == "" & _matchInteraction.selected_shot == ""){
            c_original.a = 0.8f;
            lr.SetColors(c_original, c_original);

            start_pt.GetComponent<Renderer>().material = start_normal;
            end_pt.GetComponent<Renderer>().material = end_normal;
        }
        else
        {// hide other shots when a shot is selected
            c_original.a = 0f;

            if (_matchInteraction.selected_shot != this.name & _matchInteraction.hovered_shot != this.name)
            {
                lr.SetColors(c_original, c_original);

                start_pt.GetComponent<Renderer>().material = transparent;
                end_pt.GetComponent<Renderer>().material = transparent;
            }
            if (_matchInteraction.selected_shot != this.name & !hasUpdated)
            {
                DeselectLine();
            }
            if (_matchInteraction.hovered_shot != this.name & !hasHoverUpdated)
            {
                DehighlightLine();
            }
            
        }
    }

    public void HoverLine() // trigger on hover
    {
        if (_matchInteraction.hovered_shot != this.name & _matchInteraction.selected_shot != this.name)
        {
            hasHoverUpdated = false;

            lr.SetColors(c_hovered, c_hovered);
            lr.widthMultiplier = 2;

            _matchInteraction.hovered_shot = this.name;
        }
    }
    public void DehighlightLine()  // trigger on exit
    {
        if (_matchInteraction.hovered_shot == this.name) _matchInteraction.hovered_shot = "";
        if (_matchInteraction.selected_shot != this.name)
        {
            lr.SetColors(c_original, c_original);
            lr.widthMultiplier = 1;
            hasHoverUpdated = true;
        }
    }
    public void SelectLine() 
    {
        if (_matchInteraction.selected_shot != this.name)
        {
            hasUpdated = false;

            // update selection
            _matchInteraction.selected_shot = this.name;


            lr.SetColors(c_selected, c_selected);
            lr.widthMultiplier = 2;
            start_pt.GetComponent<Renderer>().material = start_normal;
            end_pt.GetComponent<Renderer>().material = end_normal;

            // link to video
            Debug.Log("hover line " + this.name.Split('-')[1]);
            _matchInteraction.selected_rally = this.name.Split('-')[1];
        }
        else
        {
            // if double select then deselect
            DeselectLine();
        }
    }
    public void DeselectLine()  // trigger on exit
    {
        if (_matchInteraction.selected_shot == this.name)
        {
            _matchInteraction.selected_shot = "";
            // delink to video
            Debug.Log("deselect line " + this.name.Split('-')[1] + " summaryView: " + _matchInteraction.summaryViewOn);
            if (_matchInteraction.summaryViewOn)
            {
                _matchInteraction.selected_rally = "";
            }
        }
        lr.SetColors(c_original, c_original);
        lr.widthMultiplier = 1;
        //start_pt.GetComponent<Renderer>().material = start_normal;
        //end_pt.GetComponent<Renderer>().material = end_normal;

        hasUpdated = true;
    }

    void CreateMesh()
    {
        var mesh = new Mesh();
        lr.widthMultiplier = 2;
        lr.BakeMesh(mesh);
        mesh.Optimize();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        lr.widthMultiplier = 1;

        this.GetComponent<MeshCollider>().sharedMesh = mesh;

    }
}

