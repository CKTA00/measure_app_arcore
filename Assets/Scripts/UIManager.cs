using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] PlacePoint placePoint;


    public void OnPointPlaceBtnPress()
    {
        placePoint.PlaceAtCenter();
    }

    public void OnPointRemoveBtnPress()
    {

    }

    public void On90DegressBtnPress()
    {

    }

    public void OnSelectBtnPress()
    {
        placePoint.SelectPoint();
    }
}
