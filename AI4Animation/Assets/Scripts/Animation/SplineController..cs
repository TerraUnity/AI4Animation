using UnityEngine;
using BezierSolution;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SplineController
{
  public bool Inspect = false;
  public enum TravelMode { Once, Loop, PingPong };
  private Transform cachedTransform;
  public BezierSpline spline;
  public TravelMode travelMode;
  private float progress = 0f;
  float targetSpeed = 2f;
  public bool waiting = false;
  public float NormalizedT
  {
    get { return progress; }
    set { progress = value; }
  }

  public Style[] Styles = new Style[0];

  public float[] GetStyle()
  {
    float[] style = new float[Styles.Length];
    for (int i = 0; i < Styles.Length; i++)
    {
      style[i] = Styles[i].Query() ? 1f : 0f;
    }
    return style;
  }

  public string[] GetNames()
  {
    string[] names = new string[Styles.Length];
    for (int i = 0; i < names.Length; i++)
    {
      names[i] = Styles[i].Name;
    }
    return names;
  }


  public BezierPoint getStarttPosition()
  {
    return spline.GetBezierPoint(ref progress);
  }


  public BezierPoint getEndPoint()
  {
    return spline.GetBezierEndPoint(ref progress);
  }




  public Vector3 getTransition(Transform t)
  {
    BezierPoint gp = spline.GetBezierPoint(ref progress);
    Style.type = gp.statusMode;

    if (waiting)
      return t.position;

    Vector3 targetPos = spline.MoveAlongSpline(ref progress, targetSpeed * Time.deltaTime);
    targetPos = new Vector3(targetPos.x, t.position.y, targetPos.z);

    if (gp.statusMode == BezierPoint.StatusMode.Run)
      targetSpeed = 2.3f;
    else
      targetSpeed = 2;

    //targetPos = new Vector3(targetPos.x,t.position.y + 0.5f,targetPos.z);
    return targetPos - t.position;
  }

  public Vector3 QueryMove()
  {
    Vector3 move = Vector3.zero;
    if(Style.type != BezierPoint.StatusMode.Wait)
    {
      move.z += 1f;
    }
    return move;
  }

  public float QueryTurn()
  {
    return 0f;
  }

  public void SetStyleCount(int count)
  {
    count = Mathf.Max(count, 0);
    if (Styles.Length != count)
    {
      int size = Styles.Length;
      System.Array.Resize(ref Styles, count);
      for (int i = size; i < count; i++)
      {
        Styles[i] = new Style();
      }
    }
  }

  public bool QueryAny()
  {
    for (int i = 0; i < Styles.Length; i++)
    {
      if (Styles[i].Query())
      {
        return true;
      }
    }
    return false;
  }

  [System.Serializable]
  public class Style
  {
    public string Name;
    public float Bias = 1f;
    public float Transition = 0.1f;
    public KeyCode[] Keys = new KeyCode[0];
    public bool[] Negations = new bool[0];
    public Multiplier[] Multipliers = new Multiplier[0];
    public static BezierPoint.StatusMode type = BezierPoint.StatusMode.Walk;



    public bool Query()
    {
      bool active = false;

      if(Name == "Walk" && type == BezierPoint.StatusMode.Walk)
      {
        active = true;
      }
      else if (Name == "Jog" && type == BezierPoint.StatusMode.Run)
      {
        active = true;
      }
      else if (Name == "Stand" && type == BezierPoint.StatusMode.Wait)
      {
        active = true;
      }
      return active;
    }

    public void SetKeyCount(int count)
    {
      count = Mathf.Max(count, 0);
      if (Keys.Length != count)
      {
        System.Array.Resize(ref Keys, count);
        System.Array.Resize(ref Negations, count);
      }
    }

    public void AddMultiplier()
    {
      ArrayExtensions.Add(ref Multipliers, new Multiplier());
    }

    public void RemoveMultiplier()
    {
      ArrayExtensions.Shrink(ref Multipliers);
    }

    [System.Serializable]
    public class Multiplier
    {
      public KeyCode Key;
      public float Value;
    }
  }

#if UNITY_EDITOR
  public void Inspector()
  {
    Utility.SetGUIColor(Color.grey);
    using (new GUILayout.VerticalScope("Box"))
    {
      Utility.ResetGUIColor();
      if (Utility.GUIButton("SplineController", UltiDraw.DarkGrey, UltiDraw.White))
      {
        Inspect = !Inspect;
      }

      if (Inspect)
      {
        using (new EditorGUILayout.VerticalScope("Box"))
        {
          spline = EditorGUILayout.ObjectField(spline, typeof(BezierSpline), true) as BezierSpline;
        }
      }
    }
  }
#endif

}
