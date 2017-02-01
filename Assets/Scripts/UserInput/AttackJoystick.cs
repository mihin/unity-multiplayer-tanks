using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(GUITexture))]
internal class AttackJoystick : IJoystick
{
    private GUITexture texture = null;
    protected bool mouseDown = false;
    private int lastFingerId = -1;

    protected Vector2 fingerCurrPos;
    protected Vector2 fingerDownPos;
    private Vector2 circlePosition;

    private static AttackJoystick instance = null;
    public static AttackJoystick Instance
    {
        get
        {
            return instance;
        }
    }
    private void Awake()
    {
        instance = this;
    }

    protected virtual void Start()
    {
        texture = GetComponent<GUITexture>();
        texture.pixelInset = new Rect(Screen.width - (getWidth() + getOffsetX()), getOffsetY() + 1.5f * getHeight(), getWidth(), getHeight());
        UpdateTouchZone();
        circlePosition = new Vector2(texture.pixelInset.x, texture.pixelInset.y);
    }

    public void UpdateTouchZone()
    {
        touchZone = ExtendZone(texture.pixelInset, 50);
    }

    protected virtual void Update()
    {
        Vector2 newPosition = UpdateJoystick();

        if (mouseDown)
        {
            fingerCurrPos = newPosition;

            float x = Mathf.Clamp(fingerCurrPos.x, touchZone.min.x, touchZone.max.x);
            float y = Mathf.Clamp(fingerCurrPos.y, touchZone.min.y, touchZone.max.y);

            float xGui = x;
            float yGui = y;

            float dx = x - touchZone.center.x;
            float dy = y - touchZone.center.y;

            Vector2 vec = new Vector2(dx, dy);
            float len = vec.magnitude;

            vec.Normalize();

            vec *= Mathf.Min(len, touchZone.height / 2f);

            xGui = touchZone.center.x + vec.x;
            yGui = touchZone.center.y + vec.y;

            MoveGui(xGui, yGui);

            position = ComputePosition(x, y);
        }
        else
        {
            MoveGuiBack();
        }
    }

    protected Vector2 ComputePosition(float x, float y)
    {
        Vector2 p;
        p.x = (x - touchZone.center.x) / touchZone.width;
        p.y = (y - touchZone.center.y) / touchZone.height;
        return p;
    }

    public virtual void MoveGui(float x, float y)
    {
        float newX = x - texture.pixelInset.width / 2f;
        float newY = y - texture.pixelInset.height / 2f;
        texture.pixelInset = new Rect(newX, newY, texture.pixelInset.width, texture.pixelInset.height);
    }

    protected virtual void MoveGuiBack()
    {
        Vector2 local = new Vector2(texture.pixelInset.x, texture.pixelInset.y);
        Vector2 shift = local - circlePosition;

        if (Vector2.SqrMagnitude(shift) > IDLE_RADIUS)
        {
            Vector2 position = Vector2.Lerp(local, circlePosition, BACK_GUI_SPEED * Time.deltaTime);
            texture.pixelInset = new Rect(position.x, position.y, texture.pixelInset.width, texture.pixelInset.height);
        }
    }

    private bool ExecuteAction(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
        {
            if (OnTouchBegan(touch))
            {
                return true;
            }
        }
        else
        {
            if (lastFingerId == touch.fingerId)
            {
                OnTouchMove(touch);
                return true;
            }
        }
        return false;
    }

    protected Vector2 UpdateJoystick()
    {
#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
        int touchCount = Input.touchCount;
        Vector2 position = Vector2.zero;
        if (touchCount > 0)
        {
            for (int i = 0; i < touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (ExecuteAction(touch))
                {
                    position = touch.position;
                    break;
                }
            }
        }
        else
        {
            ResetJoystick();
        }
        return position;
#else
        if (!mouseDown)
        {
            if (Input.GetMouseButtonDown(0))
            {
                BeginTouch();
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    if (touchZone.Contains(Input.mousePosition))
                    {
                        BeginTouch();
                    }
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                EndTouch();
            }
        }
        return Input.mousePosition;
#endif
    }

    public bool Touched
    {
        get
        {
            return mouseDown;
        }
    }

    public bool BeginTouch()
    {
        if (touchZone.Contains(Input.mousePosition))
        {
            fingerDownPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            mouseDown = true;

            return true;
        }
        return false;
    }

    public void EndTouch()
    {
        ResetJoystick();
    }

    protected bool OnTouchBegan(Touch touch)
    {
        if (touchZone.Contains(touch.position))
        {
            if (lastFingerId == -1 || lastFingerId != touch.fingerId)
            {
                lastFingerId = touch.fingerId;

                float x = Mathf.Clamp(touch.position.x, touchZone.min.x, touchZone.max.x);
                float y = Mathf.Clamp(touch.position.y, touchZone.min.y, touchZone.max.y);
                fingerDownPos = new Vector2(x, y);
                lastFingerId = touch.fingerId;
                mouseDown = true;
                return true;
            }
        }
        return false;
    }

    private void OnTouchMove(Touch touch)
    {
        fingerCurrPos = touch.position;

        float x = Mathf.Clamp(fingerCurrPos.x, touchZone.min.x, touchZone.max.x);
        float y = Mathf.Clamp(fingerCurrPos.y, touchZone.min.y, touchZone.max.y);

        position = ComputePosition(x, y);

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            ResetJoystick();
        }
    }

    private void ResetJoystick()
    {
        fingerDownPos = Vector2.zero;
        fingerCurrPos = Vector2.zero;

        position = Vector2.zero;

        lastFingerId = -1;
        mouseDown = false;
    }

    private void OnDisable()
    {
        ResetJoystick();
    }
}
