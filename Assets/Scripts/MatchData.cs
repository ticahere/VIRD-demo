using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.Video;

/*** main script to process match data  & update match data ***/

public class MatchData : MonoBehaviour
{
    // match name
    public enum MatchNum {demo_match0};
    public MatchNum matchName = MatchNum.demo_match0;

    // folder path
    public string videoPath = "/rally_video/";
    public string summaryPath = "/summary/"; //match1/summary/";
    public string trajectoryPath = "/ball_trajectory_3d/";
    public string posesPath = "/player_poses/";
    public string positionPath = "/player_position/";
    public string shotPath = "/shot/";

    // json data
    public List<MatchSummary> matchList = new List<MatchSummary>(); // match metadata 
    public MatchSummary selected_match;

    public List<RallySummary> rallySumList = new List<RallySummary>(); // rally metadata 
    public List<string> rallyName = new List<string>();

    // filter
    public SummaryPanel.Game game_select = SummaryPanel.Game.G1;
    public List<string> rallyNameFilter = new List<string>();  // a list of rally to be shown
    public List<string> shotNameFilter = new List<string>();
    public string selected_shot = "";
    public List<string> filtered_shots = new List<string>();

    // rally video menu 
    public string selected_rally = "";

    // view switch
    public bool heatmapOn = true;
    public bool shotPointOn = true;
    public bool shotArcOn = true;
    public bool summaryViewOn = true;
    public bool matchSplit = false;

    // view management
    public SummaryPanel _summaryPanel;
    public ShotPanel _shotPanel;
    public VideoPanel _videoPanel;

    public GameObject _videoCanvas;
    public Trajectory _trajectory;
    public Shot _shot;
    public Heatmap _heatmap;

    // Derived stats
    public float matchLength = 0; // duration of the match
    public float rallyLength = 0; // total shots 
    public float maxRallyLength = 0; // max shot count

    // public float gameLength_G1 = 0; // G1 duration
    // public float rallyLength_G1 = 0; // G1 total shots 
    // public int rallyCount_G1 = 0; // G1 total rallies 
    // public float gameLength_G2 = 0; 
    // public float rallyLength_G2 = 0;  
    // public int rallyCount_G2 = 0;  
    // public float gameLength_G3 = 0;  
    // public float rallyLength_G3 = 0;  
    // public int rallyCount_G3 = 0;  
    public float[] gameLength = new float[6]; // G1 i,ii; G2 i,ii; G3 i,ii
    public float[] gameRallyLength = new float[6]; // G1 i,ii; G2 i,ii; G3 i,ii
    public int[] rallyCount = new int[6]; // G1 i,ii; G2 i,ii; G3 i,ii

    // Heatmap data
    public float[] shotPercA = new float[7];  // order: FrontLeft, MidLeft, BackLeft, FrontRight, MidRight, BackRight, NotInCourt
    public float[] shotPercB = new float[7];
    public float[] shotPercA_To = new float[7];  // for Areas_To
    public float[] shotPercB_To = new float[7];

    public string courtA_dir = "From"; // "From", "To"
    public string courtB_dir = "From";  // "From", "To"

    // video replay offset
    public int prev_offset = 6; // ~0.2sec
    public int post_offset = 3; // ~0.1sec
    public int video_end_buffer = 16; // 
    // public int video_end_buffer = 12; // 0.4sec let the video play longer

    MatchInteraction _matchInteraction;


    // Start is called before the first frame update
    void Start()
    {
        _matchInteraction = gameObject.transform.GetComponent<MatchInteraction>();

        videoPath = matchName.ToString() + videoPath;
        summaryPath = matchName.ToString() + summaryPath;
        trajectoryPath = matchName.ToString() + trajectoryPath;
        posesPath = matchName.ToString() + posesPath;
        positionPath = matchName.ToString() + positionPath;
        shotPath = matchName.ToString() + shotPath;

        // _matchInteraction.framerate = matchName.ToString() == "study_match2" ? 1/59.97f : 1/30f;

        LoadMatch();
        LoadRally();
        LoadShot();
        LoadVideo();

        _matchInteraction.rallyNameFilter = rallyNameFilter;
        _matchInteraction.shotNameFilter = shotNameFilter;

    }

    // -------------- update view --------------- //
    public void UpdateRallyList()
    {
        FilterShots();
        _heatmap.UpdateStats();
        _shot.UpdateShotView();
        _videoPanel.UpdateVideoView();
        _shotPanel.UpdateLocationFilter();
    }

