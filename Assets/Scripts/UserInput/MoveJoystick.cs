using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(GUITexture))]
internal class MoveJoystick : IJoystick
{
    private const int OFFSET_X = 140;

    private const float defaultCameraAngle = 180.0f;
    private const float skipAreaRadius = -0.05f;

    private GUITexture texture = null;
    protected bool mouseDown = false;
    private int lastFingerId = -1;
    private Vector2 circlePosition;

    protected Vector2 fingerCurrPos;
    protected Vector2 fingerDownPos;
    private static Material material = null;

    public event Action<Vector2> OnSetPosition = null;

    public static bool IsActive = true;

    private static MoveJoystick instance = null;
    public static MoveJoystick Instance
    {
        get
        {
            return instance;
        }
    }
    private void Awake()
    {
        instance = this;

        InitTexture();
        gameObject.SetActive(IsActive);
    }

    private void InitTexture()
    {
        texture = GetComponent<GUITexture>();
        texture.pixelInset = new Rect(getOffsetX(), getOffsetY(), getWidth(), getHeight());
        circlePosition = new Vector2(texture.pixelInset.x, texture.pixelInset.y);

        //UpdateTouchZone();

        touchZone = new Rect(circlePosition.x - ApplyScale(OFFSET_X), circlePosition.y, getTouchZoneWidth(), getTouchZoneHeight());
    }

    protected virtual void Start()
    {
        InitTexture();
    }

    public virtual void MoveGui(float x, float y)
    {
        float newX = x - texture.pixelInset.width / 2f;
        float newY = y - texture.pixelInset.height / 2f;
        texture.pixelInset = new Rect(newX, newY, texture.pixelInset.width, texture.pixelInset.height);
    }

    protected virtual bool MoveGuiBack()
    {
        Vector2 local = new Vector2(texture.pixelInset.x, texture.pixelInset.y);
        Vector2 shift = local - circlePosition;

        if (Vector2.SqrMagnitude(shift) > IDLE_RADIUS)
        {
            Vector2 position = Vector2.Lerp(local, circlePosition, BACK_GUI_SPEED * Time.deltaTime);
            texture.pixelInset = new Rect(position.x, position.y, texture.pixelInset.width, texture.pixelInset.height);
            return true;
        }
        return false;
    }

