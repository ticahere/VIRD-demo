using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XRTools.Rendering;

/*** script to create shots (from 3d trajectory data & hit_predict) & update filtered shots ***/


public class Shot : MonoBehaviour
{

    //  data
    //public string shotFile = "_hit";
    public string shotFile = "_hit";
    public MatchData _matchData;
    public ShotPanel _shotPanel;

    // shot color
    //public Color c_player_bottom = Color.yellow; // player A  
    //public Color c_player_top = Color.cyan; // player B 
    public Color c_win = Color.cyan; 
    public Color c_error = Color.red;
    public Color c_normal = Color.white;


    // game objects for 3D shot arc - one object for one shot arc 
    public GameObject shotArc; // holding all individual shot arcs
    public GameObject shot_prefab;
    public List<string> shotFilter = new List<string>(); // store all shot names and ball pos

    public List<ShotArc> shotArray = new List<ShotArc>(); // store all shot names and ball pos
    public struct ShotArc // individual shot arc object
    {
        public string shotName;
        public int startStep;
        public int endStep;
        public Vector3[] ballPos;
        public Vector3[] startPos; // player position
        public Vector3[] endPos;
        public GameObject shotLine;
        public int totalRallyStep;
    }
   
    // parse trajectory data to create individual shot arc
    public void RenderShots()
    {
        Debug.Log("Render Shots: " + _matchData.rallyNameFilter.Count); 

        // reset all shots
        shotFilter.Clear();
        shotArray.Clear();
        foreach (Transform child in shotArc.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        //_shotPanel.ResetShotFilter();


        foreach (string t_name in _matchData.rallyNameFilter) 
        {
            if (t_name != "") 
            //if (t_name == "1_02_00_3d") // test with one rally
            {
                TextAsset binAsset = Resources.Load<TextAsset>(_matchData.trajectoryPath + t_name + "_3d");
                string[] posArray = binAsset.text.Split("\n");
                int totalStep = posArray.Length - 2;
                Vector3[] pos = new Vector3[totalStep];

                string shotFilePath = _matchData.matchName.ToString() == "study_match2"? _matchData.shotPath + t_name + shotFile + "_predict_corrected" : _matchData.shotPath + t_name + shotFile + "_predict";
                TextAsset hitAsset = Resources.Load<TextAsset>(shotFilePath); 
                string[] hitArray = hitAsset.text.Split("\n");

                List<int[]> hitIndex = new List<int[]>();

                for (int i = 1; i < totalStep + 1; i++) //skip first row (column name)
                {
                    int hitVal = int.Parse(hitArray[i].Split(",")[1]);
                    //bool isHit = hitVal == 1 | i == 1 | hitVal == 2;
                    bool isHit = hitVal == 1 | hitVal == 2;
                    if (isHit)
                    {
                        hitIndex.Add(new int[] { i, hitVal });
                    }
                }

                // check for winner/errors
                RallySummary rallySum = _matchData.rallySumList.Find(i => i.name == t_name); // shot count in the selected rally
                // Debug.Log("render shot " + t_name);

                int rallyLength = rallySum.shots.Length; // shot count in the selected rally
                string rallyWinner = rallySum.playerWin == "top" ? "B" : "A"; // rally winner: top (B) or bottom (A)
                // Debug.Log(t_name + " Rally Length: " + rallyLength + ", Winner: " + rallyWinner);

                for (int i = 0; i < hitIndex.Count; i++)
                {
                    int currentStep = hitIndex[i][0];
                    int hitVal = hitIndex[i][1]; //  1: bottom, 2: top

                    if (currentStep < totalStep)
                    {
                        
                        int endStep = (i < hitIndex.Count - 1) ? hitIndex[i + 1][0] : totalStep +1;

                        // load ball positions
                        Vector3[] shotPos = new Vector3[endStep - currentStep];
                        int point = 0;
                        bool isValidShot = false; //true;

                        for (int j = currentStep; j < endStep; j++)
                        {
                            float x = float.Parse(posArray[j].Split(",")[1]);
                            float z = float.Parse(posArray[j].Split(",")[2]);
                            float y = float.Parse(posArray[j].Split(",")[3]);
                            //shotPos[point] = new Vector3(x, y, z);
                            
                            if (!(x == -1 & y == -1 & z == -1) & y>=0)
                            {
                                //flip for test_match_3 // adjust for study_match1 rally 2_01_02 & 2_04_02
                                if (_matchData.selected_match.MatchID == 3 | (_matchData.selected_match.MatchID == 1 & (t_name == "2_01_02" | t_name == "2_04_02")))
                                {
                                    Debug.Log("Flip " + t_name);
                                    //flip for test_match_3
                                    shotPos[point] = new Vector3(6.1f - x, y, z);
                                }
                                else shotPos[point] = new Vector3(x, y, z);

                                isValidShot = true;
                            } else if (!(x == -1 & y == -1 & z == -1)){
                                shotPos[point] = new Vector3(x, 0, z); // set y to 0 if negative value is found
                                isValidShot = true;
                            }
                            else
                            {
                                // Debug.Log(t_name + " invalid shot " + i + " x: " + x +  " y: " + y + " z: " + z);
                            }
                           

                            // validity test
                            //if (y < 0) isValidShot = false;
                            //Debug.Log(shotPos[point]);
                            point++;
                        }
                        if (isValidShot)
                        {
                            ShotArc shotObj = new ShotArc();
                            string from = Helper.DetectZoneWithCourt(shotPos[0], hitVal, "start");
                            string landing = Helper.DetectZoneWithCourt(shotPos[shotPos.Length - 1], hitVal, "end");
                            string shotType = ""; 

                            // highlight last two shots
                            Debug.Log("rally length " + t_name + " " + rallyLength);

                            if (i > rallyLength - 3)  shotType = from.Contains(rallyWinner + "-") ? "-Winner" : "-Error";
                            string shotName = "Shot-" + t_name + "-" + i + shotType+ "-from-" + from + "-to-" + landing;

                            shotObj.startStep = currentStep;
                            shotObj.endStep = endStep;
                            shotObj.ballPos = shotPos;
                            shotObj.shotLine = DrawShots(shotPos, shotName, shotType);
                            //shotObj.shotLine = DrawShots(shotPos, t_name + "-" + i, from, landing, shotType);
                            shotObj.shotName = shotName;
                            shotObj.totalRallyStep = totalStep;
                            shotArray.Add(shotObj);
                            shotFilter.Add(shotObj.shotName);
                        }
                       
                    }
                }
                
            }
        }
    
    }

    public void UpdateShotView()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(_matchData.filtered_shots.Contains(child.name));
        }
    }

    public void ToggleShotPoint(bool shotPointOn)
    {
        foreach (Transform child in shotArc.transform)
        {
            GameObject startPoint = child.gameObject.transform.GetChild(0).gameObject;
            GameObject endPoint = child.gameObject.transform.GetChild(1).gameObject;
            startPoint.SetActive(shotPointOn);
            endPoint.SetActive(shotPointOn);
        }

    }

    public void ToggleShotArc(bool shotArcOn)
    {
        foreach (Transform child in shotArc.transform)
        {
            LineRenderer ln = child.gameObject.transform.GetComponent<LineRenderer>();
            ln.enabled = shotArcOn;
        }

    }

    GameObject DrawShots(Vector3[] pos, string shotName, string shotType)
    {
        GameObject shotLine = Instantiate(shot_prefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0)) as GameObject;

        // plot the start & end projected points
        GameObject startPoint = shotLine.gameObject.transform.GetChild(0).gameObject;
        GameObject endPoint = shotLine.gameObject.transform.GetChild(1).gameObject;
        startPoint.transform.position = new Vector3(pos[0].x, 0.02f, pos[0].z); // start
        endPoint.transform.position = new Vector3(pos[pos.Length - 1].x, 0.02f, pos[pos.Length - 1].z); // end
        float y = shotName.Contains("from-A") ? -90f : 90f;
        endPoint.transform.Rotate(0.0f, y, 0.0f);


        LineRenderer lr = shotLine.GetComponent<LineRenderer>();
        lr.positionCount = pos.Length;
        lr.SetPositions(pos);
        
        switch (shotType)
        {
            case "-Winner": 
                lr.SetColors(c_win, c_win);
                break;
            case "-Error":  
                lr.SetColors(c_error, c_error);
                break;
            default:  
                lr.SetColors(c_normal, c_normal);
                break;
        }

        //lr.SetWidth(0.02f, 0.005f);
        lr.SetWidth(0.04f, 0.04f);
        shotLine.name = shotName;
        shotLine.transform.SetParent(shotArc.transform, true);


        return shotLine;
    }
}
