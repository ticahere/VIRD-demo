using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Trajectory : MonoBehaviour
{

    //  data
    public MatchData _matchData;
    public MatchInteraction _matchInteraction;
    private int trajectory_id = 0;

    // setting
    bool isLoaded = false;
    // player
    string[] _bodyToModify = new string[] { "pelvis","left_hip","right_hip","spine1","left_knee","right_knee","spine2","left_ankle","right_ankle","spine3", "left_foot","right_foot","neck","left_collar","right_collar","head","left_shoulder","right_shoulder","left_elbow", "right_elbow","left_wrist","right_wrist","left_index1","right_index1" };
    Dictionary<string, Transform> _transformFromNamePlayerA;
    Dictionary<string, Transform> _transformFromNamePlayerB;
    public Quaternion[,] playerAPoses;
    public Quaternion[,] playerBPoses;
    public Vector3[] playerAPosition;
    public Vector3[] playerBPosition;

    // time control
    float time = 0.0f;

    // game objects for 3D trajectory
    public GameObject shuttleCock;
    public Vector3[] ballPos;

    // player position control
    float y_offset = 2.2f; //1.3589f; 
    bool showPose = false;

    // player
    public GameObject playerA; 
    public GameObject playerB;  
    public GameObject playerA_match0; // Yamaguchi - 156cm  -- 165 * 0.95
    public GameObject playerB_match0; // Marin - 172.7cm -- 165 * 1.047
    public GameObject playerA_match1; // Axelsen - 194cm --
    public GameObject playerB_match1; // Momota - 175cm --
    public Material material_playerA_match0; // Yamaguchi: blue
    public Material material_playerB_match0; // Marin: white
    public Material material_playerA_match1; // Axelsen: white
    public Material material_playerB_match1;  // Momota: blue
    public Material material_playerA_match2; // Castillo: red
    public Material material_playerB_match2;  // Ma: blue

    // player height
    float playerAHeight_match0 = 156f / 165f; // Yamaguchi
    float playerBHeight_match0 = 172.7f / 165f; // Marin

    float playerAHeight_match1 = 194f / 180f; // Axelsen
    float playerBHeight_match1 = 175f / 180f; // Momota
    
    float playerAHeight_match2 = 175f / 180f; // Castillo
    float playerBHeight_match2 = 170f / 180f; // Ma


    // court floor
    public GameObject venue; 

    
    public void Awake()
    {
        
    }

    void Update()
    {
        time += Time.deltaTime;
        if (time >= _matchInteraction.framerate)
        {
            time = time - _matchInteraction.framerate;

            if (_matchInteraction.trajectoryOn)
            {
                shuttleCock.transform.GetChild(0).gameObject.SetActive(true);
                playerA.SetActive(true);
                playerB.SetActive(true);
                if(!_matchInteraction.summaryViewOn) {
                    venue.SetActive(true);
                } else venue.SetActive(false);
                MoveBall();
            }
            else
            {
                shuttleCock.transform.GetChild(0).gameObject.SetActive(false);
                playerA.SetActive(false);
                playerB.SetActive(false);
                venue.SetActive(false);
               
            }
        }
    }

    public void RenderTrajectory()
    {
        int totalStep = 0;
        TextAsset binAsset = Resources.Load<TextAsset>(_matchData.trajectoryPath + _matchData.selected_rally + "_3d"); // trajectoryList[trajectory_id-1]);
        string[] posArray = binAsset.text.Split("\n");
        totalStep = posArray.Length - 2;
        Vector3[] pos = new Vector3[totalStep];
        Debug.Log("render trajectory " + totalStep);

        for (int i = 1; i < totalStep + 1; i++) //skip first row (column name)
        {
            float x = float.Parse(posArray[i].Split(",")[1]);
            float z = float.Parse(posArray[i].Split(",")[2]);
            float y = float.Parse(posArray[i].Split(",")[3]);
            //pos[i-1] = new Vector3(x, y, z);

           
            if (!(x == -1 & y == -1 & z == -1) & y >= 0)
            {
                if (_matchData.selected_match.MatchID == 3 | (_matchData.selected_match.MatchID == 1 & (_matchData.selected_rally  == "2_01_02" | _matchData.selected_rally  == "2_04_02")))
                {
                     Debug.Log("Flip " + _matchData.selected_rally );
                    //flip for test_match_3
                    pos[i - 1] = new Vector3(6.1f - x, y, z);
                } else pos[i - 1] = new Vector3(x, y, z);
            }
            

            
        }
        
        ballPos = pos;
        _matchInteraction.startStep = 0;
        _matchInteraction.totalStep = totalStep;
    }
    
    public void RenderPosition()
    {
        showPose = false;
        int totalStep1 = 0;
        TextAsset posAsset1 = Resources.Load<TextAsset>(_matchData.positionPath +_matchData.selected_rally +"/player1_position_corrected(3)");
        string[] bodyPosition1 = new string[]{};
        if (posAsset1 != null){
            bodyPosition1 = posAsset1.text.Split("\n");
            Debug.Log("render position  " + bodyPosition1);
            showPose = true;
        }

        TextAsset posAsset2 = Resources.Load<TextAsset>(_matchData.positionPath +_matchData.selected_rally + "/player2_position_corrected(3)");
        string[] bodyPosition2 = new string[] {};
        if (posAsset2 != null){
            bodyPosition2 = posAsset2.text.Split("\n");
        }

        if (showPose){
            totalStep1 = bodyPosition1.Length - 2;
            Vector3[] pos_playerA = new Vector3[totalStep1];
            Vector3[] pos_playerB = new Vector3[totalStep1];
            Debug.Log("render position  " + totalStep1);

            for (int i = 1; i < totalStep1 + 1; i++) //skip first row (column name)
            {
                float z1 = float.Parse(bodyPosition1[i].Split(",")[1]);
                float y1 = y_offset;
                float x1 = float.Parse(bodyPosition1[i].Split(",")[2]);

                float z2 = float.Parse(bodyPosition2[i].Split(",")[1]);
                float y2 = y_offset;
                float x2 = float.Parse(bodyPosition2[i].Split(",")[2]);

                pos_playerA[i - 1] = new Vector3(x1/100, y1, 13.32f-z1/100);
                pos_playerB[i - 1] = new Vector3(x2/100, y2, 13.32f-z2/100);
            }    
            
            playerAPosition = pos_playerA;
            playerBPosition = pos_playerB;
        }
    }

    public void RenderPoses()
    {
        int totalStep2 = 0; 
        TextAsset posAssetA = Resources.Load<TextAsset>(_matchData.posesPath +_matchData.selected_rally +"/player1_poses_corrected(2)");//_corrected(2)
        string[] bodyPosesA = new string[] {};
        if (showPose) bodyPosesA = posAssetA.text.Split("\n");

        TextAsset posAssetB = Resources.Load<TextAsset>(_matchData.posesPath +_matchData.selected_rally + "/player2_poses_corrected(2)");//_corrected(2)
        string[] bodyPosesB = new string[] {};
        if (showPose) bodyPosesB = posAssetB.text.Split("\n");

        if (showPose){
            totalStep2 = bodyPosesA.Length - 2;
            Quaternion[,] poses_playerA = new Quaternion[totalStep2,24];
            Quaternion[,] poses_playerB = new Quaternion[totalStep2,24];
            Debug.Log("render poses " + totalStep2);

            for (int i = 1; i < totalStep2 + 1; i++) //skip first row (column name)
            {
                for (int j=0; j<24; j++)
                {
                    float rodXA = float.Parse(bodyPosesA[i].Split(",")[j*3+2]); //2 player 1 frame 1
                    float rodYA = float.Parse(bodyPosesA[i].Split(",")[j*3+3]);
                    float rodZA = float.Parse(bodyPosesA[i].Split(",")[j*3+4]);

                    float rodXB = float.Parse(bodyPosesB[i].Split(",")[j*3+2]); //2 player 1 frame 1
                    float rodYB = float.Parse(bodyPosesB[i].Split(",")[j*3+3]);
                    float rodZB = float.Parse(bodyPosesB[i].Split(",")[j*3+4]);

                    if (j==0)
                    {
                        Quaternion quatA = QuatFromRodrigues(rodXA, rodYA, rodZA);
                        Quaternion quatB = QuatFromRodrigues(rodXB, rodYB, rodZB);
                        
                        poses_playerA[i - 1,j] = quatA*Quaternion.Euler(0.0f, 0.0f, 180.0f);
                        poses_playerB[i - 1,j] = quatB*Quaternion.Euler(0.0f, 0.0f, 180.0f);
                    }
                    else
                    {
                        Quaternion quatA = QuatFromRodrigues(-rodXA, rodYA, rodZA);
                        Quaternion quatB = QuatFromRodrigues(-rodXB, rodYB, rodZB);

                        poses_playerA[i - 1,j] = quatA;
                        poses_playerB[i - 1,j] = quatB;
                    }  
                }
            }

            playerAPoses = poses_playerA;
            playerBPoses = poses_playerB;
        }
    }

    public void MoveBall()
    {
        if (_matchInteraction.currentStep < _matchInteraction.totalStep)
        {
            shuttleCock.transform.position = ballPos[_matchInteraction.currentStep];
            if (_matchInteraction.currentStep < _matchInteraction.totalStep - 1)
            {
                if (showPose){
                    int poseStep = _matchData.matchName.ToString() == "study_match2" ? _matchInteraction.currentStep*2 : _matchInteraction.currentStep;
                     //Modify player positions
                    // playerA.transform.position = Vector3.Lerp(playerAPosition[_matchInteraction.currentStep],playerAPosition[_matchInteraction.currentStep+1], _matchInteraction.framerate);
                    // playerB.transform.position = Vector3.Lerp(playerBPosition[_matchInteraction.currentStep],playerBPosition[_matchInteraction.currentStep+1], _matchInteraction.framerate);
                    playerA.transform.position = Vector3.Lerp(playerAPosition[poseStep],playerAPosition[poseStep+1], _matchInteraction.framerate);
                    playerB.transform.position = Vector3.Lerp(playerBPosition[poseStep],playerBPosition[poseStep+1], _matchInteraction.framerate);
                    GetToTheGround();

                    //Modify player poses
                    for (int j=0; j<24; j++)
                    {
                        string name = _bodyToModify[j];
                        // Quaternion quatA = Quaternion.Lerp(playerAPoses[_matchInteraction.currentStep,j],playerAPoses[_matchInteraction.currentStep+1,j], _matchInteraction.framerate);
                        // Quaternion quatB = Quaternion.Lerp(playerBPoses[_matchInteraction.currentStep,j],playerBPoses[_matchInteraction.currentStep+1,j], _matchInteraction.framerate);
                        Quaternion quatA = Quaternion.Lerp(playerAPoses[poseStep,j],playerAPoses[poseStep+1,j], _matchInteraction.framerate);
                        Quaternion quatB = Quaternion.Lerp(playerBPoses[poseStep,j],playerBPoses[poseStep+1,j], _matchInteraction.framerate);
                        SetLocalJointRotationA(name, quatA);
                        SetLocalJointRotationB(name, quatB);
                    } 
                }
               

                // Determine which direction to rotate towards
                Vector3 targetDirection = ballPos[_matchInteraction.currentStep + 1] - ballPos[_matchInteraction.currentStep];
                // The step size is equal to speed times frame time.
                float singleStep = _matchInteraction.speed * Time.deltaTime;
                // Rotate the forward vector towards the target direction by one step
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
                shuttleCock.transform.rotation = Quaternion.LookRotation(newDirection);
            }

            _matchInteraction.currentStep += 1;
        }
        else
        {
            // add a buffer time
            if (_matchInteraction.currentStep < _matchInteraction.totalStep + _matchData.video_end_buffer){
                _matchInteraction.currentStep += 1;
            } else {
                if (_matchInteraction.loop)
                {
                    _matchInteraction.currentStep = _matchInteraction.startStep;
                    //_matchInteraction.currentStep = 0;
                } else _matchInteraction.trajectoryOn = false;
            }
        }
    }

    public static Quaternion QuatFromRodrigues(float rodX, float rodY, float rodZ)
    {
        Vector3 axis = new Vector3(rodX, rodY, rodZ);
        float angle_deg = - axis.magnitude * Mathf.Rad2Deg;
        Vector3.Normalize(axis);

        Quaternion quat = Quaternion.AngleAxis(angle_deg, axis);
  
        return quat;
    }

    public void GetToTheGround()
    {
        // To make the player touch the ground
        Transform PlayerAleftFoot = _transformFromNamePlayerA["left_foot"];
        Transform PlayerArightFoot = _transformFromNamePlayerA["right_foot"];
        Transform PlayerBleftFoot = _transformFromNamePlayerB["left_foot"];
        Transform PlayerBrightFoot = _transformFromNamePlayerB["right_foot"];

        //Calculate the vertical distance between the player and each foot 
        float PlayerAdistanceL = Mathf.Abs(playerA.transform.position.y - PlayerAleftFoot.position.y);
        float PlayerAdistanceR = Mathf.Abs(playerA.transform.position.y - PlayerArightFoot.position.y);
        float PlayerBdistanceL = Mathf.Abs(playerB.transform.position.y - PlayerBleftFoot.position.y);
        float PlayerBdistanceR = Mathf.Abs(playerB.transform.position.y - PlayerBrightFoot.position.y);

        float y1 = Mathf.Max (PlayerAdistanceL,PlayerAdistanceR);  //1.3589f touch the ground standing male
        float y2 = Mathf.Max (PlayerBdistanceL,PlayerBdistanceR);  //1.3589f touch the ground standing

        playerA.transform.position = new Vector3(playerA.transform.position.x, y1, playerA.transform.position.z);
        playerB.transform.position = new Vector3(playerB.transform.position.x, y2, playerB.transform.position.z);

    }

    public void SetLocalJointRotationA(string name, Quaternion quatLocal)
    {
        Transform joint = _transformFromNamePlayerA[name];
        joint.localRotation = quatLocal;
    }

    public void SetLocalJointRotationB(string name, Quaternion quatLocal)
    {
        Transform joint = _transformFromNamePlayerB[name];
        joint.localRotation = quatLocal;
    }

    public void UpdatePlayerModel(){
        Debug.Log("Player model " + _matchData.selected_match.MatchID);

        // assign player
        if (_matchData.selected_match.MatchID == 0){
             // assign player color
            if (_matchData.game_select == SummaryPanel.Game.G1 | _matchData.game_select == SummaryPanel.Game.G1i | _matchData.game_select == SummaryPanel.Game.G1ii | _matchData.game_select == SummaryPanel.Game.G3i){
                playerA_match0.transform.Find("SMPLX-mesh-female").GetComponent<Renderer>().material = material_playerA_match0;
                playerB_match0.transform.Find("SMPLX-mesh-female").GetComponent<Renderer>().material = material_playerB_match0;
                // assign height
                playerA_match0.transform.localScale = new Vector3(1, playerAHeight_match0, playerAHeight_match0);
                playerB_match0.transform.localScale = new Vector3(0.95f, playerBHeight_match0, playerBHeight_match0);
            } else {
                playerA_match0.transform.Find("SMPLX-mesh-female").GetComponent<Renderer>().material = material_playerB_match0;
                playerB_match0.transform.Find("SMPLX-mesh-female").GetComponent<Renderer>().material = material_playerA_match0;
                playerA_match0.transform.localScale = new Vector3(0.95f, playerBHeight_match0, playerBHeight_match0);
                playerB_match0.transform.localScale = new Vector3(1, playerAHeight_match0, playerAHeight_match0);
            }   
            
            playerA = playerA_match0;
            playerB = playerB_match0;
        } else if (_matchData.selected_match.MatchID == 1){
             // assign player color
            if (_matchData.game_select == SummaryPanel.Game.G1 | _matchData.game_select == SummaryPanel.Game.G1i | _matchData.game_select == SummaryPanel.Game.G1ii | _matchData.game_select == SummaryPanel.Game.G3i){
                playerA_match1.transform.Find("SMPLX-mesh-male").GetComponent<Renderer>().material = material_playerA_match1;
                playerB_match1.transform.Find("SMPLX-mesh-male").GetComponent<Renderer>().material = material_playerB_match1;
                // assign height
                playerA_match1.transform.localScale = new Vector3(playerAHeight_match1, playerAHeight_match1, playerAHeight_match1);
                playerB_match1.transform.localScale = new Vector3(playerBHeight_match1, playerBHeight_match1, playerBHeight_match1);
            } else {
                playerA_match1.transform.Find("SMPLX-mesh-male").GetComponent<Renderer>().material = material_playerB_match1;
                playerB_match1.transform.Find("SMPLX-mesh-male").GetComponent<Renderer>().material = material_playerA_match1;
                // assign height
                playerA_match1.transform.localScale = new Vector3(playerBHeight_match1, playerBHeight_match1, playerBHeight_match1);
                playerB_match1.transform.localScale = new Vector3(playerAHeight_match1, playerAHeight_match1, playerAHeight_match1);
            }   
           playerA = playerA_match1;
           playerB = playerB_match1;
        } else if(_matchData.selected_match.MatchID == 2){
             // assign player color
            if (_matchData.game_select == SummaryPanel.Game.G1 | _matchData.game_select == SummaryPanel.Game.G1i | _matchData.game_select == SummaryPanel.Game.G1ii | _matchData.game_select == SummaryPanel.Game.G3i){
                playerA_match1.transform.Find("SMPLX-mesh-male").GetComponent<Renderer>().material = material_playerA_match2;
                playerB_match1.transform.Find("SMPLX-mesh-male").GetComponent<Renderer>().material = material_playerB_match2;
                // assign height
                playerA_match1.transform.localScale = new Vector3(playerAHeight_match2, playerAHeight_match2, playerAHeight_match2);
                playerB_match1.transform.localScale = new Vector3(playerBHeight_match2, playerBHeight_match2, playerBHeight_match2);
            } else {
                playerA_match1.transform.Find("SMPLX-mesh-male").GetComponent<Renderer>().material = material_playerB_match2;
                playerB_match1.transform.Find("SMPLX-mesh-male").GetComponent<Renderer>().material = material_playerA_match2;
                // assign height
                playerA_match1.transform.localScale = new Vector3(playerBHeight_match2, playerBHeight_match2, playerBHeight_match2);
                playerB_match1.transform.localScale = new Vector3(playerAHeight_match2, playerAHeight_match2, playerAHeight_match2);
            }   
           playerA = playerA_match1;
           playerB = playerB_match1;
        }

        if (!isLoaded){
            //Take every child component to get each name
            if (_transformFromNamePlayerA == null)
            {
                _transformFromNamePlayerA = new Dictionary<string, Transform>();
                Transform[] transformsPlayerA = playerA.transform.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in transformsPlayerA)
                {
                    _transformFromNamePlayerA.Add(t.name, t);
                }
            }

            if (_transformFromNamePlayerB == null)
            {
                _transformFromNamePlayerB = new Dictionary<string, Transform>();
                Transform[] transformsPlayerB = playerB.transform.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in transformsPlayerB)
                {
                    _transformFromNamePlayerB.Add(t.name, t);
                }
            }
            isLoaded = true;
        }
    }
}
