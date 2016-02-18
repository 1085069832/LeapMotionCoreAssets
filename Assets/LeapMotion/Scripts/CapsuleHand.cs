using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Leap;

public class CapsuleHand : HandModel {
  private const int THUMB_BASE_INDEX = (int)Finger.FingerType.TYPE_THUMB * 4 + (int)Finger.FingerJoint.JOINT_MCP;
  private const int PINKY_BASE_INDEX = (int)Finger.FingerType.TYPE_PINKY * 4 + (int)Finger.FingerJoint.JOINT_MCP;

  private const float SPHERE_RADIUS = 0.008f;
  private const float CYLINDER_RADIUS = 0.006f;
  private const float PALM_RADIUS = 0.015f;

  private static int _colorIndex = 0;
  private static Color[] _colorList = { Color.blue, Color.green, Color.magenta, Color.cyan, Color.red, Color.yellow };

  public bool showArm = true;
  public Material mat;

  private Dictionary<int, Transform> _jointSpheres;
  private Transform mockThumbJointSphere;
  private Transform palmPositionSphere;

  private Transform wristPositionSphere;

  private Dictionary<Transform, Tuple<Transform, Transform>> _capsuleToSpheres;

  private Transform armFrontLeft, armFrontRight, armBackLeft, armBackRight;

  private Renderer[] _renderers;

  public override void InitHand() {
    base.InitHand();

    _jointSpheres = new Dictionary<int, Transform>();
    _capsuleToSpheres = new Dictionary<Transform, Tuple<Transform, Transform>>();

    createSpheres();
    createCapsules();

    //_renderers = GetComponentsInChildren<Renderer>();

    _colorIndex = (_colorIndex + 1) % _colorList.Length;
  }

  private void createSpheres() {
    //Create spheres for finger joints
    using (FingerList fingers = hand_.Fingers) {
      for (int i = 0; i < fingers.Count; i++) {
        using (Finger finger = fingers[i]) {
          for (int j = 0; j < 4; j++) {
            int key = getFingerJointIndex((int)finger.Type, j);
            _jointSpheres[key] = createSphere("Joint", SPHERE_RADIUS);
          }
        }
      }
    }

    mockThumbJointSphere = createSphere("MockJoint", SPHERE_RADIUS);
    palmPositionSphere = createSphere("PalmPosition", PALM_RADIUS);
    wristPositionSphere = createSphere("WristPosition", SPHERE_RADIUS);

    if (showArm) {
      armFrontLeft = createSphere("ArmFrontLeft", SPHERE_RADIUS);
      armFrontRight = createSphere("ArmFrontRight", SPHERE_RADIUS);
      armBackLeft = createSphere("ArmBackLeft", SPHERE_RADIUS);
      armBackRight = createSphere("ArmBackRight", SPHERE_RADIUS);
    }
  }

  private void createCapsules() {
    //Create capsules between finger joints
    for (int i = 0; i < 5; i++) {
      for (int j = 0; j < 3; j++) {
        int keyA = getFingerJointIndex(i, j);
        int keyB = getFingerJointIndex(i, j + 1);

        Transform sphereA = _jointSpheres[keyA];
        Transform sphereB = _jointSpheres[keyB];

        createCapsule("Finger Joint", sphereA, sphereB);
      }
    }

    //Create capsule between finger knuckles
    for (int i = 0; i < 4; i++) {
      int keyA = getFingerJointIndex(i, 0);
      int keyB = getFingerJointIndex(i + 1, 0);

      Transform sphereA = _jointSpheres[keyA];
      Transform sphereB = _jointSpheres[keyB];

      createCapsule("Hand Joints", sphereA, sphereB);
    }

    //Create the rest of the hand
    Transform thumbBase = _jointSpheres[THUMB_BASE_INDEX];
    Transform pinkyBase = _jointSpheres[PINKY_BASE_INDEX];
    createCapsule("Hand Bottom", thumbBase, mockThumbJointSphere);
    createCapsule("Hand Side", pinkyBase, mockThumbJointSphere);

    if (showArm) {
      createCapsule("ArmFront", armFrontLeft, armFrontRight);
      createCapsule("ArmBack", armBackLeft, armBackRight);
      createCapsule("ArmLeft", armFrontLeft, armBackLeft);
      createCapsule("ArmRight", armFrontRight, armBackRight);
    }
  }

