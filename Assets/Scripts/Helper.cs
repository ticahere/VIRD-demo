using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper 
{
    public static string[] courtArea = new string[] { "FrontLeft", "MidLeft", "BackLeft", "FrontRight", "MidRight", "BackRight", "NotInCourt" };


    // heatmap color
    // from - PURPLE
    public static Color from_hot = new Color(93 / 255f, 38 / 255f, 137 / 255f);
    public static Color from_cold = new Color(1, 1, 1);
    // to - YELLOW
    public static Color to_hot = new Color(255 / 255f, 103/255f, 0);
    public static Color to_cold = new Color(1, 1, 1);

   /* // from - red
    public static Color from_hot = new Color(179 / 255f, 0, 0);
    public static Color from_cold = new Color(1, 1, 1);
    // to - blue
    public static Color to_hot = new Color(0, 0, 104 / 255f);
    public static Color to_cold = new Color(1, 1, 1);*/

    public static float thres_hot = 0.4f;
    public static float thres_cold = 0.1f;

    // player name colors
    public static Color top_player_color = new Color(0, 200 / 255f, 255 / 255f);
    public static Color bottom_player_color = new Color(255 / 255f, 231 / 255f, 0);


    public static string DetectZoneWithCourt(Vector3 pt, int player, string step)
    {
        // categorize shots based on start & end location
        float half_court_length = 6.71f;
        float half_court_width = 3.05f;
        float court_length = 13.41f;
        float court_width = 6.1f;

        string court;
        if (step == "start")
        {
            court = player == 1 ? "A" : "B";
        } else  court = player == 1 ? "B" : "A";

        if (pt.x < 0 | pt.z < 0 | pt.x > court_width | pt.z > court_length)
        {
            if (pt.z < half_court_length)
            {
                return "A-NotInCourt";
            }
            else return "B-NotInCourt";
        }
        // Play side (A, B)
        if (court == "A") // side A (bottom) 
        {
            // left 
            if (pt.x < half_court_width)
            {
                if (pt.z < half_court_length / 3) { return "A-BackLeft"; }
                else if (pt.z < half_court_length * 2 / 3) { return "A-MidLeft"; }
                else return "A-FrontLeft";
            }
            else // right
            {
                if (pt.z < half_court_length / 3) { return "A-BackRight"; }
                else if (pt.z < half_court_length * 2 / 3) { return "A-MidRight"; }
                else return "A-FrontRight";
            }
        }
        else // side B (top)
        {
            // right 
            if (pt.x < half_court_width)
            {
                if (pt.z < half_court_length * 4 / 3) { return "B-FrontRight"; }
                else if (pt.z < half_court_length * 5 / 3) { return "B-MidRight"; }
                else return "B-BackRight";
            }
            else // left
            {
                if (pt.z < half_court_length * 4 / 3) { return "B-FrontLeft"; }
                else if (pt.z < half_court_length * 5 / 3) { return "B-MidLeft"; }
                else return "B-BackLeft";
            }
        }
    }
    public static string DetectZone(Vector3 pt)
    {
        // categorize shots based on start & end location
        float half_court_length = 6.71f;
        float half_court_width = 3.05f;
        float court_length = 13.41f;
        float court_width = 6.1f;


        if (pt.x < 0 | pt.z < 0 | pt.x > court_width | pt.z > court_length)
        {
            if (pt.z < half_court_length)
            {
                return "A-NotInCourt";
            }
            else return "B-NotInCourt";
        }
        // Play side (A, B)
        if (pt.z < half_court_length) // side A (bottom)  6.71
        {
            // left 
            if (pt.x < half_court_width)
            {
                if (pt.z < half_court_length / 3) { return "A-BackLeft"; }
                else if (pt.z < half_court_length * 2 / 3) { return "A-MidLeft"; }
                else return "A-FrontLeft";
            }
            else // right
            {
                if (pt.z < half_court_length / 3) { return "A-BackRight"; }
                else if (pt.z < half_court_length * 2 / 3) { return "A-MidRight"; }
                else return "A-FrontRight";
            }
        }
        else // side B (top)
        {
            // right 
            if (pt.x < half_court_width)
            {
                if (pt.z < half_court_length * 4 / 3) { return "B-FrontRight"; }
                else if (pt.z < half_court_length * 5 / 3) { return "B-MidRight"; }
                else return "B-BackRight";
            }
            else // left
            {
                if (pt.z < half_court_length * 4 / 3) { return "B-FrontLeft"; }
                else if (pt.z < half_court_length * 5 / 3) { return "B-MidLeft"; }
                else return "B-BackLeft";
            }
        }
    }

    public static Color GetShotPercColor(float p, string dir)
    {

        Color c;
        Color cold;
        Color hot;
        if (dir == "From")
        {
            cold = Helper.from_cold;
            hot = Helper.from_hot;
        }
        else // dir == "To"
        {
            cold = Helper.to_cold;
            hot = Helper.to_hot;
        }
        float r0 = cold.r;
        float g0 = cold.g;
        float b0 = cold.b;

        float r1 = hot.r;
        float g1 = hot.g;
        float b1 = hot.b;

        if (p < Helper.thres_cold)
        {
            c = cold;
        }
        else if (p > Helper.thres_hot)
        {
            c = hot;
        }
        else
        {
            float red = r0 + (r1 - r0) * (p - Helper.thres_cold) / (Helper.thres_hot - Helper.thres_cold);
            float green = g0 + (g1 - g0) * (p - Helper.thres_cold) / (Helper.thres_hot - Helper.thres_cold);
            float blue = b0 + (b1 - b0) * (p - Helper.thres_cold) / (Helper.thres_hot - Helper.thres_cold);
            c = new Color(red, green, blue);
        }

        c.a = (p == 0 | float.IsNaN(p)) ? 0 : 0.95f;
        return c;
    }
}
