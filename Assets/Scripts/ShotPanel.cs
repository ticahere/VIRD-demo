using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*** script to filter shot data through 1) Shot Panel, 2) Court Position Interaction ***/

public class ShotPanel : MonoBehaviour
{
    
    public MatchData _matchData;
    public MatchInteraction _matchInteraction;

    // location filter
    private string cam_zone_old = "";
    public string cam_zone = "";
    public bool enableBodyFilter = false;

    // shot filter UI 
    public GameObject filter;
    public GameObject locationAll;
    public GameObject locationHalf;
    public GameObject outcomeFilter;
    public GameObject locationFilter;

    public bool isShotUpdated = false;

    // location filter
    public enum CourtArea { All, FrontLeft, MidLeft, BackLeft, FrontRight, MidRight, BackRight, NotInCourt };
    public enum CourtSide { All, A, B };  // A: Bottom, B: Top (player position based on 1st game)
    public CourtArea selectedCourtArea = CourtArea.All;
    public CourtSide selectedCourtSide = CourtSide.All;

    // Shot arc
    public GameObject shotArc;
    private int shotCount = 0;


    void Start()
    {
        foreach (Transform child in filter.transform) // shotby filter
        {
            Button btn = child.GetComponent<Button>();
            string btnName = child.gameObject.name;
            btn.onClick.AddListener(() =>
            {
                Debug.Log("shot by clicked " + btnName + " " + isShotUpdated);
                if (btnName != _matchInteraction.shot_by){
                    if (btnName == "All") _matchInteraction.selectedArea = "All";
                    UpdateShotLocationFilterType(btnName);

                }
                isShotUpdated = btnName == _matchInteraction.shot_by;

                _matchInteraction.shot_by = btnName; 
                if (selectedCourtArea.ToString() != "All") Invoke("ResetLocFilter", 0.01f);
            });
        }

        foreach (Transform child in outcomeFilter.transform) // outcome filter
        {
            Button btn = child.GetComponent<Button>();
            string btnName = child.gameObject.name;
            gameObject.transform.Find("ShotFilter/FilterResults/FilterOutcome/Text").GetComponent<Text>().text = _matchInteraction.shot_outcome + " Shots";
            btn.onClick.AddListener(() =>
            {
                isShotUpdated = btnName == _matchInteraction.shot_outcome;
                _matchInteraction.shot_outcome = btnName;
                
                gameObject.transform.Find("ShotFilter/FilterResults/FilterOutcome/Text").GetComponent<Text>().text = _matchInteraction.shot_outcome + " Shots";
            });
        }

        foreach (Transform child in locationAll.transform) // shot direction
        {
            Button btn = child.GetComponent<Button>();
            string btnName = child.gameObject.name;
            if (!btnName.Contains("Player")){
                btn.onClick.AddListener(() => {
                    isShotUpdated = btnName == _matchInteraction.shot_dir;
                    _matchInteraction.shot_dir = btnName;
                });
            }
        }
        
        foreach (Transform child in locationFilter.transform.Find("Areas_Half").transform) // location filter
        {
            if (child.name != "Court_From_Half" & child.name != "Court_To_Half" ){
                Button btn = child.GetComponent<Button>();
                string btnName = child.gameObject.name;

                btn.onClick.AddListener(() => {
                    Debug.Log("court click " + btnName);


                    string tempSelectedArea = btnName.Substring(0, btnName.Length - 1);
                    string tempSelectedSide = btnName.Substring(btnName.Length - 1);


                    if (_matchInteraction.shot_by == "PlayerB"){ // reverse selected side when filter by Player B (on the opposite side)
                        tempSelectedSide = tempSelectedSide == "A" ? "B" : "A";
                    }
                    // if (tempSelectedArea != selectedCourtArea.ToString() | tempSelectedSide != selectedCourtSide.ToString()){
                        _matchInteraction.locationFilterOn = true;

                        selectedCourtArea = tempSelectedArea != selectedCourtArea.ToString() | tempSelectedSide != selectedCourtSide.ToString() ? (CourtArea)System.Enum.Parse( typeof(CourtArea), tempSelectedArea ) : CourtArea.All;
                        // selectedCourtArea = (CourtArea)System.Enum.Parse( typeof(CourtArea), tempSelectedArea );
                        selectedCourtSide = (CourtSide)System.Enum.Parse( typeof(CourtSide), tempSelectedSide );
                        _matchInteraction.selectedArea = selectedCourtArea.ToString();
                        _matchInteraction.selectedSide = selectedCourtSide.ToString();
                        
                        ApplyLocationFilter(selectedCourtArea.ToString(), selectedCourtSide.ToString());
                        FilterShot();
                    // } 
                });
            }
        }

        Button resetLocationFilter = locationFilter.transform.Find("Reset").GetComponent<Button>();
        resetLocationFilter.onClick.AddListener(() =>
        {
            ResetLocFilter();
        });

        _matchInteraction.heatmapOn = gameObject.transform.Find("ShotFilter/Heatmap Toggle").GetComponent<Toggle>().isOn;
        // UpdatePlayerName();
        Invoke("UpdatePlayerName", 0.2f);
        UpdateShotLocationFilterType("All");

        isShotUpdated = false;
    }

