using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] PlacePoint placePoint;

    [SerializeField] TextMeshProUGUI textMeshTogglePlanes;
    [SerializeField] TextMeshProUGUI textMeshToggleLines;
    [SerializeField] TextMeshProUGUI textMeshToggleAngles;
    [SerializeField] TextMeshProUGUI textMeshToggleArea;

    void Start()
    {
        // Toggle each option twice, so that the right text
        // is shown, based on the default values in "PlacePoint"
        for (int i = 0; i < 2; i++)
        {
            OnShowPlanesToggle();
            OnShowLinesToggle();
            OnShowAnglesToggle();
            OnShowAreaToggle();
        }
    }

    public void OnPointPlaceBtnPress()
    {
        placePoint.PlaceAtCenter();
    }

    public void OnPointRemoveBtnPress()
    {
        placePoint.RemoveSelectedPoint();
    }

    public void OnSelectBtnPress()
    {
        placePoint.SelectPoint();
    }

    public void On90DegBtnPress()
    {
        placePoint.EnforceRightAngleToggle();
    }

    public void OnShowPlanesToggle()
    {
        placePoint.showDetectedPlanes = !placePoint.showDetectedPlanes;
        string activeText = placePoint.showDetectedPlanes ? "Enabled" : "Disabled";
        textMeshTogglePlanes.text = "Planes\n" + $"[{activeText}]";
    }

    public void OnShowLinesToggle()
    {
        placePoint.showLineLabels = !placePoint.showLineLabels;
        string activeText = placePoint.showLineLabels ? "Enabled" : "Disabled";
        textMeshToggleLines.text = "Line Labels\n" + $"[{activeText}]";
    }

    public void OnShowAnglesToggle()
    {
        placePoint.showAngleLabels = !placePoint.showAngleLabels;
        string activeText = placePoint.showAngleLabels ? "Enabled" : "Disabled";
        textMeshToggleAngles.text = "Angle Labels\n" + $"[{activeText}]";
    }

    public void OnShowAreaToggle()
    {
        placePoint.showAreaLabel = !placePoint.showAreaLabel;
        string activeText = placePoint.showAreaLabel ? "Enabled" : "Disabled";
        textMeshToggleArea.text = "Area Label\n" + $"[{activeText}]";
    }
}
