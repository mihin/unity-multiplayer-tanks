using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

internal class IJoystick : MonoBehaviour
{
    private const int OFFSET_X = 180;
    private const int OFFSET_Y = 180;
    private const int WIDTH = 240;
    private const int HEIGHT = 240;

    private const float TOUCHZONE_WIDTH = 0.5f;
    private const float TOUCHZONE_HEIGHT = 0.5f;

    protected Vector2 position;
    protected Rect touchZone;

    protected const float BACK_GUI_SPEED = 10f;
    protected const float IDLE_RADIUS = 0.0001f;

    protected Rect ExtendZone(Rect rect, float percent)
    {
        float w = rect.width * (1f + percent / 100f);
        float h = rect.height * (1f + percent / 100f);
        return new Rect(rect.x - 0.5f * rect.width * percent / 100f, rect.y - 0.5f * rect.height * percent / 100f, w, h);
    }

    protected int ApplyScale(int p)
    {
        return p;
        //return (int)(p * IAppConfig.Instance.ScreenCoef);
    }

    protected int getOffsetX()
    {
        return ApplyScale(OFFSET_X);
    }

    protected int getOffsetY()
    {
        return ApplyScale(OFFSET_Y);
    }

    protected int getWidth()
    {
        return ApplyScale(WIDTH);
    }

    protected int getHeight()
    {
        return ApplyScale(HEIGHT);
    }

    private int ApplyScaleX(float p)
    {
        return (int)(p * Screen.width);
    }

    private int ApplyScaleY(float p)
    {
        return (int)(p * Screen.height);
    }

    protected int getTouchZoneWidth()
    {
        return ApplyScaleX(TOUCHZONE_WIDTH);
    }

    protected int getTouchZoneHeight()
    {
        return ApplyScaleY(TOUCHZONE_HEIGHT);
    }
}
