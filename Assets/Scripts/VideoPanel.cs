using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Linq;
using TMPro;

public class VideoPanel : MonoBehaviour
{
    
    public MatchData _matchData;
    public MatchInteraction _matchInteraction;

    // video objects
    public GameObject videoCanvas;
    public VideoPlayer vp;
    public List<VideoClip> videoClips;

    // video panel prefab
    public GameObject rally_menu;
    public GameObject diagram_menu;
    public GameObject rally_prefab;

    // rally stats
    private int filterRallyCount = 0;
    private int shortRallyCount = 0;

    // diagram
    public GameObject area_diagram;
    List<string> Diagram_List = new List<string>();
    private string cam_zone_old = "";

    private string video_name = "";
    public bool isVideoLoaded = false;

    void Start()
    {
        //Unity.XR.Oculus.Performance.TrySetDisplayRefreshRate(90f);
    }

    void Update()
    {
        //if (!isVideoLoaded) RenderVideos();

        if (_matchData.selected_rally != "")
        {
            _matchInteraction.videoPanelOn = true;
            PlayVideo();
        } else {
            vp.Stop();
        }
        
        videoCanvas.SetActive(_matchInteraction.videoPanelOn);

        //  faces user
        //var target = gameObject.transform.position - Camera.main.transform.position;
        //gameObject.transform.rotation = Quaternion.LookRotation(target);
        //gameObject.transform.eulerAngles = new Vector3(0, gameObject.transform.eulerAngles.y, 0);

    }
    public void RenderVideos()
    {
        Debug.Log("render videos");
        videoClips.Clear();


        // create a diagram
        CreateDiagram("Diagram", rally_menu);
        
        for (int i = 0; i < _matchData.rallyNameFilter.Count; i++)
        {
            // load video clips
            string v_name = _matchData.videoPath + _matchData.rallyNameFilter[i];
            VideoClip c = Resources.Load<VideoClip>(v_name) as VideoClip;
            videoClips.Add(c);

            // create video preview on video panel
            GameObject newRally = Instantiate(rally_prefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0)) as GameObject;

            int rid = i + 1;
            string r_name = _matchData.rallyNameFilter[i];
            string r_winner = _matchData.rallySumList.Find(r => r.name == r_name).playerWin;
            newRally.name = r_name;
            newRally.transform.SetParent(rally_menu.transform, true);
            newRally.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            newRally.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);
            newRally.GetComponent<RectTransform>().localPosition = new Vector3(newRally.transform.position.x, newRally.transform.position.y, 0);
            newRally.transform.Find("Title").GetComponent<Text>().text = r_name;

            // update video texture
            //RenderTexture rt = new RenderTexture(1280, 720, 16, RenderTextureFormat.ARGB32);
            //rt.Create();
            //newRally.transform.Find("RallyClip").GetComponent<RawImage>().texture = rt;
            //VideoPlayer vp = newRally.transform.Find("RallyClip").GetComponent<VideoPlayer>();
            //vp.clip = videoClips[i];
            //vp.targetTexture = rt;
            // set menu selection
            newRally.transform.Find("RallyClip").GetComponent<Toggle>().onValueChanged.AddListener(delegate {
                UpdateSelection(newRally.transform.Find("RallyClip"), r_name);
            });

            // update rally data: score & length
            string ScoreTop = r_name[3] == char.Parse("_") ? r_name.Substring(2, 1) : r_name.Substring(2, 2); // video clip names can be "1_1_0", "1_10_9" or "1_12_10"
            newRally.transform.Find("Score/PlayerTop").GetComponent<Text>().text = ScoreTop;

            bool overHalf = int.Parse(r_name.Substring(2,2)) > 11 | int.Parse(r_name.Substring(5,2)) > 11 ;
            bool isFirstGamePos = r_name[0] == char.Parse("1") | (r_name[0] == char.Parse("3") & !overHalf);
           
            string ScoreBottom = r_name[r_name.Length - 2] == char.Parse("_") ? r_name.Substring(r_name.Length - 1, 1) : r_name.Substring(r_name.Length - 2, 2); 
            newRally.transform.Find("Score/PlayerBottom").GetComponent<Text>().text = ScoreBottom;