    public void UpdateShotList()
    {
        FilterShots();
        _heatmap.UpdateStats();

        _videoPanel.UpdateVideoView();
        _shot.UpdateShotView();
        _shotPanel.UpdateLocationFilter();

    }

    public void SelectRallyDetail()
    {
        if (!_matchInteraction.summaryViewOn)
        {
            FilterShots();

            // update heatmap
            _heatmap.UpdateStats();

            // turn on trajectory and video for the selected_rally
            _trajectory.RenderTrajectory();
            _trajectory.RenderPosition();
            _trajectory.RenderPoses();
            _videoPanel.RenderVideo();

            // filter shots to the selected rally
            _shot.UpdateShotView();
            _shotPanel.UpdateLocationFilter();
        }
        else
        {
            //preview one shot
            _trajectory.RenderTrajectory();
             _trajectory.RenderPosition();
            _trajectory.RenderPoses();
            _videoPanel.RenderVideo();
        }

        

    }
    public void SelectShot()
    {
        if (selected_shot != "")
        {
            Shot.ShotArc arc = _shot.shotArray.Find(i => i.shotName == selected_shot);
            
                // add buffer before/after the shot
                Debug.Log("selected shot " + arc.shotName + " start: " + arc.startStep + " , end: " + arc.endStep + " , arc.totalRallyStep: " + arc.totalRallyStep);
                _matchInteraction.startStep = arc.startStep < prev_offset? 0 : arc.startStep - prev_offset;
                _matchInteraction.currentStep = _matchInteraction.startStep;
                _matchInteraction.totalStep = arc.endStep + post_offset - 1 < arc.totalRallyStep ? arc.endStep + post_offset : arc.endStep - 1;

        }
        else
        {
            _trajectory.RenderTrajectory();
            _trajectory.RenderPosition();
            _trajectory.RenderPoses();
        }
       
        _videoPanel.RenderVideo();

    }
    public void UpdateShotPoint()
    {
        shotPointOn = _matchInteraction.shotPointOn;
        //_shot.shotArc.SetActive(shotArcOn);
        _shot.ToggleShotPoint(shotPointOn);
    }
    public void UpdateShotArc()
    {
        shotArcOn = _matchInteraction.shotArcOn;
        _shot.ToggleShotArc(shotArcOn);
    }
    public void UpdateHeatMap()
    {
        //Debug.Log("update heat map");
        heatmapOn = _matchInteraction.heatmapOn;
        _heatmap.gameObject.SetActive(heatmapOn);
        // toggle checkmark
        _shotPanel.gameObject.transform.Find("ShotFilter/Heatmap Toggle").GetComponent<Toggle>().graphic.canvasRenderer.SetAlpha(heatmapOn ? 1f : 0f);
    }
    public void UpdateSummaryView()
    {
        summaryViewOn = _matchInteraction.summaryViewOn;
       
        _summaryPanel.gameObject.SetActive(summaryViewOn);
        //_videoPanel.transform.Find("Back").gameObject.SetActive(!summaryViewOn);
        //_videoPanel.transform.Find("Back").gameObject.SetActive(_videoPanel.showCanvas);

        FilterShots();
        _heatmap.UpdateStats();
        _shot.UpdateShotView();
        //_videoPanel.UpdateVideoView();
    }
    