    void OnValidate() {
            Debug.Log("Inspector has changed " + selectedCourtArea.ToString() + " " + selectedCourtSide.ToString());

            // update location filter
            if (_matchInteraction.selectedArea != selectedCourtArea.ToString() | _matchInteraction.selectedSide != selectedCourtSide.ToString()){
                _matchInteraction.locationFilterOn = true;
                
                _matchInteraction.selectedArea = selectedCourtArea.ToString();
                _matchInteraction.selectedSide = selectedCourtSide.ToString();
                
                ApplyLocationFilter(selectedCourtArea.ToString(), selectedCourtSide.ToString());
                FilterShot();
            }
        
    }
    
    void Update()
    {
        // detect user position on court
        cam_zone = Helper.DetectZone(Camera.main.transform.position);
        if (enableBodyFilter)
        {
            isShotUpdated = cam_zone == cam_zone_old;
        }
        //_matchInteraction.selectedArea = cam_zone;

        //if (cam_zone != cam_zone_old | !isShotUpdated)
        if (!isShotUpdated)
        {
            Debug.Log("Shot update");
            // update shot_by or shot_from filter 
            cam_zone_old = cam_zone;

            // reset selection when other shot filters are clicked
            if (_matchInteraction.summaryViewOn)
            {
                _matchInteraction.selected_rally = "";
            }
            _matchInteraction.selected_shot = "";



            if (_matchInteraction.shot_by == "All")
            {
                //Debug.Log("location filter off");
                _matchInteraction.shot_from = "All";
                _matchInteraction.shot_to = "All";
                _matchInteraction.locationFilterOn = false;

            }
            else
            {
                //Debug.Log("location filter on");
                _matchInteraction.locationFilterOn = true;

                if (enableBodyFilter)
                {
                    string tempSelectedArea = !cam_zone.Contains("NotInCourt") ? cam_zone.Substring(2) : "All";
                    string tempSelectedSide = cam_zone.Substring(0, 1);

                    if (tempSelectedArea != selectedCourtArea.ToString() | tempSelectedSide != selectedCourtSide.ToString()){
                        selectedCourtArea = (CourtArea)System.Enum.Parse( typeof(CourtArea), tempSelectedArea );
                        selectedCourtSide = (CourtSide)System.Enum.Parse( typeof(CourtSide), tempSelectedSide );
                        _matchInteraction.selectedArea = selectedCourtArea.ToString();
                        _matchInteraction.selectedSide = selectedCourtSide.ToString();
                    }
                    
                }
                // ApplyLocationFilter(selectedArea, selectedSide);
                ApplyLocationFilter(selectedCourtArea.ToString(), selectedCourtSide.ToString());

            }
            FilterShot();

            // remain select status
            foreach (Transform child in filter.transform)
            {
                bool btnActive = true;
                if (child.GetComponent<Button>().gameObject.name == _matchInteraction.shot_by) btnActive = false;
                child.GetComponent<Button>().interactable = btnActive;
            }
            foreach (Transform child in outcomeFilter.transform)
            {
                bool btnActive = true;
                if (child.GetComponent<Button>().gameObject.name == _matchInteraction.shot_outcome) btnActive = false;
                child.GetComponent<Button>().interactable = btnActive;
            }
            foreach (Transform child in locationAll.transform)
            {
                bool btnActive = true;
                if (child.GetComponent<Button>().gameObject.name == _matchInteraction.shot_dir | _matchInteraction.shot_by != "All") btnActive = false;
                child.GetComponent<Button>().interactable = btnActive;
            }
            
        }

        //  faces user
        var target = gameObject.transform.position - Camera.main.transform.position;
        gameObject.transform.rotation = Quaternion.LookRotation(target);
        gameObject.transform.eulerAngles = new Vector3(0, gameObject.transform.eulerAngles.y, 0);

    }
    void ApplyLocationFilter(string court_area, string side)
    {
        Debug.Log("Apply Location Filter: " + court_area + " " + side);
        if (_matchInteraction.shot_by == "PlayerA")
        {
            if (side == "A") // in court A: filter "fromA" 
            //if (cam_zone.Contains("A-")) // in court A: filter "fromA" 
            {
                _matchInteraction.shot_from = court_area;
                _matchInteraction.shot_to = "All";
            }
            else // in court B: filter "toB" 
            {
                _matchInteraction.shot_to = court_area;
                _matchInteraction.shot_from = "All";
            }
        }
        else if (_matchInteraction.shot_by == "PlayerB")
        {
            if (side == "B")
            //if (cam_zone.Contains("B-")) // in court A: filter "fromB" 
            {
                _matchInteraction.shot_from = court_area;
                _matchInteraction.shot_to = "All";
            }
            else // in court A: filter "toA" 
            {
                _matchInteraction.shot_to = court_area;
                _matchInteraction.shot_from = "All";
            }
        }
        else {
            _matchInteraction.shot_from = "All";
            _matchInteraction.shot_to = "All";
        }
    }