             if (isFirstGamePos) {
                newRally.transform.Find("Score/PlayerTop").GetComponent<Text>().color =  r_winner == "top" ? Helper.top_player_color : Color.white;
                newRally.transform.Find("Score/PlayerBottom").GetComponent<Text>().color = r_winner == "bottom" ? Helper.bottom_player_color : Color.white;
            } else {
                newRally.transform.Find("Score/PlayerTop").GetComponent<Text>().color = r_winner == "bottom" ? Helper.top_player_color : Color.white;
                newRally.transform.Find("Score/PlayerBottom").GetComponent<Text>().color = r_winner == "top" ? Helper.bottom_player_color : Color.white;
            } 

            int rallyDuration = _matchData.rallySumList.Find(r => r.name == r_name).shotCount;
            newRally.transform.Find("Duration/value").GetComponent<Text>().text = rallyDuration.ToString();
            float rallyDurationAvg = _matchData.rallyLength / _matchData.rallySumList.Count;
            newRally.transform.Find("Duration/Bar").GetComponent<RectTransform>().sizeDelta = new Vector2(200 * rallyDuration / _matchData.maxRallyLength, 30);
            //highlight short rallies (< 10 shots)
            newRally.transform.Find("Duration/Bar").GetComponent<Image>().color = rallyDuration < 10 ? Color.red : new Color(1/255f, 120 / 255f, 233 / 255f);
        }



        isVideoLoaded = true;
    }
    public void UpdateVideoView()
    {
        filterRallyCount = 0;
        shortRallyCount = 0;
        //if (_matchData.summaryViewOn & (cam_zone_old!= _matchInteraction.selectedArea | _matchInteraction.selectedArea.Contains("NotInCourt") | _matchInteraction.shot_by != "All"))
        if (_matchData.summaryViewOn)
        {
            cam_zone_old = _matchInteraction.selectedArea;

            UpdateVideoDiagram();

            List<string> rallyOfShot = new List<string>();
            foreach (string shot in _matchData.shotNameFilter)
            {
                string rallyName = shot.Substring(5, 7);
                if (!rallyOfShot.Contains(rallyName)) rallyOfShot.Add(rallyName);
            }
            if (diagram_menu.transform.childCount == 0 | _matchInteraction.selectedArea == "All") // no locationFilter
            {
                Debug.Log("Update Video View - normal filtering");
                rally_menu.transform.parent.gameObject.SetActive(true);
                diagram_menu.transform.parent.gameObject.transform.parent.gameObject.SetActive(false);

                foreach (Transform child in rally_menu.transform)
                {
                    bool showRally = rallyOfShot.Contains(child.name) & _matchData.rallyNameFilter.Contains(child.name) | child.name.Contains("Diagram");
                    //bool showRally =  _matchInteraction.rallyNameFilter.Contains(child.name) | child.name.Contains("Diagram");
                    child.gameObject.SetActive(showRally);

                    if (showRally) {
                        filterRallyCount ++;
                        int rallyDuration = _matchData.rallySumList.Find(r => r.name == child.name).shotCount;
                        if (rallyDuration < 10) shortRallyCount ++;
                    }
                }
            }
            else
            {
                // rearrange rally video based on diagram category
                Debug.Log("Update Video View - diagram category");
                rally_menu.transform.parent.gameObject.SetActive(false);
                diagram_menu.transform.parent.gameObject.transform.parent.gameObject.SetActive(true);

                foreach (string d_name in Diagram_List)
                {
                    List<string> shotsInArea = new List<string>();
                    shotsInArea = _matchData.filtered_shots.FindAll(x => x.Contains(d_name));

                    List<string> rallyOfShotsInArea = new List<string>();
                    Debug.Log("diagram area " + d_name + " shot count " + shotsInArea.Count);

                    foreach (string s in shotsInArea)
                    {
                        string r_Name = s.Substring(5, 7);
                        if (!rallyOfShotsInArea.Contains(r_Name)) rallyOfShotsInArea.Add(r_Name);
                    }

                    foreach (Transform child in rally_menu.transform)
                    {
                        bool showRally = rallyOfShotsInArea.Contains(child.name);


                        if (showRally)
                        {
                            GameObject duplicatedRally = GameObject.Instantiate(child.gameObject);
                            duplicatedRally.transform.SetParent(diagram_menu.transform.Find("Diagram-" + d_name + "/RallyVideo/Viewport/Content").gameObject.transform, true);
                            duplicatedRally.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                            duplicatedRally.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);
                            duplicatedRally.GetComponent<RectTransform>().localPosition = Vector3.zero;
                            string rallyName = duplicatedRally.name.Substring(0, duplicatedRally.name.IndexOf("("));
                            duplicatedRally.name = rallyName;

                            duplicatedRally.SetActive(true);
                            duplicatedRally.transform.Find("RallyClip").GetComponent<Toggle>().onValueChanged.AddListener(delegate
                            {
                                UpdateSelection(duplicatedRally.transform.Find("RallyClip"), rallyName);
                            });
                            
                            filterRallyCount ++;
                            int rallyDuration = _matchData.rallySumList.Find(r => r.name == child.name).shotCount;
                            if (rallyDuration < 10) shortRallyCount ++;
                            //Debug.Log("add new rally  " + duplicatedRally.name + " under " + d_name);
                        }
                    }
                }
            }

            gameObject.transform.Find("RallyStats/Count").GetComponent<Text>().text = shortRallyCount.ToString() + " / " + filterRallyCount.ToString();
        }
        
    }
    public void UpdateVideoDiagram()
    {
        string startSide = _matchInteraction.shot_by == "All" ? "All" : _matchInteraction.shot_by.Substring(_matchInteraction.shot_by.Length - 1); //All, A or B
        string startArea = _matchInteraction.shot_from; // e.g., MidLeft
        string endSide = _matchInteraction.shot_by == "All" ? "All" : startSide == "A" ? "B" : "A";
        string endArea = _matchInteraction.shot_to;

        string locationFilter = (startArea != "All") ? (startSide + "-" + startArea) : (endSide + "-" + endArea);
        Debug.Log("UpdateVideoDiagram (by) " + _matchInteraction.shot_by + "(from) " + _matchInteraction.shot_from + " (to)" + _matchInteraction.shot_to + ", locationFilter " + locationFilter);

        //rally_menu.transform.Find("Diagram/ShotFilter").GetComponent<Text>().text = (startSide == "All" & endSide == "All") ? "" : locationFilter.Substring(2); //"All" : locationFilter.Substring(2);
        //rally_menu.transform.Find("Diagram/From").GetComponent<Text>().text = (startSide == "All" & endSide == "All") ? "" : locationFilter.Substring(0, 1) == startSide ? "From" : "To"; //_matchInteraction.shot_dir : locationFilter.Substring(0, 1) == startSide ? "From" : "To";
        //rally_menu.transform.Find("Diagram/Player").GetComponent<Text>().text = startSide == "All" ? "" : startSide == "A" ? _matchData.selected_match.PlayerBottom : _matchData.selected_match.PlayerTop;


        // remove all diagrams
        foreach (Transform child in diagram_menu.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        Diagram_List = new List<string>();
        List<float> Diagram_Stats = new List<float>();

        if (_matchInteraction.locationFilterOn & (startArea !="All" | endArea != "All"))
        {
            foreach (string side in new string[] { "A", "B" })
            {
                for (int i = 0; i < Helper.courtArea.Length; i++)
                {
                    string area = Helper.courtArea[i] + side; // e.g. MidLeftA
                    bool checkOppositeSide = false;

                    checkOppositeSide = (startArea != "All") ? side != startSide : side == startSide;
                    string diagramFilter = (startArea != "All") ? (startSide + "-" + startArea) : (endSide + "-" + endArea);
                    string diagramTarget = side + "-" + Helper.courtArea[i];


                    // create new diagram for each grid 
                    if (checkOppositeSide) //(side != startSide)
                    {
                        float shotPerc = side == "A" ? _matchData.shotPercA[i] : _matchData.shotPercB[i];
                        Debug.Log("shotPerc " + diagramFilter + " " + shotPerc + " target " + diagramTarget);
                        if (shotPerc != 0)
                        {
                            string diagramName = "";
                            string filterDir = "";
                            if (diagramFilter.Substring(0,1) == startSide)
                            {
                                diagramName = "from-" + diagramFilter + "-to-" + diagramTarget;
                                filterDir = "To";
                            } else {
                                diagramName = "from-" + diagramTarget + "-to-" + diagramFilter; 
                                filterDir = "From";
                            }


                            // CreateDiagram("Diagram-" + diagramName, diagram_menu);
                            // GameObject new_diagram = diagram_menu.transform.Find("Diagram-" + diagramName).gameObject;

                            // new_diagram.transform.Find("From/Text").GetComponent<Text>().text = filterDir;
                            // new_diagram.transform.Find("From").GetComponent<Image>().color = filterDir == "From" ? Helper.from_hot : Helper.to_hot;
                            // new_diagram.transform.Find("ShotFilter").GetComponent<Text>().text = diagramTarget.Substring(2);

                            // new_diagram.transform.Find("Player").GetComponent<Text>().text = startSide == "A" ? _matchData.selected_match.PlayerBottom : _matchData.selected_match.PlayerTop;

                            // Diagram_List.Add(diagramName);
                            // Diagram_Stats.Add(shotPerc);

                            
                            // color this diagram
                            // foreach (string new_diagram_side in new string[] { "A", "B" })
                            // {
                            //     for (int j = 0; j < Helper.courtArea.Length; j++)
                            //     {
                            //         //Debug.Log("color diagram " + new_diagram_side + Helper.courtArea[j] + _matchData.shotPercA[j]);

                            //         float new_diagram_shotPerc = new_diagram_side == "A" ? _matchData.shotPercA[j] : _matchData.shotPercB[j];
                            //         string new_diagram_courtDir = new_diagram_side == startSide ? "From" : "To";
                         
                            //         Color new_diagram_areaColor = Helper.GetShotPercColor(new_diagram_shotPerc, new_diagram_courtDir);
                            //         string statsVal = new_diagram_shotPerc == new_diagram_shotPerc ? new_diagram_shotPerc == 0 ? "" : (Mathf.Round(new_diagram_shotPerc * 100)).ToString() + "%" : "";
                                   
                            //         // assign stats value
                            //         if (new_diagram_courtDir == filterDir & Helper.courtArea[j] == diagramTarget.Substring(2)) {
                            //             new_diagram.transform.Find("Stats/Text").GetComponent<Text>().text = statsVal;
                            //             // change color based on background color
                            //             new_diagram.transform.Find("Stats/Text").GetComponent<Text>().color = new_diagram_shotPerc > 0.2f ? Color.white : Color.black;
                            //             new_diagram.transform.Find("ShotFilter").GetComponent<Text>().color = new_diagram_shotPerc > 0.2f ? Color.white : Color.black;
                            //             new_diagram.transform.Find("Stats").GetComponent<Image>().color = new_diagram_areaColor;
                            //         }

                            //     }
                            // }

                        }
                    }

                 }
            }
        }

        // // rank diagrams
        // float[] array = Diagram_Stats.ToArray();
        // Dictionary<float, int> numRanks = array
        //     .GroupBy(i => i)
        //     .OrderByDescending(g => g.Key)
        //     .Select((g, index) => (num: g.Key, rank: index))
        //     .ToDictionary(x => x.num, x => x.rank);
        // int[] result = array.Select(i => numRanks[i]).ToArray();

        // for (int d = 0; d < Diagram_Stats.Count(); d++)
        // {
        //     diagram_menu.transform.Find("Diagram-" + Diagram_List[d]).gameObject.transform.SetSiblingIndex(result[d]);
        // }
        
    }

    void CreateDiagram(string name, GameObject parent)
    {
        if (name != "Diagram"){
            GameObject newDiagram = Instantiate(area_diagram, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0)) as GameObject;
        
            newDiagram.name = name;
            newDiagram.transform.SetParent(parent.transform, true);
            newDiagram.transform.localScale = new Vector3(1f, 1f, 1);
            newDiagram.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);
            newDiagram.GetComponent<RectTransform>().localPosition = Vector3.zero;
        }
       
    }

    public void ViewMatch()
    {
        //_matchInteraction.selected_rally = r_name;
        _matchData.selected_rally = "";
        // show all shots by default
        if (_matchInteraction.shot_outcome != "All"){
            _matchInteraction.shot_outcome = "All";
            _matchInteraction._shotPanel.isShotUpdated = false;
        }
        Debug.Log(_matchInteraction.selected_rally + " was selected in Match");

        _matchInteraction.summaryViewOn = false;
    }
    
    public void UpdateSelection(Transform g, string r_name)
    {
        _matchInteraction.selected_rally = r_name;
        _matchInteraction.selected_shot = "";
        // show all shots by default
        // if (_matchInteraction.shot_outcome != "All"){
        //     _matchInteraction.shot_outcome = "All";
        //     _matchInteraction._shotPanel.isShotUpdated = false;
        // }
        //VideoPlayer vp = g.GetComponent<VideoPlayer>();
        //vp.Stop();
        //vp.Play();
        Debug.Log(r_name + " was selected");

        _matchInteraction.summaryViewOn = false;
    }

    public void RenderVideo()
    {
        Debug.Log("Video Panel - Render Video " + _matchInteraction.selected_rally);

        videoCanvas.transform.Find("Title").GetComponent<Text>().text = "Rally " + _matchData.selected_rally;
        
        videoCanvas.transform.Find("Restart").gameObject.SetActive(!_matchInteraction.summaryViewOn);
        videoCanvas.transform.Find("Back").gameObject.SetActive(!_matchInteraction.summaryViewOn);
        videoCanvas.transform.Find("ViewMatch").gameObject.SetActive(_matchInteraction.summaryViewOn);
        gameObject.transform.Find("Back").gameObject.SetActive(!_matchInteraction.summaryViewOn);

        vp.clip = videoClips.Find(v => v.name == _matchData.selected_rally);

        PlayVideo();
    }
    public void PlayVideo()
    {
        
        //Debug.Log("Video frame: "+ vp.frame + " current step:" + _matchInteraction.currentStep);

        // if (_matchData.matchName.ToString() == "study_match2" ){
        //     if (_matchInteraction.currentStep == 0)
        //     {
        //         Debug.Log("study_match2 restart video" + vp.frameCount);
        //         //vp.Stop();
        //         vp.frame = 0;
        //         vp.Play();
        //     }
        //     else if (!_matchInteraction.loop & (_matchInteraction.currentStep == _matchInteraction.totalStep)) // play once
        //     {
                

        //     } else if (_matchInteraction.currentStep == _matchInteraction.startStep)
        //     {
        //         vp.Play();
        //         vp.frame = _matchInteraction.currentStep / 2;
        //     }
        // } else {
            if (_matchInteraction.currentStep == 0)
            {
                Debug.Log("restart video " + vp.frameCount + " " + _matchInteraction.currentStep);
                vp.Stop();
                vp.frame = 0;
                vp.Play();
            }
            else if (!_matchInteraction.loop & (_matchInteraction.currentStep == _matchInteraction.totalStep)) // play once
            {
                //vp.frame = 0;
                //vp.Stop();

            } else if (_matchInteraction.currentStep == _matchInteraction.startStep)
            {
                vp.Play();
                vp.frame = _matchInteraction.currentStep;
            }
        // }
    }

    public void SummaryView()
    {
        _matchInteraction.selected_rally = "";
        _matchInteraction.summaryViewOn = true;
        // show winner+error shots by default
        if (_matchInteraction.shot_outcome != "Winner+Error"){
            _matchInteraction.shot_outcome = "Winner+Error";
            _matchInteraction._shotPanel.isShotUpdated = false;
        }
    }

    public void RestartVideo()
    {
        _matchInteraction.selected_shot = "";
        _matchInteraction.currentStep = 0;
        _matchInteraction.startStep = 0;
    }
}
