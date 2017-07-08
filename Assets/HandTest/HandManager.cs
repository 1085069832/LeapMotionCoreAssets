using Leap;
using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    LeapProvider leapProvider;
    Frame frame;
    Hand rightHand;
    Hand leftHand;
    List<Hand> hands = new List<Hand>();

    private void Awake()
    {
        leapProvider = FindObjectOfType<LeapProvider>() as LeapProvider;
    }

    public List<Hand> _hands
    {
        get
        {
            return hands;
        }
    }

    /// <summary>
    /// 获取触碰的手
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        //伸出去的第一只手是0
        frame = leapProvider.CurrentFrame;
        if (transform.name == "Sphere_R")
        {
            print("R");
            if (frame.Hands[0].IsRight)
            {
                rightHand = frame.Hands[0];
            }
            else
            {
                rightHand = frame.Hands[1];
            }

            if (!hands.Contains(rightHand))
            {
                hands.Add(rightHand);
            }
        }
        else if (transform.name == "Sphere_L")
        {
            print("L");
            if (frame.Hands[0].IsLeft)
            {
                leftHand = frame.Hands[0];
            }
            else
            {
                leftHand = frame.Hands[1];
            }

            if (!hands.Contains(leftHand))
            {
                hands.Add(leftHand);
            }
        }
    }
}
