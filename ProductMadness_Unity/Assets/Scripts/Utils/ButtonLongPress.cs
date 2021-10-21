using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// A helper class for detecting long press on a button
/// </summary>
public class ButtonLongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float holdTime = .5f;

    public UnityEvent onClick = new UnityEvent();
    public UnityEvent onLongPress = new UnityEvent();

    private float startTime;

    public void OnPointerDown(PointerEventData eventData)
    {
        startTime = Time.time;
        Invoke("OnLongPress", holdTime);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Time.time - startTime < holdTime)
        {
            onClick.Invoke();
        }
        CancelInvoke("OnLongPress");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }

    void OnLongPress()
    {
        onLongPress.Invoke();
    }
}