    void FilterShot() // filter shots based on location
    {
        Debug.Log("Filter shot- by " + _matchInteraction.shot_by + " , outcome " + _matchInteraction.shot_outcome + " ,selected rally " + _matchInteraction.selected_rally);
        
        List<string> filter_shot = new List<string>();
        shotCount = 0;

        foreach (Transform child in shotArc.transform)
        {
            bool showShot = true;

            // filter shot visibility by outcome
            if (_matchInteraction.shot_outcome != "All")
            {
                if (child.name.Contains("-Winner") | child.name.Contains("-Error"))
                {
                    if (_matchInteraction.shot_outcome == "Winner") showShot = child.name.Contains("-Winner");
                    if (_matchInteraction.shot_outcome == "Error") showShot = child.name.Contains("-Error");
                }
                else showShot = false;
            }

            // filter shot visibility
            if (_matchInteraction.shot_by == "PlayerA")
            {
                if (!child.name.Contains("-from-A"))
                {
                    showShot = false;
                }
            }
            else if (_matchInteraction.shot_by == "PlayerB")
            {
                if (!child.name.Contains("-from-B"))
                {
                    showShot = false;
                }
            }

            if (_matchInteraction.shot_from != "All")
            {
                if (_matchInteraction.shot_by == "PlayerA")
                {
                    if (!child.name.Contains("-from-A-" + _matchInteraction.shot_from)) showShot = false;
                }
                else if (_matchInteraction.shot_by == "PlayerB")
                {
                    if (!child.name.Contains("-from-B-" + _matchInteraction.shot_from)) showShot = false;
                }
                else
                {
                    if (!(child.name.Contains("-from-A-" + _matchInteraction.shot_from) | child.name.Contains("-from-B-" + _matchInteraction.shot_from)))
                    {
                        showShot = false;
                    }
                }
            }

            if (_matchInteraction.shot_to != "All")
            {
                if (_matchInteraction.shot_by == "PlayerA")
                {
                    if (!child.name.Contains("-to-B-" + _matchInteraction.shot_to)) showShot = false;
                }
                else if (_matchInteraction.shot_by == "PlayerB")
                {
                    if (!child.name.Contains("-to-A-" + _matchInteraction.shot_to)) showShot = false;
                }
                else
                {
                    if (!(child.name.Contains("-to-A-" + _matchInteraction.shot_to) | child.name.Contains("-to-B-" + _matchInteraction.shot_to)))
                    {
                        showShot = false;
                    }
                }

            }
            if (showShot) {
                filter_shot.Add(child.name);
            }
        }

        _matchInteraction.shotNameFilter = filter_shot;
        
        
        isShotUpdated = true;

    }

