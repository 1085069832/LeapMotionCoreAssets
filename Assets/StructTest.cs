using UnityEngine;
using System;
using System.Collections;
using Leap;

public class StructTest : MonoBehaviour {
  private Controller _controller;
  private Leap.Hand.PackedHandPose handPose = new Hand.PackedHandPose();

	// Use this for initialization
	void Start () {
    _controller = new Controller();
	}
	
	// Update is called once per frame
	void Update () {
    debugHandPoseStruct();
	}

  [ContextMenu("Request Hand Pose Struct")]
  private void debugHandPoseStruct() {
    if (_controller == null) {
      return;
    }

    Frame frame = _controller.Frame();

    if (frame.Hands.Count <= 0) { return; }

    Hand hand = frame.Hands[0];

    hand.getHandPoseData(ref handPose);

    if (handPose == null) {
      Debug.Log("null hand pose");
      return;
    }

    //Debug.Log(handPose.FirstFloat);
  }
}