    private bool ExecuteAction(Touch touch)
    {
        if (!mouseDown)
        {
            if (OnTouchBegan(touch))
            {
                return true;
            }
        }
        if (lastFingerId == touch.fingerId)
        {
            OnTouchMove(touch);
            return true;
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

    protected virtual void Update()
    {
        //if (!TutorialManager.Instance.IsJoystickAvailable)
        //    return;

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

    protected bool OnTouchBegan(Touch touch)
    {
        if (touchZone.Contains(touch.position))
        {
            if (lastFingerId == -1 || lastFingerId != touch.fingerId)
            {
                lastFingerId = touch.fingerId;

                float x = Mathf.Clamp(touch.position.x, touchZone.min.x, touchZone.max.x);
                float y = Mathf.Clamp(touch.position.y, touchZone.min.y, touchZone.max.y);
                lastFingerId = touch.fingerId;
                SetPosition(x, y);
                mouseDown = true;
                return true;
            }
        }
        return false;
    }

    private void SetPosition(float x, float y)
    {
        fingerDownPos = new Vector2(x, y);

        //float w = texture.pixelInset.width;
        //float h = texture.pixelInset.height;

        //texture.pixelInset = new Rect(x - w / 2f, y - h / 2f, w, h);

        float w = texture.pixelInset.width;
        float h = texture.pixelInset.height;

        texture.pixelInset = new Rect(x - w / 2f, y - h / 2f, w, h);

        if (OnSetPosition != null)
            OnSetPosition(new Vector2(x, y));

        UpdateTouchZone();
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

    public void UpdateTouchZone()
    {
        touchZone = ExtendZone(texture.pixelInset, 100f);

        //touchZone = new Rect(texture.pixelInset.x - getTouchZoneWidth() / 2f + texture.pixelInset.width / 2f,
        //    texture.pixelInset.y - getTouchZoneHeight() / 2f + texture.pixelInset.height / 2f,
        //    getTouchZoneWidth(), getTouchZoneHeight());
    }

    private void ResetJoystick()
    {
        //touchZone = ExtendZone(new Rect(circlePosition.x, circlePosition.y, getTouchZoneWidth(), getTouchZoneHeight()), 0f);
        touchZone = new Rect(circlePosition.x - ApplyScale(OFFSET_X), circlePosition.y, getTouchZoneWidth(), getTouchZoneHeight());

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

    public bool Touched
    {
        get
        {
            return mouseDown;
        }
    }

    public bool BeginTouch(Vector3 position)
    {
        if (touchZone.Contains(position))
        {
            SetPosition(position.x, position.y);
            mouseDown = true;

            return true;
        }
        return false;
    }

    public bool BeginTouch()
    {
        return BeginTouch(Input.mousePosition);
    }

    public void EndTouch()
    {
        ResetJoystick();
    }

    protected Vector2 ComputePosition(float x, float y)
    {
        Vector2 p;
        p.x = (x - touchZone.center.x) / touchZone.width;
        p.y = (y - touchZone.center.y) / touchZone.height;
        return p;
    }

    public virtual Vector2 Position
    {
        get
        {
            return new Vector3(position.x, position.y);
        }
    }

    public Vector3 GetRotatePosition()
    {
        if (Math.Abs(position.x) < skipAreaRadius && Math.Abs(position.y) < skipAreaRadius)
            return Vector3.zero;

        // Возвращает корректные координаты для установки direction'a MainHero, которые проглотит MainHero.LookAt
        Vector2 vec = new Vector2(-position.x, -position.y);
        vec = TransformByCamera(vec);
        vec.Normalize();
        return new Vector3(vec.x * 1000, 0, vec.y * 1000);
    }

    private Vector2 TransformByCamera(Vector2 src)
    {
        return src;

        //MainCamera camera = MainCamera.Instance;
        Camera camera = Camera.current;
        float yrot = camera.transform.rotation.eulerAngles.y;
        float needRotate = defaultCameraAngle - yrot;

        if (Mathf.Abs(needRotate) < 1.0f)
        {
            return src;
        }

        Vector3 src3 = new Vector3(src.x, src.y, 0.0f);

        src3 = Quaternion.AngleAxis(needRotate, Vector3.forward) * src3;

        src.x = src3.x;
        src.y = src3.y;
        return src;
    }

    public static Material CreateMaterial()
    {
        if (material == null)
        {
            material = Resources.Load<Material>("DebugUI");
        }
        return material;
    }

    //protected virtual void OnRenderObject()
    //{
    //    Rect rn = GetRectNormalized();

    //    Material material = CreateMaterial();
    //    if (material != null)
    //    {
    //        material.SetPass(0);
    //    }

    //    GL.PushMatrix();
    //    GL.LoadOrtho();

    //    GL.Begin(GL.TRIANGLES);
    //    GL.Color(new Vector4(1, 1, 1, 0.5f));
    //    GL.Vertex3(rn.min.x, rn.min.y, 0);
    //    GL.Vertex3(rn.min.x, rn.max.y, 0);
    //    GL.Vertex3(rn.max.x, rn.min.y, 0);

    //    GL.Vertex3(rn.max.x, rn.min.y, 0);
    //    GL.Vertex3(rn.min.x, rn.max.y, 0);
    //    GL.Vertex3(rn.max.x, rn.max.y, 0);

    //    GL.End();

    //    GL.PopMatrix();
    //}

    private Rect GetRectNormalized()
    {
        float x = TouchZone.x / Screen.width;
        float y = TouchZone.y / Screen.height;
        float w = TouchZone.width / Screen.width;
        float h = TouchZone.height / Screen.height;

        return new Rect(x, y, w, h);
    }

    public virtual Rect TouchZone
    {
        get
        {
            return touchZone;
        }
    }

    public Vector3 GetMovePosition()
    {
        Vector3 movePoint = Vector3.zero;
        if (Math.Abs(position.x) < skipAreaRadius && Math.Abs(position.y) < skipAreaRadius)
            return movePoint;

        Vector2 tmpPos = TransformByCamera(position);

        movePoint.x = tmpPos.x;
        movePoint.z = tmpPos.y;

        return movePoint;
    }

    public void Show()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (texture != null && !texture.enabled)
            texture.enabled = true;
    }

    public void Hide()
    {
        if (texture != null && texture.enabled)
            texture.enabled = false;
    }

    public GUITexture Texture
    {
        get { return texture; }
    }
}
