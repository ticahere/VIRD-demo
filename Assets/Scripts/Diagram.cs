using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Diagram : MonoBehaviour
{
    public GameObject shot_prefab;
    private MatchData _matchData; 
    private int diagram_rally_id = 0;
    public GameObject shot_panel;

    // read rally data from MatchData script
    public RallySummary selectedRally;

    // Start is called before the first frame update
    void Start()
    {
        _matchData = gameObject.transform.GetComponent<MatchData>();
    }

    // Update is called once per frame
    void Update()
    {
       /* if (m.selected_rally != diagram_rally_id)
        {
            diagram_rally_id = m.selected_rally;
            UpdateDiagram();
        }*/
    }

    public void UpdateDiagram()
    {
        selectedRally = _matchData.rallySumList[diagram_rally_id - 1];
        shot_panel.transform.parent.transform.Find("Text").GetComponent<Text>().text = selectedRally.name;
        foreach (Transform child in shot_panel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        for (int i = 0; i < selectedRally.shots.Length; i++)
        {
            GameObject newShot = Instantiate(shot_prefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0)) as GameObject;
            int sid = selectedRally.shots[i].index;
            newShot.name = "Shot-" + sid;
            newShot.transform.SetParent(shot_panel.transform, true);
            newShot.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            newShot.GetComponent<RectTransform>().localPosition = new Vector3(newShot.transform.position.x, newShot.transform.position.y, 0);
            newShot.GetComponent<RectTransform>().localRotation = Quaternion.identity;

            newShot.transform.Find("From").GetComponent<Text>().text = selectedRally.shots[i].playerHit;
            newShot.transform.Find("ShotType").GetComponent<Text>().text = selectedRally.shots[i].tendency;

            // draw 2D trajectory
            Vector3 StartPt = selectedRally.shots[i].startPlayerPosition;
            Vector3 EndPt = selectedRally.shots[i].endPlayerPosition;
            // todo: transform from 3D to 2D
            newShot.transform.Find("StartPt").GetComponent<RectTransform>().anchoredPosition = new Vector3(StartPt.x * 10, StartPt.y * 10, 0);
            newShot.transform.Find("EndPt").GetComponent<RectTransform>().anchoredPosition = new Vector3(EndPt.x * 10, EndPt.y * 10, 0);

            GameObject shotLine = new GameObject("Line");
            shotLine.transform.SetParent(newShot.transform, true);
            LineRenderer lRend = shotLine.AddComponent<LineRenderer>();
            lRend.SetWidth(0.04f, 0.06f);
            lRend.SetPosition(0, new Vector3(newShot.transform.Find("StartPt").GetComponent<RectTransform>().position.x, newShot.transform.Find("StartPt").GetComponent<RectTransform>().position.y, 0));
            lRend.SetPosition(1, new Vector3(newShot.transform.Find("EndPt").GetComponent<RectTransform>().position.x, newShot.transform.Find("EndPt").GetComponent<RectTransform>().position.y, 0));

        }
    }
}