  private int getFingerJointIndex(int fingerIndex, int jointIndex) {
    return fingerIndex * 4 + jointIndex;
  }

  private Transform createSphere(string name, float radius) {
    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    Destroy(sphere.GetComponent<Collider>());
    sphere.transform.parent = transform;
    sphere.transform.localScale = Vector3.one * radius * 2;
    sphere.GetComponent<Renderer>().material = mat;
    sphere.GetComponent<Renderer>().material.color = _colorList[_colorIndex];
    sphere.name = name;
    return sphere.transform;
  }

  private void createCapsule(string name, Transform jointA, Transform jointB) {
    GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    Destroy(capsule.GetComponent<Collider>());
    capsule.name = name;
    capsule.transform.parent = transform;
    capsule.transform.localScale = Vector3.one * CYLINDER_RADIUS * 2;
    capsule.GetComponent<Renderer>().material = mat;
    capsule.GetComponent<Renderer>().material.color = Color.white;
    _capsuleToSpheres[capsule.transform] = new Tuple<Transform, Transform>(jointA, jointB);
  }

  public override void UpdateHand() {
    //Update all spheres
    using (FingerList fingers = hand_.Fingers) {
      for (int i = 0; i < fingers.Count; i++) {
        using (Finger finger = fingers[i]) {
          for (int j = 0; j < 4; j++) {
            int key = getFingerJointIndex((int)finger.Type, j);
            Transform sphere = _jointSpheres[key];
            sphere.position = controller_.ToUnitySpace(finger.JointPosition((Finger.FingerJoint)j));
          }
        }
      }
    }

    palmPositionSphere.position = GetPalmPosition();

    /*
    Vector3 displacement = GetPalmPosition() - MoveUpDown.Instance.transform.TransformPoint(UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.Head));
    float headPercent = 1.0f - Mathf.InverseLerp(0.1f, 0.15f, displacement.xz().magnitude);
    float bodyPercent = 1.0f - Mathf.InverseLerp(0.06f, 0.09f, displacement.xz().magnitude);
    bodyPercent = 0;
    float percent = Mathf.Max(headPercent, bodyPercent);

    for(int i=0; i<_renderers.Length; i++) {
      _renderers[i].material.SetFloat("_Cutoff", percent);
    }
    */

    Vector3 wristPos = GetWristPosition();
    wristPositionSphere.position = wristPos;

    Transform thumbBase = _jointSpheres[THUMB_BASE_INDEX];

    Vector3 thumbBaseToPalm = thumbBase.position - GetPalmPosition();
    mockThumbJointSphere.position = GetPalmPosition() + Vector3.Reflect(thumbBaseToPalm, controller_.ToUnityDir(hand_.Basis.xBasis));

    //Update Arm
    if (showArm) {
      using (var arm = hand_.Arm) {
        Vector3 right = controller_.ToUnityDir(arm.Basis.xBasis).normalized * arm.Width * 0.001f * 0.7f * 0.5f;
        Vector3 wrist = controller_.ToUnitySpace(arm.WristPosition);
        Vector3 elbow = controller_.ToUnitySpace(arm.ElbowPosition);

        float armLength = Vector3.Distance(wrist, elbow);
        wrist -= controller_.ToUnityDir(arm.Direction) * armLength * 0.05f;

        armFrontRight.position = wrist + right;
        armFrontLeft.position = wrist - right;
        armBackRight.position = elbow + right;
        armBackLeft.position = elbow - right;
      }
    }

    //Update all joints
    foreach (var pair in _capsuleToSpheres) {
      Transform sphereA = pair.Value.a;
      Transform sphereB = pair.Value.b;
      Transform capsule = pair.Key;

      Vector3 delta = sphereA.position - sphereB.position;
      Vector3 perp = Vector3.Cross(delta, Vector3.up);

      capsule.rotation = Quaternion.LookRotation(perp, delta);
      Vector3 scale = capsule.localScale;
      scale.y = delta.magnitude / 2.0f;

      capsule.localScale = scale;

      capsule.position = (sphereA.position + sphereB.position) / 2;
    }
  }
}