    public void UpdateLocationFilter()
    {
        string startSide = _matchInteraction.shot_by == "All" ? "All" : _matchInteraction.shot_by.Substring(_matchInteraction.shot_by.Length - 1); //All, A or B
        string startArea = _matchInteraction.shot_from; // e.g., MidLeft
        string endSide = _matchInteraction.shot_by == "All" ? "All" : startSide == "A" ? "B" : "A";
        string endArea = _matchInteraction.shot_to;

        string locationFilter = (startArea != "All") ? (startSide + "-" + startArea) : (endSide + "-" + endArea);

        // assign values to filter result row
        gameObject.transform.Find("ShotFilter/FilterResults/FilterLocation/Area").GetComponent<Text>().text = (startSide == "All" & endSide == "All" | (locationFilter.Substring(2) == "All")) ? "All Areas" : locationFilter.Substring(2);
        
        string filterDir = (startSide == "All" & endSide == "All" | locationFilter.Contains("All")) ? "" : locationFilter.Substring(0, 1) == startSide ? "From" : "To";
        gameObject.transform.Find("ShotFilter/FilterResults/FilterLocation/Direction/Text").GetComponent<Text>().text = filterDir;
        gameObject.transform.Find("ShotFilter/FilterResults/FilterLocation/Direction").GetComponent<Image>().color = filterDir == "From" ? Helper.from_hot : filterDir == "To" ? Helper.to_hot : Color.clear;

        if (_matchData.game_select == SummaryPanel.Game.G1 | _matchData.game_select == SummaryPanel.Game.G1i | _matchData.game_select == SummaryPanel.Game.G1ii| _matchData.game_select == SummaryPanel.Game.G3i){
            gameObject.transform.Find("ShotFilter/FilterResults/FilterPlayer/Text").GetComponent<Text>().text = startSide == "All" ? "All Players" : startSide == "A" ? _matchData.selected_match.PlayerBottom : _matchData.selected_match.PlayerTop;
            gameObject.transform.Find("ShotFilter/FilterResults/FilterPlayer/Text").GetComponent<Text>().color = startSide == "All" ? Color.white : startSide == "A" ? Helper.bottom_player_color : Helper.top_player_color ;
        } else {
            gameObject.transform.Find("ShotFilter/FilterResults/FilterPlayer/Text").GetComponent<Text>().text = startSide == "All" ? "All Players" : startSide == "A" ? _matchData.selected_match.PlayerTop : _matchData.selected_match.PlayerBottom;
            gameObject.transform.Find("ShotFilter/FilterResults/FilterPlayer/Text").GetComponent<Text>().color = startSide == "All" ? Color.white : startSide == "A" ? Helper.top_player_color : Helper.bottom_player_color ;
        }

        

        //color main diagram
        foreach (string side in new string[] { "A", "B" })
        {
            for (int i = 0; i < Helper.courtArea.Length; i++)
            {
                string area = Helper.courtArea[i];
                bool showArea = startArea == "All" ? true : side == startSide ? area == startArea : true;
                
                float shotPerc = side == "A" ? _matchData.shotPercA[i] : _matchData.shotPercB[i];
                float shotPerc_To = side == "A" ? _matchData.shotPercA_To[i] : _matchData.shotPercB_To[i];
                
                string courtDir = startSide == "All" ? _matchInteraction.shot_dir : side == startSide ? "From" : "To";
                Color areaColor = Helper.GetShotPercColor(shotPerc, courtDir);
                
                // gameObject.transform.Find("ShotFilter/LocationFilter/Areas/" + area + side).GetComponent<Image>().color = areaColor;
                // gameObject.transform.Find("ShotFilter/LocationFilter/Areas/" + area + side + "/Value").GetComponent<TMP_Text>().text = shotPerc == shotPerc ? shotPerc == 0 ? "" : (Mathf.Round(shotPerc * 100)).ToString() + "%" : "";

                if (_matchInteraction.shot_by == "All"){
                    // color Areas_From
                    Color areaFromColor = Helper.GetShotPercColor(shotPerc, "From");
                    gameObject.transform.Find("ShotFilter/LocationFilter/Areas_From/" + area + side).GetComponent<Image>().color = areaFromColor;
                    gameObject.transform.Find("ShotFilter/LocationFilter/Areas_From/" + area + side + "/Value").GetComponent<TMP_Text>().text = shotPerc == shotPerc ? shotPerc == 0 ? "" : (Mathf.Round(shotPerc * 100)).ToString() + "%" : "";
                    // color Areas_To
                    Color areaToColor = Helper.GetShotPercColor(shotPerc_To, "To");
                    gameObject.transform.Find("ShotFilter/LocationFilter/Areas_To/" + area + side).GetComponent<Image>().color = areaToColor;
                    gameObject.transform.Find("ShotFilter/LocationFilter/Areas_To/" + area + side + "/Value").GetComponent<TMP_Text>().text = shotPerc_To == shotPerc_To ? shotPerc_To == 0 ? "" : (Mathf.Round(shotPerc_To * 100)).ToString() + "%" : "";
                
                } else {
                    if (_matchInteraction.shot_by == "PlayerA") { // For player A, side A is "FROM", side B is "TO"
                        // Court_From (original A) ; Court_To (original B)
                        gameObject.transform.Find("ShotFilter/LocationFilter/Areas_Half/" + area + side).GetComponent<Image>().color = areaColor;
                        gameObject.transform.Find("ShotFilter/LocationFilter/Areas_Half/" + area + side + "/Value").GetComponent<TMP_Text>().text = shotPerc == shotPerc ? shotPerc == 0 ? "" : (Mathf.Round(shotPerc * 100)).ToString() + "%" : "";
                    } else if (_matchInteraction.shot_by == "PlayerB") { // For player A, side A is "TO", side B is "FROM"
                        Debug.Log("Reverse " + side + " shot by" + _matchInteraction.shot_by);
                        string inverseSide = (side == "A") ? "B" : "A";
                        gameObject.transform.Find("ShotFilter/LocationFilter/Areas_Half/" + area + inverseSide).GetComponent<Image>().color = areaColor;
                        gameObject.transform.Find("ShotFilter/LocationFilter/Areas_Half/" + area + inverseSide + "/Value").GetComponent<TMP_Text>().text = shotPerc == shotPerc ? shotPerc == 0 ? "" : (Mathf.Round(shotPerc * 100)).ToString() + "%" : "";
                    }
                }
            }
        }
        // update shot count number
        gameObject.transform.Find("ShotFilter/LocationFilter/ShotCount/Value").GetComponent<Text>().text = _matchData.filtered_shots.Count.ToString();

    }

