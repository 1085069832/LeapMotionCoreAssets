using UnityEngine;
using System.Collections;
using Leap;

public class MemTest : MonoBehaviour {

  private Controller _controller;
 
	// Use this for initialization
	void Start () {
    _controller = new Controller();
	}
	
	// Update is called once per frame
	void Update () {
    Frame frame = _controller.Frame();
    if (frame.Hands.Count > 0) {
      palmPos(frame.Hands[0]);
    }
	}

  private void palmPos(Hand hand) {
    Vector palmPositon = hand.PalmPosition;
  }
}
