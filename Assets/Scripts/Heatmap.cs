using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Heatmap : MonoBehaviour
{
     MatchData _matchData;
     MatchInteraction _matchInteraction;

    public GameObject heatmap_prefab;

    // court property
    public float x_offset = 0.08f;
    float half_length = 6.71f;
    float half_width = 3.05f;
    public string draw_court = "All";
   /* public string courtA_dir = "From"; // "From", "To"
    public string courtB_dir = "From";  // "From", "To"*/


    // update chart
    public bool isUpdated = false;

    // shot stats
    int[] fromShotCountA = new int[7];
    float[] fromShotPercA = new float[7];
    int[] fromShotCountB = new int[7];
    float[] fromShotPercB = new float[7];

    int[] toShotCountA = new int[7];
    float[] toShotPercA = new float[7];
    int[] toShotCountB = new int[7];
    float[] toShotPercB = new float[7];

    int fromShotCountTotalA = 0;
    int fromShotCountTotalB = 0;
    int toShotCountTotalA = 0;
    int toShotCountTotalB = 0;


    // Start is called before the first frame update
    void Start()
    {
        _matchData = GameObject.Find("MatchDataManager").GetComponent<MatchData>();
        _matchInteraction = GameObject.Find("MatchDataManager").GetComponent<MatchInteraction>();
        DrawCourt();
        DrawShotPercentage(draw_court);

    }

    // Update is called once per frame
    void Update()
    {
        if (!isUpdated)
        {
            UpdateCourt();
        }
    }

    void DrawCourt()
    {
        print("Draw Court");
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (string court in new string[] { "A", "B" })
        {
            foreach (string side in new string[] { "Left", "Right" })
            {
                CreateFront(side, court);
                CreateMid(side, court);
                CreateBack(side, court);
            }
        }
       
    }

    void CreateFront(string side, string court) 
    {
       
        List<Vector3> verticiesList = new List<Vector3> { };
        if (court == "A")
        {
            x_offset = side == "Left" ? 0.08f : (0.08f + half_width);

            verticiesList.Add(new Vector3(x_offset, 0, half_length * 2 / 3));
            verticiesList.Add(new Vector3(x_offset + half_width, 0, half_length * 2 / 3));
            verticiesList.Add(new Vector3(x_offset + half_width, 0, half_length));
            verticiesList.Add(new Vector3(x_offset, 0, half_length));
        } else // court == "B"
        {
            x_offset = side == "Left" ? (0.08f + half_width) : 0.08f;

            verticiesList.Add(new Vector3(x_offset, 0, half_length));
            verticiesList.Add(new Vector3(x_offset + half_width, 0, half_length));
            verticiesList.Add(new Vector3(x_offset + half_width, 0, half_length + half_length / 3));
            verticiesList.Add(new Vector3(x_offset , 0, half_length + half_length / 3));
        }
       

        Vector3[] verticies = verticiesList.ToArray();

        //triangles
        List<int> trianglesList = new List<int> { };
        for (int i = 0; i < (verticies.Length - 2); i++)
        {
            trianglesList.Add(0);
            trianglesList.Add(i + 1);
            trianglesList.Add(i + 2);
        }
        int[] triangles = trianglesList.ToArray();

        CreateCourtComponent("Front" + side + court, verticies, triangles);

    }
    void CreateMid(string side, string court)
    {

        List<Vector3> verticiesList = new List<Vector3> { };
        if (court == "A")
        {
            x_offset = side == "Left" ? 0.08f : (0.08f + half_width);

            verticiesList.Add(new Vector3(x_offset, 0, half_length / 3));
            verticiesList.Add(new Vector3(x_offset + half_width, 0, half_length / 3));
            verticiesList.Add(new Vector3(x_offset + half_width, 0, half_length * 2 / 3));
            verticiesList.Add(new Vector3(x_offset, 0, half_length * 2 / 3));
        }
        else // court == "B"
        {
            x_offset = side == "Left" ? (0.08f + half_width) : 0.08f;
            verticiesList.Add(new Vector3(x_offset, 0, half_length + half_length / 3));
            verticiesList.Add(new Vector3(x_offset + half_width, 0, half_length + half_length / 3));
            verticiesList.Add(new Vector3(x_offset + half_width, 0, half_length + half_length * 2 / 3));
            verticiesList.Add(new Vector3(x_offset, 0, half_length + half_length * 2 / 3));
        }
        Vector3[] verticies = verticiesList.ToArray();
        

        //triangles
        List<int> trianglesList = new List<int> { };
        for (int i = 0; i < (verticies.Length - 2); i++)
        {
            trianglesList.Add(0);
            trianglesList.Add(i + 1);
            trianglesList.Add(i + 2);
        }
        int[] triangles = trianglesList.ToArray();

        CreateCourtComponent("Mid" + side + court, verticies, triangles);

    }

    void CreateBack(string side, string court)
    {

        List<Vector3> verticiesList = new List<Vector3> { };
        if (court == "A")
        {
            x_offset = side == "Left" ? 0.08f : (0.08f + half_width);

            verticiesList.Add(new Vector3(x_offset, 0, 0));
            verticiesList.Add(new Vector3(x_offset + half_width, 0, 0));
            verticiesList.Add(new Vector3(x_offset + half_width, 0, half_length / 3));
            verticiesList.Add(new Vector3(x_offset, 0, half_length / 3));
        }else // court == "B"
        {
            x_offset = side == "Left" ?(0.08f + half_width) : 0.08f;
            verticiesList.Add(new Vector3(x_offset, 0, half_length + half_length * 2 / 3));
            verticiesList.Add(new Vector3(x_offset + half_width, 0, half_length + half_length * 2 / 3));
            verticiesList.Add(new Vector3(x_offset + half_width, 0, half_length + half_length));
            verticiesList.Add(new Vector3(x_offset, 0, half_length + half_length));
        }

        Vector3[] verticies = verticiesList.ToArray();

        //triangles
        List<int> trianglesList = new List<int> { };
        for (int i = 0; i < (verticies.Length - 2); i++)
        {
            trianglesList.Add(0);
            trianglesList.Add(i + 1);
            trianglesList.Add(i + 2);
        }
        int[] triangles = trianglesList.ToArray();

        CreateCourtComponent("Back" + side + court, verticies, triangles);
    }
    void CreateCourtComponent(string name, Vector3[] v, int[] t)
    {
        GameObject m_CourtComp = Instantiate(heatmap_prefab, new Vector3(0, 0.012f, 0), Quaternion.identity) as GameObject;
        m_CourtComp.name = name;
        m_CourtComp.transform.SetParent(gameObject.transform, true);
        m_CourtComp.layer = 3; // set layer to "vis"
        MeshFilter m_MeshFilter = m_CourtComp.GetComponent<MeshFilter>();
        Mesh m_Mesh = new Mesh();
        m_MeshFilter.mesh = m_Mesh;


        UpdateMesh(m_Mesh, v, t);
    }

    void UpdateMesh(Mesh m_mesh, Vector3[] vertices, int[] triangles)
    {
        m_mesh.vertices = vertices;
        m_mesh.triangles = triangles;
    }
    public void UpdateStats()
    {
        fromShotCountA = new int[7];
        fromShotPercA = new float[7];
        fromShotCountB = new int[7];
        fromShotPercB = new float[7];

        toShotCountA = new int[7];
        toShotPercA = new float[7];
        toShotCountB = new int[7];
        toShotPercB = new float[7];

         fromShotCountTotalA = 0;
         fromShotCountTotalB = 0;
         toShotCountTotalA = 0;
         toShotCountTotalB = 0;

        // calculate shot count
        foreach (string shot_name in _matchData.filtered_shots)
        {

            // calculate court A
            if (shot_name.Contains("-from-A-"))
            {
                fromShotCountTotalA += 1;
                for (int i = 0; i < Helper.courtArea.Length; i++)
                {
                    if (shot_name.Contains("-from-A-" + Helper.courtArea[i]))
                    {
                        fromShotCountA[i] += 1;
                    }
                }
            }
            // calculate court B
            if (shot_name.Contains("-from-B-"))
            {
                fromShotCountTotalB += 1;
                for (int i = 0; i < Helper.courtArea.Length; i++)
                {
                    if (shot_name.Contains("-from-B-" + Helper.courtArea[i]))
                    {
                        fromShotCountB[i] += 1;
                    }
                }
            }

            // calculate court A
            if (shot_name.Contains("-to-A-"))
            {
                toShotCountTotalA += 1;
                for (int i = 0; i < Helper.courtArea.Length; i++)
                {
                    if (shot_name.Contains("-to-A-" + Helper.courtArea[i]))
                    {
                        toShotCountA[i] += 1;
                    }
                }
            }
            // calculate court B
            if (shot_name.Contains("-to-B-"))
            {
                toShotCountTotalB += 1;
                for (int i = 0; i < Helper.courtArea.Length; i++)
                {
                    if (shot_name.Contains("-to-B-" + Helper.courtArea[i]))
                    {
                        toShotCountB[i] += 1;
                    }
                }
            }
        }


        // calculate shot perc 
        for (int i = 0; i < fromShotCountA.Length; i++) fromShotPercA[i] = (float)fromShotCountA[i] / fromShotCountTotalA;
        for (int i = 0; i < fromShotCountB.Length; i++) fromShotPercB[i] = (float)fromShotCountB[i] / fromShotCountTotalB;
        for (int i = 0; i < toShotCountA.Length; i++) toShotPercA[i] = (float)toShotCountA[i] / toShotCountTotalA;
        for (int i = 0; i < toShotCountB.Length; i++) toShotPercB[i] = (float)toShotCountB[i] / toShotCountTotalB;

        if (!_matchInteraction.locationFilterOn)
        {
            // if (_matchInteraction.shot_dir == "From")
            // {
            //     _matchData.shotPercA = fromShotPercA;
            //     _matchData.shotPercB = fromShotPercB;
            // }
            // else // shot_dir == "To"
            // {
            //     _matchData.shotPercA = toShotPercB; // reverse the court
            //     _matchData.shotPercB = toShotPercA;
            // }
            _matchData.shotPercA = fromShotPercA;
            _matchData.shotPercB = fromShotPercB;
            _matchData.shotPercA_To = toShotPercB;  // reverse the court
            _matchData.shotPercB_To = toShotPercA;
            
        }
        else // _matchInteraction.locationFilterOn == true
        {
            if (_matchInteraction.shot_by == "PlayerA")
            {
                _matchData.shotPercA = fromShotPercA;
                _matchData.shotPercB = toShotPercB;
            }

            if (_matchInteraction.shot_by == "PlayerB")
            {
                _matchData.shotPercA = toShotPercA;
                _matchData.shotPercB = fromShotPercB;
            }
        }
        isUpdated = false;
    }
    void UpdateCourt()
    {
        Debug.Log("heat map update court " + _matchInteraction.locationFilterOn);
        
        // update shot heatmap 
        _matchData.courtA_dir = _matchInteraction.shot_by == "All" ? _matchInteraction.shot_dir : _matchInteraction.shot_by == "PlayerA" ? "From" : "To";
        _matchData.courtB_dir = _matchInteraction.shot_by == "All" ? _matchInteraction.shot_dir : _matchInteraction.shot_by == "PlayerA" ? "To" : "From";

        draw_court = "All";

        DrawShotPercentage(draw_court);

    }

    public void DrawShotPercentage(string court)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        if (court == "A" | court == "All")
        {
            for (int i =0; i< Helper.courtArea.Length; i++)
            {
                Color c = Helper.GetShotPercColor(_matchData.shotPercA[i], _matchData.courtA_dir);
                if (Helper.courtArea[i] !="NotInCourt"){
                     gameObject.transform.Find(Helper.courtArea[i] + "A").gameObject.SetActive(c.a!=0);
                    gameObject.transform.Find(Helper.courtArea[i] + "A").GetComponent<MeshRenderer>().material.color = c;
                    SetStats(Helper.courtArea[i] + "A", _matchData.shotPercA[i]);
                }
               

            }
        } 
        if (court == "B" | court == "All")
        {
            for (int i = 0; i < Helper.courtArea.Length; i++)
            {
                Color c = Helper.GetShotPercColor(_matchData.shotPercB[i], _matchData.courtB_dir);
                if (Helper.courtArea[i] !="NotInCourt"){
                    gameObject.transform.Find(Helper.courtArea[i] + "B").gameObject.SetActive(c.a!=0);
                    gameObject.transform.Find(Helper.courtArea[i] + "B").GetComponent<MeshRenderer>().material.color = c;
                    SetStats(Helper.courtArea[i] + "B", _matchData.shotPercB[i]);
                }
            }
        }
        isUpdated = true;
    }
    public void SetStats(string zone, float stats)
    {
        GameObject shotText = gameObject.transform.Find(zone + "/value").gameObject;
        //Debug.Log(zone + " " + stats);
        shotText.GetComponent<TextMeshPro>().text = stats == stats ? stats == 0 ? "" : (Mathf.Round(stats * 100)).ToString() + "%" : "";
        Vector3 meshCenter = gameObject.transform.Find(zone).gameObject.transform.GetComponent<Renderer>().bounds.center;
        shotText.gameObject.transform.position = new Vector3(meshCenter.x, 0.14f, meshCenter.z);

        // label faces user
        shotText.transform.rotation = Quaternion.LookRotation(shotText.transform.position - Camera.main.transform.position);
        

    }
}
