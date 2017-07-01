/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary. Licensed under Apache 2.0                           *
* Available at http://www.apache.org/licenses/LICENSE-2.0.html                 *
\******************************************************************************/

using UnityEngine;
using System.Collections;

/// <summary>
/// 展开花瓣
/// </summary>
public class FlowerBloom : MonoBehaviour {

  public AnimationCurve openCurve;
  public float openAngle = -47;
  public float closeAngle = 100;
  public float openTime = 1.0f;
  public float closeTime = 1.0f;
  public bool open = false;

  public HingeJoint[] pedals;

  private float phase_ = 1.0f;

  void Update() {
    if (open)//开启花瓣
      phase_ += Time.deltaTime / openTime;
    else
      phase_ -= Time.deltaTime / closeTime;

    phase_ = Mathf.Clamp(phase_, 0.0f, 1.0f);

    float percent_done = openCurve.Evaluate(phase_);
    float angle = closeAngle + percent_done * (openAngle - closeAngle);

    foreach (HingeJoint hinge in pedals) {
      JointSpring spring = hinge.spring;//弹簧
      spring.targetPosition = angle;//设置弹簧拉向的角度
      hinge.spring = spring;
    }
  }
}