    public void UpdatePlayerName(){
        Debug.Log("Update player name " + _matchData.game_select.ToString());

        if (_matchData.game_select == SummaryPanel.Game.G1 | _matchData.game_select == SummaryPanel.Game.G1i | _matchData.game_select == SummaryPanel.Game.G1ii| _matchData.game_select == SummaryPanel.Game.G3i){

            filter.transform.Find("PlayerA/Text").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerBottom;
            filter.transform.Find("PlayerA/Text").GetComponent<TMP_Text>().color =  Helper.bottom_player_color;
            filter.transform.Find("PlayerB/Text").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerTop;
            filter.transform.Find("PlayerB/Text").GetComponent<TMP_Text>().color =  Helper.top_player_color;

            locationAll.transform.Find("PlayerA/Text").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerBottom;
            locationAll.transform.Find("PlayerA/Text").GetComponent<TMP_Text>().color =  Helper.bottom_player_color;
            locationAll.transform.Find("PlayerB/Text").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerTop;
            locationAll.transform.Find("PlayerB/Text").GetComponent<TMP_Text>().color =  Helper.top_player_color;

            locationHalf.transform.Find("PlayerA/Text").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerBottom;
            locationHalf.transform.Find("PlayerA/Text").GetComponent<TMP_Text>().color =  Helper.bottom_player_color;
            locationHalf.transform.Find("PlayerB/Text").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerTop;
            locationHalf.transform.Find("PlayerB/Text").GetComponent<TMP_Text>().color =  Helper.top_player_color;
        } else {
            filter.transform.Find("PlayerA/Text").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerTop;
            filter.transform.Find("PlayerA/Text").GetComponent<TMP_Text>().color =  Helper.top_player_color;
            filter.transform.Find("PlayerB/Text").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerBottom;
            filter.transform.Find("PlayerB/Text").GetComponent<TMP_Text>().color =  Helper.bottom_player_color;

            locationAll.transform.Find("PlayerA/Text").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerTop;
            locationAll.transform.Find("PlayerA/Text").GetComponent<TMP_Text>().color =  Helper.top_player_color;
            locationAll.transform.Find("PlayerB/Text").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerBottom;
            locationAll.transform.Find("PlayerB/Text").GetComponent<TMP_Text>().color =  Helper.bottom_player_color;

            locationHalf.transform.Find("PlayerA/Text").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerTop;
            locationHalf.transform.Find("PlayerA/Text").GetComponent<TMP_Text>().color =  Helper.top_player_color;
            locationHalf.transform.Find("PlayerB/Text").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerBottom;
            locationHalf.transform.Find("PlayerB/Text").GetComponent<TMP_Text>().color =  Helper.bottom_player_color;
        }
    }