    // -------------- Load match data--------------- //
    // match level data
    void LoadMatch() 
    {
        Debug.Log("load match list" );

        string fileName = Path.Combine(Application.dataPath, "Resources/match_list.json");
        //string fileName = Path.Combine(Application.persistentDataPath, "Resources/match_list.json");
        StreamReader r = new StreamReader(fileName);
        string json = r.ReadToEnd();
        matchList = JsonUtility.FromJson<MatchList>(json).MatchObjects.ToList();

        selected_match = matchList.Find(m => m.MatchID.ToString() == matchName.ToString().Substring(matchName.ToString().Length - 1));
        _trajectory.UpdatePlayerModel();

        Debug.Log("load match " + selected_match.MatchID);
    }
    // rally level data
    public void LoadRally()
    {
        // get the rally name list from folder
        DirectoryInfo dir = new DirectoryInfo(Path.Combine(Application.dataPath, "Resources/" + videoPath));
        //DirectoryInfo dir = new DirectoryInfo(Path.Combine(Application.persistentDataPath, "Resources/" + videoPath));
        FileInfo[] info = dir.GetFiles("*.*");
        foreach (FileInfo f in info)
        {
            string r = f.Name.Substring(0, f.Name.IndexOf("."));
            if (!rallyName.Contains(r))
            {
                rallyName.Add(r);
            }
        }
        rallyNameFilter = rallyName;

        LoadSummary();
    }
    public void LoadSummary() 
    {
        rallySumList.Clear();
        for (int i = 0; i < rallyNameFilter.Count; i++)
        {
            string rallySum = Path.Combine(Application.dataPath, "Resources/" + summaryPath + rallyName[i] + "_summary.json");
            //string rallySum = Path.Combine(Application.persistentDataPath, "Resources/" + summaryPath + rallyName[i] + "_summary.json");
            
            using (StreamReader r = new StreamReader(rallySum))
            {
                string json = r.ReadToEnd();
                RallySummary rd = JsonUtility.FromJson<RallySummary>(json);
                rallySumList.Add(rd);
                matchLength += rd.duration;
                rallyLength += rd.shotCount;
                maxRallyLength = rd.shotCount > maxRallyLength ? rd.shotCount : maxRallyLength;

                bool overHalf = int.Parse(rd.name.Substring(2,2)) > 11 | int.Parse(rd.name.Substring(5,2)) > 11 ;
                   
                //Game stats
                if (rd.name.Substring(0, 1) == "1") 
                {
                    // gameLength_G1 += rd.duration;
                    // rallyLength_G1 += rd.shotCount;
                    // rallyCount_G1 += 1;
                    if(!overHalf) { 
                        gameLength[0] += rd.duration;
                        gameRallyLength[0] += rd.shotCount;
                        rallyCount[0] += 1;
                    } else {
                        gameLength[1] += rd.duration;
                        gameRallyLength[1] += rd.shotCount;
                        rallyCount[1] += 1;
                    }
                } 
                else if (rd.name.Substring(0, 1) == "2") 
                {
                    // gameLength_G2 += rd.duration;
                    // rallyLength_G2 += rd.shotCount;
                    // rallyCount_G2 += 1;
                    if(!overHalf) { 
                        gameLength[2] += rd.duration;
                        gameRallyLength[2] += rd.shotCount;
                        rallyCount[2] += 1;
                    } else {
                        gameLength[3] += rd.duration;
                        gameRallyLength[3] += rd.shotCount;
                        rallyCount[3] += 1;
                    }
                }
                else if (rd.name.Substring(0, 1) == "3") 
                {
                    // gameLength_G3 += rd.duration;
                    // rallyLength_G3 += rd.shotCount;
                    // rallyCount_G3 += 1;
                    if(!overHalf) { 
                        gameLength[4] += rd.duration;
                        gameRallyLength[4] += rd.shotCount;
                        rallyCount[4] += 1;
                    } else {
                        gameLength[5] += rd.duration;
                        gameRallyLength[5] += rd.shotCount;
                        rallyCount[5] += 1;
                    }
                }
            }
        }
        Debug.Log("rallySumList count: " + rallySumList.Count);

    }
    // shot data
    public void LoadShot()
    {
        _shot.RenderShots();
        shotNameFilter = _shot.shotFilter;
    }
    // video list
    public void LoadVideo()
    {
        _videoPanel.RenderVideos();
    }

    //----- update data
    void FilterShots()
    {
        filtered_shots.Clear();
        foreach (Transform child in _shot.transform)
        {
            if (selected_rally != "" & !summaryViewOn)
            {
                if (selected_rally.Contains(child.name.Substring(5, 7)) & shotNameFilter.Contains(child.name))
                {
                    filtered_shots.Add(child.name);
                }
            }
            else
            {
                if (rallyNameFilter.Contains(child.name.Substring(5, 7)) & shotNameFilter.Contains(child.name))
                {
                    filtered_shots.Add(child.name);
                }
            }
        }
    }

}

[Serializable]
public class MatchList
{
    public MatchSummary[] MatchObjects;
}

[Serializable]
public class MatchSummary
{
    public float MatchID;
    public string PlayerTop;
    public string PlayerBottom;
    public string Winner;
    public string G1;
    public string G2;
    public string G3;
}
[Serializable]
public class RallySummary
{
    public string name;
    public int shotCount;
    public float duration;
    public string playerServe;
    public string playerWin;
    public ShotSummary[] shots;
}
[Serializable]
public class ShotSummary
{
    public int index;
    public string playerHit;
    public string tendency;
    public float startTime;
    public float endTime;
    public Vector3 startPlayerPosition;
    public Vector3 endPlayerPosition;
}

