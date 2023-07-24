using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummaryPanel : MonoBehaviour
{
    // data source
    public MatchData _matchData;
    public MatchInteraction _matchInteraction;

    // game info
    string playerB = "Top_";
    string playerA = "Bottom_";
    int[] winnerCount = new int[14]; // Total ([0] Top [1] Bottom), G1i ([2] Top [3] Bottom), G1ii ([4] Top [5] Bottom), G2i ([6] Top [7] Bottom), G2ii ([8] Top [9] Bottom), G3i ([10] Top [11] Bottom), G3ii ([12] Top [13] Bottom)
    // int[] winnerCount = new int[10]; // Total ([0] Top [1] Bottom), G1 ([2] Top [3] Bottom), G2 ([4] Top [5] Bottom), G3i ([6] Top [7] Bottom), G3ii ([8] Top [9] Bottom)
    int[] errorCount = new int[2]; // Unforced errors (Top, Bottom)
    bool rallyLoaded = false;

    // rally filter
    public enum RallyWinner { All, PlayerA, PlayerB }; // A: Bottom, B: Top (player position based on 1st game)
    // public enum Game { G1, G2, G3i, G3ii };
    public enum Game { G1, G1i, G1ii, G2, G2i, G2ii, G3i, G3ii };
    
    public RallyWinner rally_winner = RallyWinner.All;
    private RallyWinner rally_winner_old = RallyWinner.All;
    // public Game game_select = Game.G1;
    private Game game_select_old = Game.G1;
   

    // rally filter UI 
    public GameObject gameFilter_FullSet;
    public GameObject gameFilter_HalfSet;
    public GameObject gameFilter_Game3;
    public bool isRallyUpdated = false;

    void Start()
    {

        // foreach (Transform child in gameFilter.transform)
        // {
        //     Button btn = child.GetComponent<Button>();
        //     string btnName = child.gameObject.name;
        //     btn.onClick.AddListener(() =>
        //     {
        //         _matchInteraction.game_select= btnName.Contains("G1") ? Game.G1 : btnName.Contains("G2") ? Game.G2 : btnName.Contains("_ii") ? Game.G3ii : Game.G3i;
        //         if (btnName.Contains("All"))
        //         {
        //             rally_winner = RallyWinner.All;
        //         }
        //         else
        //         {
        //             if (_matchInteraction.game_select== Game.G1 | _matchInteraction.game_select== Game.G1i | _matchInteraction.game_select== Game.G1ii | _matchInteraction.game_select== Game.G3i){
        //                 rally_winner = btnName.Contains("Bottom") ? RallyWinner.PlayerA : RallyWinner.PlayerB;
        //             } else {
        //                 rally_winner = btnName.Contains("Bottom") ? RallyWinner.PlayerB : RallyWinner.PlayerA;
        //             }
        //         }
        //         Debug.Log("Match click " + btnName + " " + rally_winner);

        //         isRallyUpdated = !((rally_winner != rally_winner_old) | (_matchInteraction.game_select!= game_select_old));
                
        //     });
        // }
        foreach (Transform child in gameFilter_Game3.transform)
        {
            Button btn = child.GetComponent<Button>();
            string btnName = child.gameObject.name;
            btn.onClick.AddListener(() =>
            {
                _matchInteraction.game_select= btnName.Contains("_ii") ? Game.G3ii : Game.G3i;
                if (btnName.Contains("All"))
                {
                    rally_winner = RallyWinner.All;
                }
                else
                {
                    if (_matchInteraction.game_select== Game.G3i){
                        rally_winner = btnName.Contains("Bottom") ? RallyWinner.PlayerA : RallyWinner.PlayerB;
                    } else {
                        rally_winner = btnName.Contains("Bottom") ? RallyWinner.PlayerB : RallyWinner.PlayerA;
                    }
                }
                Debug.Log("Match click " + btnName + " " + rally_winner);

                isRallyUpdated = !((rally_winner != rally_winner_old) | (_matchInteraction.game_select!= game_select_old));
                
            });
        }
        foreach (Transform child in gameFilter_HalfSet.transform)
        {
            Button btn = child.GetComponent<Button>();
            string btnName = child.gameObject.name;
            btn.onClick.AddListener(() =>
            {
                if (btnName.Contains("G1")){
                    _matchInteraction.game_select= btnName.Contains("_ii") ? Game.G1ii : Game.G1i;
                } else  _matchInteraction.game_select= btnName.Contains("_ii") ? Game.G2ii : Game.G2i;
                if (btnName.Contains("All"))
                {
                    rally_winner = RallyWinner.All;
                }
                else
                {
                    if (_matchInteraction.game_select== Game.G1i | _matchInteraction.game_select== Game.G1ii){
                        rally_winner = btnName.Contains("Bottom") ? RallyWinner.PlayerA : RallyWinner.PlayerB;
                    } else {
                        rally_winner = btnName.Contains("Bottom") ? RallyWinner.PlayerB : RallyWinner.PlayerA;
                    }
                }
                Debug.Log("Match click " + btnName + " " + rally_winner);

                isRallyUpdated = !((rally_winner != rally_winner_old) | (_matchInteraction.game_select!= game_select_old));
                
            });
        }
        foreach (Transform child in gameFilter_FullSet.transform)
        {
            Button btn = child.GetComponent<Button>();
            string btnName = child.gameObject.name;
            btn.onClick.AddListener(() =>
            {
                _matchInteraction.game_select= btnName.Contains("G1") ? Game.G1 : Game.G2;
                if (btnName.Contains("All"))
                {
                    rally_winner = RallyWinner.All;
                }
                else
                {
                    if (_matchInteraction.game_select== Game.G1){
                        rally_winner = btnName.Contains("Bottom") ? RallyWinner.PlayerA : RallyWinner.PlayerB;
                    } else {
                        rally_winner = btnName.Contains("Bottom") ? RallyWinner.PlayerB : RallyWinner.PlayerA;
                    }
                }
                Debug.Log("Match click " + btnName + " " + rally_winner);

                isRallyUpdated = !((rally_winner != rally_winner_old) | (_matchInteraction.game_select!= game_select_old));
                
            });
        }
    }

    void Update()
    {
        if (!rallyLoaded)
        {
            LoadRallySummary();
        }
        if (!isRallyUpdated) FilterRallyWinner();

        //  faces user
        var target = gameObject.transform.position - Camera.main.transform.position;
        gameObject.transform.rotation = Quaternion.LookRotation(target);
        gameObject.transform.eulerAngles = new Vector3(0, gameObject.transform.eulerAngles.y, 0);

    }
    void LoadRallySummary()
    {
        Debug.Log("Rally Summary" + _matchData.rallySumList.Count);
        string currentGame = "1";
        bool overHalf = false;

        for (int i = 0; i< _matchData.rallySumList.Count; i++)
        {
            string r_name = _matchData.rallySumList[i].name;
             if (r_name.Substring(0,1) != currentGame){
                //reset overHalf
                overHalf = false;
                currentGame = r_name.Substring(0,1);
            }
            string r_winner = _matchData.rallySumList[i].playerWin;
            string r_last_player = _matchData.rallySumList[i].shots[_matchData.rallySumList[i].shots.Length - 1].playerHit;
            bool isWinner = r_last_player == r_winner;
            bool isError = r_last_player != r_winner;

            string errorType = "";
            // categorize unforced vs. forced errors
            //if (isError) errorType = _matchData.rallySumList[i].shots[_matchData.rallySumList[i].shots.Length - 2].tendency != "Offensive" ? "UFE" : "FE";

            
            // bool overHalf = int.Parse(r_name.Substring(2,2)) > 11 | int.Parse(r_name.Substring(5,2)) > 11 ;

            // assign points to players by game
            if (r_winner == "top") // "TOP" position in the video, so the order is reversed in G2 (and G3ii
            {
                if (r_name.Substring(0,1) == "1") {
                    if (!overHalf) {
                        winnerCount[2] += 1; //G1i top
                    } else winnerCount[4] += 1; //G1ii top
                    winnerCount[0] += 1;
                }
                if (r_name.Substring(0,1) == "2") {
                    // winnerCount[5] += 1; // reverse in G2
                    if (!overHalf) { 
                        winnerCount[7] += 1; // G2i top
                    } else winnerCount[9] += 1; // G2ii top
                    winnerCount[1] += 1;
                }
                if (r_name.Substring(0,1) == "3") {
                    
                    if (!overHalf) { 
                        winnerCount[10] += 1; // G3i top
                        winnerCount[0] += 1;
                    } else { // reverse in G3ii
                        winnerCount[13] += 1; // G3ii top
                        winnerCount[1] += 1;
                    }
                }

            } else if (r_winner == "bottom")
            {
                if (r_name.Substring(0,1) == "1") {
                    if (!overHalf) {
                        winnerCount[3] += 1; // G1i bottom
                    } else winnerCount[5] += 1; // G1ii bottom
                    
                    winnerCount[1] += 1;
                }
                if (r_name.Substring(0,1) == "2") {
                    // winnerCount[4] += 1; // reverse in G2
                    if (!overHalf) {
                        winnerCount[6] += 1; // G2i bottom
                    } else winnerCount[8] += 1; // G2ii bottom
                    winnerCount[0] += 1;
                }
                if (r_name.Substring(0,1) == "3") {
                    if (!overHalf) {// reverse in G3ii
                        winnerCount[11] += 1; //G3i bottom
                        winnerCount[1] += 1;
                    } else {
                        winnerCount[12] += 1; // G3ii bottom
                        winnerCount[0] += 1;
                    }
                }
            }

            if (!overHalf & (int.Parse(r_name.Substring(2,2)) > 10 | int.Parse(r_name.Substring(5,2)) > 10)) {
                overHalf = true;
            }
            
        }
        gameObject.transform.Find("Match/All_Top").GetComponent<TMP_Text>().text = winnerCount[0].ToString();
        gameObject.transform.Find("Match/All_Bottom").GetComponent<TMP_Text>().text = winnerCount[1].ToString();
        //gameFilter.transform.Find("UFE_Top/value").GetComponent<TMP_Text>().text = errorCount[0].ToString();
        //gameFilter.transform.Find("UFE_Bottom/value").GetComponent<TMP_Text>().text = errorCount[1].ToString();

        //  Total ([0] Top [1] Bottom), G1i ([2] Top [3] Bottom), G1ii ([4] Top [5] Bottom), 
        //  G2i ([6] Top [7] Bottom), G2ii ([8] Top [9] Bottom), G3i ([10] Top [11] Bottom), G3ii ([12] Top [13] Bottom)
        gameFilter_FullSet.transform.Find("G1_Top/value").GetComponent<TMP_Text>().text = (winnerCount[2] + winnerCount[4]).ToString();
        gameFilter_FullSet.transform.Find("G1_Bottom/value").GetComponent<TMP_Text>().text = (winnerCount[3] + winnerCount[5]).ToString();
        gameFilter_FullSet.transform.Find("G2_Top/value").GetComponent<TMP_Text>().text = (winnerCount[6] +winnerCount[8]).ToString();
        gameFilter_FullSet.transform.Find("G2_Bottom/value").GetComponent<TMP_Text>().text = (winnerCount[7]+ winnerCount[9]).ToString();

        gameFilter_HalfSet.transform.Find("G1_Top_i/value").GetComponent<TMP_Text>().text = winnerCount[2].ToString();
        gameFilter_HalfSet.transform.Find("G1_Bottom_i/value").GetComponent<TMP_Text>().text = winnerCount[3].ToString();
        gameFilter_HalfSet.transform.Find("G1_Top_ii/value").GetComponent<TMP_Text>().text = winnerCount[4].ToString();
        gameFilter_HalfSet.transform.Find("G1_Bottom_ii/value").GetComponent<TMP_Text>().text = winnerCount[5].ToString();

        gameFilter_HalfSet.transform.Find("G2_Top_i/value").GetComponent<TMP_Text>().text = winnerCount[6].ToString();
        gameFilter_HalfSet.transform.Find("G2_Bottom_i/value").GetComponent<TMP_Text>().text = winnerCount[7].ToString();
        gameFilter_HalfSet.transform.Find("G2_Top_ii/value").GetComponent<TMP_Text>().text = winnerCount[8].ToString();
        gameFilter_HalfSet.transform.Find("G2_Bottom_ii/value").GetComponent<TMP_Text>().text = winnerCount[9].ToString();


        gameFilter_Game3.transform.Find("G3_Top_i/value").GetComponent<TMP_Text>().text = winnerCount[10].ToString();
        gameFilter_Game3.transform.Find("G3_Bottom_i/value").GetComponent<TMP_Text>().text = winnerCount[11].ToString();
        gameFilter_Game3.transform.Find("G3_Top_ii/value").GetComponent<TMP_Text>().text = winnerCount[12].ToString();
        gameFilter_Game3.transform.Find("G3_Bottom_ii/value").GetComponent<TMP_Text>().text = winnerCount[13].ToString();

        gameObject.transform.Find("Match/PlayerBottom/Text").GetComponent<Text>().text = _matchData.selected_match.PlayerBottom;
        gameObject.transform.Find("Match/PlayerTop/Text").GetComponent<Text>().text = _matchData.selected_match.PlayerTop;

        // update match stats
        gameObject.transform.Find("Stats/MatchTime/value").GetComponent<Text>().text = (_matchData.matchLength / 60).ToString("#.#");
        gameObject.transform.Find("Stats/RallyCount/value").GetComponent<Text>().text = _matchData.rallySumList.Count.ToString();
        gameObject.transform.Find("Stats/RallyAvg/value").GetComponent<Text>().text = (_matchData.rallyLength / _matchData.rallySumList.Count).ToString("#.#");
        gameObject.transform.Find("Stats/GameWinner/value").GetComponent<Text>().text = _matchData.selected_match.Winner;

        // update game stats - default G1
        float gameLengthG1 = _matchData.gameLength[0] + _matchData.gameLength[1];
        int rallyCountG1 = _matchData.rallyCount[0] + _matchData.rallyCount[1];
        float gameRallyLengthG1 = _matchData.gameRallyLength[0]+_matchData.gameRallyLength[1];
        gameObject.transform.Find("GameStats/MatchTime/value").GetComponent<Text>().text = (gameLengthG1/ 60).ToString("#.#");
        gameObject.transform.Find("GameStats/RallyCount/value").GetComponent<Text>().text = rallyCountG1.ToString();
        gameObject.transform.Find("GameStats/RallyAvg/value").GetComponent<Text>().text = (gameRallyLengthG1 / rallyCountG1).ToString("#.#");
        // gameObject.transform.Find("GameStats/GameWinner/value").GetComponent<Text>().text = _matchData.selected_match.Winner;


        // update player names by the court
        UpdatePlayerName();

        rallyLoaded = true;
    }
    void FilterRallyWinner()
    {
        //Debug.Log("FilterRallyWinner , game: " + game_select + " winner: " + rally_winner + " isRallyUpdated: "+ isRallyUpdated);
        List<string> filter_rally = new List<string>();
        string currentGame = "1";
        bool overHalf = false;
   
        for (int i = 0; i < _matchData.rallySumList.Count; i++)
        {
            string r_name = _matchData.rallySumList[i].name;
            string g = _matchData.rallySumList[i].name.Substring(0,1);
             if (r_name.Substring(0,1) != currentGame){
                //reset overHalf
                overHalf = false;
                currentGame = r_name.Substring(0,1);
            }
            string r_winner = _matchData.rallySumList[i].playerWin;
            bool isShown = true;

            string r_last_player = _matchData.rallySumList[i].shots[_matchData.rallySumList[i].shots.Length - 1].playerHit;
            bool isWinner = r_last_player == r_winner;
            bool isError = r_last_player != r_winner;
            string errorType = "";

            int playerTopScore = int.Parse(r_name.Substring(2,2));
            int playerBottomScore = int.Parse(r_name.Substring(5,2));

            // bool isFirstHalf = playerTopScore < 12 & playerBottomScore < 12;
           

            // categorize unforced vs. forced errors
            //if (isError) errorType = _matchData.rallySumList[i].shots[_matchData.rallySumList[i].shots.Length - 2].tendency != "Offensive" ? "UFE" : "FE";


            // filter by rally winner
            //if (game_select != Game.All)
            //{
            if (_matchData.matchSplit){
                if (_matchInteraction.game_select== Game.G1i) isShown = g == "1" & !overHalf;
                if (_matchInteraction.game_select== Game.G1ii) isShown = g == "1"  & overHalf;
                if (_matchInteraction.game_select== Game.G2i) isShown = g == "2" & !overHalf;
                if (_matchInteraction.game_select== Game.G2ii) isShown = g == "2"  & overHalf;
            } else {
                if (_matchInteraction.game_select== Game.G1) isShown = g == "1";
                if (_matchInteraction.game_select== Game.G2) isShown = g == "2";
            }
            

            if (_matchInteraction.game_select== Game.G3i)  isShown = g == "3" & !overHalf;
            if (_matchInteraction.game_select== Game.G3ii) isShown = g == "3"  & overHalf;
            //}
            if (isShown)
            {
                if (rally_winner != RallyWinner.All)
                { 
                    if (rally_winner == RallyWinner.PlayerA) isShown = r_winner == "bottom";
                    if (rally_winner == RallyWinner.PlayerB) isShown = r_winner == "top";
                }
            }

             if (!overHalf & (int.Parse(r_name.Substring(2,2)) > 10 | int.Parse(r_name.Substring(5,2)) > 10)) {
                overHalf = true;
            }

            if (isShown) filter_rally.Add(r_name);
        }

        // update game stats
        float gameDuration = 0;//= _matchInteraction.game_select== Game.G1 ? _matchData.gameLength_G1 : _matchInteraction.game_select== Game.G2 ? _matchData.gameLength_G2 : _matchData.gameLength_G3;
        float gameRallyCount= 0; //  = _matchInteraction.game_select== Game.G1 ? _matchData.rallyCount_G1 : _matchInteraction.game_select== Game.G2 ? _matchData.rallyCount_G2 : _matchData.rallyCount_G3;
        float rallyLength = 0;//= _matchInteraction.game_select== Game.G1 ? _matchData.rallyLength_G1 : _matchInteraction.game_select== Game.G2 ? _matchData.rallyLength_G2 : _matchData.rallyLength_G3;
        
        switch(_matchInteraction.game_select){
                case Game.G1i:
                    gameDuration =  _matchData.gameLength[0];
                    gameRallyCount = _matchData.rallyCount[0];
                    rallyLength = _matchData.gameRallyLength[0];
                    break;
                case Game.G1ii:
                    gameDuration =  _matchData.gameLength[1];
                    gameRallyCount = _matchData.rallyCount[1];
                    rallyLength = _matchData.gameRallyLength[1];
                    break;   
                case Game.G1:
                    gameDuration =  _matchData.gameLength[0] + _matchData.gameLength[1];
                    gameRallyCount = _matchData.rallyCount[0] + _matchData.rallyCount[1];
                    rallyLength = _matchData.gameRallyLength[0] + _matchData.gameRallyLength[1];
                    break; 
                case Game.G2i:
                    gameDuration =  _matchData.gameLength[2];
                    gameRallyCount = _matchData.rallyCount[2];
                    rallyLength = _matchData.gameRallyLength[2];
                    break;
                case Game.G2ii:
                    gameDuration =  _matchData.gameLength[3];
                    gameRallyCount = _matchData.rallyCount[3];
                    rallyLength = _matchData.gameRallyLength[3];
                    break;   
                case Game.G2:
                    gameDuration =  _matchData.gameLength[2] + _matchData.gameLength[3];
                    gameRallyCount = _matchData.rallyCount[2] + _matchData.rallyCount[3];
                    rallyLength = _matchData.gameRallyLength[2] + _matchData.gameRallyLength[3];
                    break; 
                case Game.G3i:
                    gameDuration =  _matchData.gameLength[4];
                    gameRallyCount = _matchData.rallyCount[4];
                    rallyLength = _matchData.gameRallyLength[4];
                    break;
                case Game.G3ii:
                    gameDuration =  _matchData.gameLength[5];
                    gameRallyCount = _matchData.rallyCount[5];
                    rallyLength = _matchData.gameRallyLength[5];
                    break;
                default:
                    break; 
        }

        gameObject.transform.Find("GameStats/Stats_Game").GetComponent<Text>().text = _matchInteraction.game_select.ToString();
            
        gameObject.transform.Find("GameStats/MatchTime/value").GetComponent<Text>().text = gameDuration != 0 ? (gameDuration / 60).ToString("#.#") : "0";
        gameObject.transform.Find("GameStats/RallyCount/value").GetComponent<Text>().text = gameRallyCount.ToString();
        gameObject.transform.Find("GameStats/RallyAvg/value").GetComponent<Text>().text = gameRallyCount != 0 ? (rallyLength / gameRallyCount).ToString("#.#") : "0";
        gameObject.transform.Find("GameStats/GameWinner/value").GetComponent<Text>().text = _matchData.selected_match.Winner;

        // update player names by the court: switch side in G2 and G3-part2
        UpdatePlayerName();
       
        // remain selected status
        // foreach (Transform child in gameFilter.transform)
        // {
        //     bool btnActive = false;
        //     string btnName = child.gameObject.name;
        //     btnActive = btnName.Contains(_matchData.game_select.ToString());

        //     if (btnActive)
        //     {
        //         if (_matchData.game_select == Game.G1 | _matchData.game_select == Game.G1i | _matchData.game_select == Game.G1ii |_matchData.game_select == Game.G3i){
        //             if (rally_winner == RallyWinner.PlayerA) btnActive = btnName.Contains("Bottom");
        //             if (rally_winner == RallyWinner.PlayerB) btnActive = btnName.Contains("Top");
        //         } else {
        //             if (rally_winner == RallyWinner.PlayerA) btnActive = btnName.Contains("Top");
        //             if (rally_winner == RallyWinner.PlayerB) btnActive = btnName.Contains("Bottom");
        //         }
               
        //         if (rally_winner == RallyWinner.All)
        //         {
        //             //btnActive = btnName == "All";
        //             btnActive = btnName.Contains("All");
        //         }
        //     }
           
        //     child.GetComponent<Button>().interactable = !btnActive;
        // }
        if (_matchData.matchSplit) {
             foreach (Transform child in gameFilter_HalfSet.transform)
            {
                bool btnActive = false;
                string btnName = child.gameObject.name;
                btnActive = btnName.Contains(_matchData.game_select.ToString());

                if (btnActive)
                {
                    if (_matchData.game_select == Game.G1i | _matchData.game_select == Game.G1ii){
                        if (rally_winner == RallyWinner.PlayerA) btnActive = btnName.Contains("Bottom");
                        if (rally_winner == RallyWinner.PlayerB) btnActive = btnName.Contains("Top");
                    } else {
                        if (rally_winner == RallyWinner.PlayerA) btnActive = btnName.Contains("Top");
                        if (rally_winner == RallyWinner.PlayerB) btnActive = btnName.Contains("Bottom");
                    }
                
                    if (rally_winner == RallyWinner.All)
                    {
                        //btnActive = btnName == "All";
                        btnActive = btnName.Contains("All");
                    }
                }
            
                child.GetComponent<Button>().interactable = !btnActive;
            }
        } else {
            foreach (Transform child in gameFilter_FullSet.transform)
            {
                bool btnActive = false;
                string btnName = child.gameObject.name;
                btnActive = btnName.Contains(_matchData.game_select.ToString());

                if (btnActive)
                {
                    if (_matchData.game_select == Game.G1){
                        if (rally_winner == RallyWinner.PlayerA) btnActive = btnName.Contains("Bottom");
                        if (rally_winner == RallyWinner.PlayerB) btnActive = btnName.Contains("Top");
                    } else {
                        if (rally_winner == RallyWinner.PlayerA) btnActive = btnName.Contains("Top");
                        if (rally_winner == RallyWinner.PlayerB) btnActive = btnName.Contains("Bottom");
                    }
                
                    if (rally_winner == RallyWinner.All)
                    {
                        //btnActive = btnName == "All";
                        btnActive = btnName.Contains("All");
                    }
                }
            
                child.GetComponent<Button>().interactable = !btnActive;
            }
        }
        
       
        foreach (Transform child in gameFilter_Game3.transform)
        {
            bool btnActive = false;
            string btnName = child.gameObject.name;
            btnActive = btnName.Contains(_matchData.game_select.ToString());

            if (btnActive)
            {
                if (_matchData.game_select == Game.G3i){
                    if (rally_winner == RallyWinner.PlayerA) btnActive = btnName.Contains("Bottom");
                    if (rally_winner == RallyWinner.PlayerB) btnActive = btnName.Contains("Top");
                } else {
                    if (rally_winner == RallyWinner.PlayerA) btnActive = btnName.Contains("Top");
                    if (rally_winner == RallyWinner.PlayerB) btnActive = btnName.Contains("Bottom");
                }
               
                if (rally_winner == RallyWinner.All)
                {
                    //btnActive = btnName == "All";
                    btnActive = btnName.Contains("All");
                }
            }
           
            child.GetComponent<Button>().interactable = !btnActive;
        }

        _matchInteraction.rallyNameFilter = filter_rally;
        
        rally_winner_old = rally_winner;
        game_select_old = _matchData.game_select;

        isRallyUpdated = true;
    }

    void UpdatePlayerName(){
        // update player names by the court: switch side in G2 and G3-part2
        if (_matchInteraction.game_select== Game.G1 | _matchData.game_select == Game.G1i | _matchData.game_select == Game.G1ii | _matchInteraction.game_select== Game.G3i){
            GameObject.Find("PlayerBottom/name").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerBottom;
            GameObject.Find("PlayerBottom1/name").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerBottom;
            GameObject.Find("PlayerBottom/name").GetComponent<TMP_Text>().color =  Helper.bottom_player_color;
            GameObject.Find("PlayerBottom1/name").GetComponent<TMP_Text>().color =  Helper.bottom_player_color;

            GameObject.Find("PlayerTop/name").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerTop;
            GameObject.Find("PlayerTop1/name").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerTop;
            GameObject.Find("PlayerTop/name").GetComponent<TMP_Text>().color =  Helper.top_player_color;
            GameObject.Find("PlayerTop1/name").GetComponent<TMP_Text>().color =  Helper.top_player_color;

        } else {
            GameObject.Find("PlayerBottom/name").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerTop;
            GameObject.Find("PlayerBottom1/name").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerTop;
            GameObject.Find("PlayerBottom/name").GetComponent<TMP_Text>().color =  Helper.top_player_color;
            GameObject.Find("PlayerBottom1/name").GetComponent<TMP_Text>().color =  Helper.top_player_color;

            GameObject.Find("PlayerTop/name").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerBottom;
            GameObject.Find("PlayerTop1/name").GetComponent<TMP_Text>().text = _matchData.selected_match.PlayerBottom;
            GameObject.Find("PlayerTop/name").GetComponent<TMP_Text>().color =  Helper.bottom_player_color;
            GameObject.Find("PlayerTop1/name").GetComponent<TMP_Text>().color =  Helper.bottom_player_color;
        }
    }

    public void toggleMatchSplit(){
        bool update_matchSplit = !_matchData.matchSplit;
        isRallyUpdated = false;
        //reset match split interface
        if(update_matchSplit){
            gameFilter_FullSet.SetActive(false);
            gameFilter_HalfSet.SetActive(true);
            _matchInteraction.game_select = Game.G1i;
        } else{
            gameFilter_FullSet.SetActive(true);
            gameFilter_HalfSet.SetActive(false);
            _matchInteraction.game_select = Game.G1;
        }
        _matchData.matchSplit = update_matchSplit;
    }
}