    void UpdateShotLocationFilterType(string shot_by){
         // select locationAll components to show
            if (shot_by == "All"){
                locationAll.SetActive(true);
                locationHalf.SetActive(false);
                gameObject.transform.Find("ShotFilter/LocationFilter/Areas_From/").gameObject.SetActive(true);
                gameObject.transform.Find("ShotFilter/LocationFilter/Areas_To/").gameObject.SetActive(true);
                gameObject.transform.Find("ShotFilter/LocationFilter/Areas_Half/").gameObject.SetActive(false);
                gameObject.transform.Find("ShotFilter/LocationFilter/Reset/").gameObject.SetActive(false);
            } else {
                locationAll.SetActive(false);
                locationHalf.SetActive(true);
                gameObject.transform.Find("ShotFilter/LocationFilter/Areas_From/").gameObject.SetActive(false);
                gameObject.transform.Find("ShotFilter/LocationFilter/Areas_To/").gameObject.SetActive(false);
                gameObject.transform.Find("ShotFilter/LocationFilter/Areas_Half/").gameObject.SetActive(true);
                gameObject.transform.Find("ShotFilter/LocationFilter/Reset/").gameObject.SetActive(true);
                
                locationHalf.transform.Find("PlayerA").gameObject.SetActive(shot_by == "PlayerA");
                locationHalf.transform.Find("PlayerB").gameObject.SetActive(shot_by == "PlayerB");
            }

    }

    public void ResetLocFilter(){
         Debug.Log("reset location filter");
          // update location filter
           
        if (selectedCourtArea.ToString() != "All"){
             selectedCourtArea = CourtArea.All;
             _matchInteraction.selectedArea = "All";
            isShotUpdated = false;

            // FilterShot();
        }
    }

    public void ToggleHeatmap()
    {
        //Debug.Log("toggle heat map");
        _matchInteraction.heatmapOn = !_matchInteraction.heatmapOn;
        gameObject.transform.Find("ShotFilter/Heatmap Toggle").GetComponent<Toggle>().graphic.canvasRenderer.SetAlpha(_matchInteraction.heatmapOn ? 1f : 0f);

    }

    public void ToggleShotPoint() // show/hide player projected position
    {
        _matchInteraction.shotPointOn = !_matchInteraction.shotPointOn;
    }

    public void ToggleShotArc() // show/hide shot arc
    {
        _matchInteraction.shotArcOn = !_matchInteraction.shotArcOn;
    }

}
