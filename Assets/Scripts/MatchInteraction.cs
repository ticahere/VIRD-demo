using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*** main script to manage interaction and update metadata ***/

public class MatchInteraction : MonoBehaviour
{
    MatchData _matchData;

    // rally filter 
    public SummaryPanel _summaryPanel;
    public List<string> rallyNameFilter = new List<string>(); // a list of rallies to be shown
    public SummaryPanel.Game game_select = SummaryPanel.Game.G1;

    // shot fitler 
    public ShotPanel _shotPanel;
    public List<string> shotNameFilter = new List<string>(); // a list of shots to be shown
    public string selected_shot = "";
    public string hovered_shot = "";

    public string shot_dir = "From";
    public string shot_by = "All"; // All, PlayerA, PlayerB
    public string shot_from = "All"; // "FrontLeft", "MidLeft", "BackLeft", "FrontRight", "MidRight", "BackRight"
    public string shot_to = "All";  // "FrontLeft", "MidLeft", "BackLeft", "FrontRight", "MidRight", "BackRight"
    public string shot_outcome = "Winner+Error"; // "All", "Error", "Winner", "Winner+Error"

    // shot court
    public bool locationFilterOn = true;
    public bool heatmapOn = true;
    public bool shotPointOn = true;
    public bool shotArcOn = true;
    public string selectedArea = "";
    public string selectedSide = "";

    // view switch
    public bool summaryViewOn = true;
    public bool videoPanelOn = false;

    // menu toggle
    public bool wristUIActive = false;

    // trajectory
    public bool trajectoryOn = false;
    public int startStep = 0;
    public int currentStep = 0;
    public int totalStep = 0;
    public float framerate = 1 / 30f; // 29.97f; //video framerate
    public bool loop = true;
    public float speed = 25f;

    // video
    public string selected_rally = "";

    public Camera cam;
    public Color summaryBackground = Color.black;
    public Color rallyBackground = new Color(77 / 255f, 77 / 255f, 77 / 255f);

    void Start()
    {
        _matchData = gameObject.transform.GetComponent<MatchData>();
        cam.clearFlags = CameraClearFlags.SolidColor;
    }
    void Update()
    {
        if (rallyNameFilter != _matchData.rallyNameFilter) UpdateRallyFilter();
        if (shotNameFilter != _matchData.shotNameFilter) UpdateShotFilter();
        if (selected_rally != _matchData.selected_rally) SelectRally();
        if (selected_shot != _matchData.selected_shot) SelectShot();
        if (game_select != _matchData.game_select) SelectGame();

        if (summaryViewOn != _matchData.summaryViewOn) UpdateSummaryView();

        if (heatmapOn != _matchData.heatmapOn) _matchData.UpdateHeatMap();
        if (shotPointOn != _matchData.shotPointOn) _matchData.UpdateShotPoint();
        if (shotArcOn != _matchData.shotArcOn) _matchData.UpdateShotArc();

    }

    // rally filter interaction
    void UpdateRallyFilter()
    {
        _matchData.rallyNameFilter = rallyNameFilter; // update the selection in MatchData
        _matchData.UpdateRallyList();
    }


    // shot fitler interaction
    void UpdateShotFilter()
    {
        //CalculateShotStats();
        _matchData.shotNameFilter = shotNameFilter; // update the selection in MatchData
        _matchData.UpdateShotList();

    }


    // video control interaction
    void SelectRally()
    {
        Debug.Log(selected_rally + " video is selected");
        _matchData.selected_rally = selected_rally;
        

        if (selected_rally != "")
        {
            startStep = 0;
            currentStep = 0;
            trajectoryOn = true;
            // update shotFilterName

            videoPanelOn = true;

            // hide heatmap by default
            heatmapOn = false;

            _matchData.SelectRallyDetail();
        }
        else {
            videoPanelOn = false;
            // show heatmap
            heatmapOn = true;
        }
    }

    void SelectShot()
    {
        _matchData.selected_shot = selected_shot;

        //if (selected_shot == "" & summaryViewOn) //hide bird
        if (selected_shot == "" & !videoPanelOn) //hide bird
        {
            trajectoryOn = false;
        }
        else
        {
            trajectoryOn = true;
            _matchData.SelectShot();
        }

    }

    void SelectGame(){ // game set
        Debug.Log("Select Game");
        _matchData.game_select = game_select;
        _shotPanel.UpdatePlayerName();
        _matchData._trajectory.UpdatePlayerModel();
    }
    //
    void UpdateSummaryView()
    {
        Debug.Log("UpdateSummaryView " + summaryViewOn);
        if (summaryViewOn)
        {
            trajectoryOn = false;
            selected_rally = "";
            //_matchData.selected_rally = "";
            cam.backgroundColor = summaryBackground;

        } else
        {
            trajectoryOn = true;
            cam.backgroundColor = rallyBackground;
        }
        
        _matchData.UpdateSummaryView();
    }
}
