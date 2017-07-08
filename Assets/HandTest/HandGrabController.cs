using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;

public class HandGrabController : MonoBehaviour
{
    HandManager handManager;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (handManager != null)
        {
            foreach (Hand hand in handManager._hands)
            {
                if (hand.IsRight)
                {
                    print("right" + hand.PinchStrength);
                }
                else
                {
                    print("left" + hand.PinchStrength);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "HandCenter")
        {
            handManager = other.GetComponent<HandManager>();
        }
    }
}
