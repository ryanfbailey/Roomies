///////////////////////////////////////////////////////////////////////
//                                                   41 Post                                       //
// Created by DimasTheDriver in May/12/2011                                      //
// Part of 'Unity: Scaling the GUI based on the screen resolution' post. //
// Available at:      http://www.41post.com/?p=3816                             //
/////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;

public class GUIScaleExample : MonoBehaviour
{
    //a GUISkin object to draw the GUI image
    public GUISkin guiSkin;

    //the GUI scale ratio
    private float guiRatio;

    //the screen width
    private float sWidth;

    //A vector3 that will be created using the scale ratio
    private Vector3 GUIsF;

    //At initialization
    void Awake()
    {
        //get the screen's width
        sWidth = Screen.width;
        //calculate the scale ratio
        guiRatio = sWidth / 1920;
        //create a scale Vector3 with the above ratio
        GUIsF = new Vector3(guiRatio, guiRatio, 1);
    }

    //Draws GUI elements
    void OnGUI()
    {
        //scale and position the GUI element to draw it at the screen's top left corner
        GUI.matrix = Matrix4x4.TRS(new Vector3(GUIsF.x, GUIsF.y, 0), Quaternion.identity, GUIsF);
        //draw GUI on the top left
        GUI.Label(new Rect(20, 20, 258, 89), "", guiSkin.customStyles[0]);

        //scale and position the GUI element to draw it at the screen's bottom right corner
        GUI.matrix = Matrix4x4.TRS(new Vector3(Screen.width - 258 * GUIsF.x, Screen.height - 89 * GUIsF.y, 0), Quaternion.identity, GUIsF);
        //draw GUI on the bottom right
        GUI.Label(new Rect(-20, -20, 258, 89), "", guiSkin.customStyles[0]);

        //scale and position the GUI element to draw it at the screen's bottom left corner
        GUI.matrix = Matrix4x4.TRS(new Vector3(GUIsF.x, Screen.height - 89 * GUIsF.y, 0), Quaternion.identity, GUIsF);
        //draw GUI on the bottom left
        GUI.Label(new Rect(20, -20, 258, 89), "", guiSkin.customStyles[0]);

        //scale and position the GUI element to draw it at the screen's top right corner
        GUI.matrix = Matrix4x4.TRS(new Vector3(Screen.width - 258 * GUIsF.x, GUIsF.y, 0), Quaternion.identity, GUIsF);
        //draw GUI on the top right
        GUI.Label(new Rect(-20, 20, 258, 89), "", guiSkin.customStyles[0]);
    }
}
