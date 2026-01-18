using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TouchscreenInput : MonoBehaviour
{
    [Header("Settings")] 
    [Tooltip("Move joystick magnitude is in [-1;1] range, this multiply it before sending it to move event")]
    public float MoveMagnitudeMultiplier = 1.0f;
    [Tooltip("Look joystick magnitude is in [-1;1] range, this multiply it before sending it to move event")]
    public float LookMagnitudeMultiplier = 1.0f;
    public bool InvertLookY;
    
    [Header("Events")]
    public UnityEvent<Vector2> MoveEvent;
    public UnityEvent<Vector2> LookEvent;
    public UnityEvent<bool> JumpEvent;
    public UnityEvent<bool> SprintEvent;
    
    private UIDocument m_Document;

    private VirtualJoystick m_MoveJoystick;
    private VirtualJoystick m_LookJoystick;

    private void Awake()
    {
        m_Document = GetComponent<UIDocument>();

        var safeArea = Screen.safeArea;

        var root = m_Document.rootVisualElement;

        root.style.position = Position.Absolute;
        root.style.left = safeArea.xMin;
        root.style.right = Screen.width - safeArea.xMax;
        root.style.top = Screen.height - safeArea.yMax;
        root.style.bottom = safeArea.yMin;
    }

    private void Start()
    {
        var joystickMove = m_Document.rootVisualElement.Q<VisualElement>("JoystickMove");
        var joystickLook = m_Document.rootVisualElement.Q<VisualElement>("JoystickLook");
        
        m_MoveJoystick = new VirtualJoystick(joystickMove);
        m_MoveJoystick.JoystickEvent.AddListener(mov =>
        {
            MoveEvent.Invoke(mov * MoveMagnitudeMultiplier);
        });;
        
        m_LookJoystick = new VirtualJoystick(joystickLook);
        m_LookJoystick.JoystickEvent.AddListener(mov =>
        {
            if (InvertLookY)
                mov.y *= -1;

            LookEvent.Invoke(mov * LookMagnitudeMultiplier);
        });

        var jumpButton = m_Document.rootVisualElement.Q<VisualElement>("ButtonJump");
        jumpButton.RegisterCallback<PointerEnterEvent>(evt => { JumpEvent.Invoke(true); });
        jumpButton.RegisterCallback<PointerLeaveEvent>(evt => { JumpEvent.Invoke(false); });
        
        var sprintButton = m_Document.rootVisualElement.Q<VisualElement>("ButtonSprint");
        sprintButton.RegisterCallback<PointerEnterEvent>(evt => { SprintEvent.Invoke(true); });
        sprintButton.RegisterCallback<PointerLeaveEvent>(evt => { SprintEvent.Invoke(false); });
    }
}
public class VirtualJoystick
{
    public VisualElement BaseElement;
    public VisualElement Thumbstick;

    public UnityEvent<Vector2> JoystickEvent = new();

    public VirtualJoystick(VisualElement root)
    {
        BaseElement = root;
        Thumbstick = root.Q<VisualElement>("JoystickHandle");
            
        BaseElement.RegisterCallback<PointerDownEvent>(HandlePress);
        BaseElement.RegisterCallback<PointerMoveEvent>(HandleDrag);
        BaseElement.RegisterCallback<PointerUpEvent>(HandleRelease);
    }

    void HandlePress(PointerDownEvent evt)
    {
        BaseElement.CapturePointer(evt.pointerId);
    }

    void HandleRelease(PointerUpEvent evt)
    {
        BaseElement.ReleasePointer(evt.pointerId);
            
        Thumbstick.style.left = Length.Percent(50);
        Thumbstick.style.top = Length.Percent(50);
        
        JoystickEvent.Invoke(Vector2.zero);
    }

    void HandleDrag(PointerMoveEvent evt)
    {
        if (!BaseElement.HasPointerCapture(evt.pointerId)) return;
            
        var width = BaseElement.contentRect.width;
        var center = new Vector3(width / 2, width / 2);
        var centerToPosition = evt.localPosition - center;

        if (centerToPosition.magnitude > width/2)
        {
            centerToPosition = centerToPosition.normalized * width / 2;
        }

        var newPos = center + centerToPosition;

        Thumbstick.style.left = newPos.x;
        Thumbstick.style.top = newPos.y;

        centerToPosition /= (width / 2);
        //we invert y as the y of UI goes down, but pushing the joystick up is expected to give a positive y value
        centerToPosition.y *= -1;

        JoystickEvent.Invoke(centerToPosition);
    }
}