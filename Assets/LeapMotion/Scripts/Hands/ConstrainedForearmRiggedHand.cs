/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary. Licensed under Apache 2.0                           *
* Available at http://www.apache.org/licenses/LICENSE-2.0.html                 *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap;

// Class to setup a rigged hand based on a model.
public class ConstrainedForearmRiggedHand : RigidHand {
  private GameObject m_ArmReferenceGameobject;
  public Vector3 modelFingerPointing = Vector3.forward;
  public Vector3 modelPalmFacing = -Vector3.up;

  // Define which basis to use to represent each of these directions.
  [SerializeField]
  BasisDirection FOREARM_UP_BASIS = BasisDirection.NEG_Y;
  [SerializeField]
  BasisDirection PALM_CENTER_UP_BASIS = BasisDirection.NEG_Y; // This is the direction up through the top of the hand.
  [SerializeField]
  BasisDirection FOREARM_FORWARD_TO_PALM_BASIS = BasisDirection.NEG_X;

  void Start() {
    m_ArmReferenceGameobject = GameObject.Find("ArmReferenceRotation");
  }

  public override void InitHand() {
    UpdateHand();
  }

  public Quaternion Reorientation() {
    return Quaternion.Inverse(Quaternion.LookRotation(modelFingerPointing, -modelPalmFacing));
  }

  public override void UpdateHand() {
    if (m_ArmReferenceGameobject == null) {
      m_ArmReferenceGameobject = GameObject.Find("ArmReferenceRotation");
    }

    if (palm != null) {
      palm.position = GetPalmPosition();
      palm.rotation = GetPalmRotation() * Reorientation();
    }

    if (forearm != null) {
      lockForearmToReferenceDirection(m_ArmReferenceGameobject.transform.forward);
    }

    for (int i = 0; i < fingers.Length; ++i) {
      if (fingers[i] != null) {
        fingers[i].fingerType = (Finger.FingerType)i;
        fingers[i].UpdateFinger();
      }
    }
  }

  // Enum abstraction of basis vectors.
  // Used to more easily make code easily
  // modified to fit different rigs.
  private enum BasisDirection {
    POS_X,
    NEG_X,
    POS_Y,
    NEG_Y,
    POS_Z,
    NEG_Z
  }

  private static Vector3 rawBasis(BasisDirection basisVector) {
    switch (basisVector) {
      case BasisDirection.POS_X:
        return Vector3.right;
      case BasisDirection.NEG_X:
        return -1 * Vector3.right;
      case BasisDirection.POS_Y:
        return Vector3.up;
      case BasisDirection.NEG_Y:
        return -1 * Vector3.up;
      case BasisDirection.POS_Z:
        return Vector3.forward;
      case BasisDirection.NEG_Z:
        return -1 * Vector3.forward;
    }

    // failure case.
    return Vector3.zero;
  }

  private static Vector3 transformWorldDirectionFromBasis(Transform transform, BasisDirection basisVector) {
    switch (basisVector) {
      case BasisDirection.POS_X:
        return transform.right;
      case BasisDirection.NEG_X:
        return -1 * transform.right;
      case BasisDirection.POS_Y:
        return transform.up;
      case BasisDirection.NEG_Y:
        return -1 * transform.up;
      case BasisDirection.POS_Z:
        return transform.forward;
      case BasisDirection.NEG_Z:
        return -1 * transform.forward;
    }

    // failure case.
    return Vector3.zero;
  }

  private bool isNegative(BasisDirection basisVector) {
    if(basisVector == BasisDirection.POS_X ||
       basisVector == BasisDirection.POS_Y ||
       basisVector == BasisDirection.POS_Z)
    {
         return false;
    }
    return true;
  }

  private void lockForearmToReferenceDirection(Vector3 referenceDirection_world) {
    Quaternion rawForewarmRotation = GetArmRotation() * Reorientation();
    Vector3 armDir = rawForewarmRotation * rawBasis(FOREARM_FORWARD_TO_PALM_BASIS);

    Vector3 limitedForearmDirection = armDir;
    lockInputVectorToWithinAngleOfReferenceVector(ref limitedForearmDirection, 45.0f, referenceDirection_world);

    Quaternion correctedRotation = Quaternion.FromToRotation(rawBasis(FOREARM_FORWARD_TO_PALM_BASIS), limitedForearmDirection);
    forearm.rotation = correctedRotation;

    float degrees = calcDegreesToRotateToAlignForearmWithPalm();
    forearm.Rotate(rawBasis(FOREARM_FORWARD_TO_PALM_BASIS), degrees, Space.Self);
  }

  // Angle is in degrees
  private static void lockInputVectorToWithinAngleOfReferenceVector(ref Vector3 input, float angle, Vector3 reference) {
    float dot = Vector3.Dot(input, reference);
    float offsetAngle = Mathf.Acos(dot);
    float goalAngle = angle * Mathf.Deg2Rad;
    if (offsetAngle > goalAngle) {
      float diff = offsetAngle - goalAngle;
      float ratio = diff / offsetAngle;
      input = Vector3.Lerp(input, reference, ratio);
    }
  }

  // Returns the angle to rotate about the long axis of the forearm.
  // This angle is in degrees.
  private float calcDegreesToRotateToAlignForearmWithPalm() {
    // palmUpWorld might be different depending on your rig.
    Vector3 palmUpWorld = transformWorldDirectionFromBasis(palm, PALM_CENTER_UP_BASIS);

    // Get a direction vector that represents palm rotation
    // about the axis along the forarm.
    // All of these axis references might be different depending on your rig.
    Vector3 palmUpForearmSpace = forearm.InverseTransformDirection(palmUpWorld).normalized;

    // Make sure we're not hitting a singularity in the upcoming calculations.
    if (localVectorIsHighlyAlignedToForearm(palmUpForearmSpace)) {
      return 0;
    }

    // Remove the vector component along the forearm, to give us a 2D vector
    // rotating around the long axis of the forearm.
    Vector3 palmUpForearmSpacePlanar = palmUpForearmSpace;
    zeroOutVectorComponentAlongBasis(ref palmUpForearmSpacePlanar, FOREARM_FORWARD_TO_PALM_BASIS);
    palmUpForearmSpacePlanar.Normalize();

    // Just some debug rays.
    // magenta is where we want the forearm up to face. cyan is where it faces now.
    Debug.DrawRay(forearm.position, transformWorldDirectionFromBasis(forearm, FOREARM_UP_BASIS) * 10.0f, Color.cyan, 0);
    Debug.DrawRay(forearm.position, forearm.TransformDirection(palmUpForearmSpacePlanar.normalized) * 10.0f, Color.magenta, 0);

    // Long function name is long, but at least descriptive? - @Daniel
    float degrees = calculateAngleRotationToAlignVectorsThatAreZYPlanar(palmUpForearmSpacePlanar, forearm.InverseTransformDirection(transformWorldDirectionFromBasis(forearm, FOREARM_UP_BASIS)));

    if (isNegative(FOREARM_UP_BASIS)) {
      degrees *= -1;
    }

    return degrees;
  }

  // Returns angle in degrees
  private float calculateAngleRotationToAlignVectorsThatAreZYPlanar(Vector3 baseVector, Vector3 vectorToAlign) {
    float baseAngle = Mathf.Atan2(baseVector.y, baseVector.z);
    float toAlignAngle = Mathf.Atan2(vectorToAlign.y, vectorToAlign.z);
    float diffAngle = toAlignAngle - baseAngle;
    float diffAngleDegrees = diffAngle * Mathf.Rad2Deg;

    return diffAngleDegrees;
  }

  // This is the super rare case that the normal direciton
  // of the top of the hand is exactly along the same vector
  // as the forearm, which presents a singularity for the
  // solver here. In this event we'll just return no correction.
  private bool localVectorIsHighlyAlignedToForearm(Vector3 forearmSpaceNormalizedVector) {
    float palmVectorComponentAlongForearm = 0;

    if (FOREARM_FORWARD_TO_PALM_BASIS == BasisDirection.POS_X || FOREARM_FORWARD_TO_PALM_BASIS == BasisDirection.NEG_X) {
      palmVectorComponentAlongForearm = Mathf.Abs(forearmSpaceNormalizedVector.x);
    }
    else if (FOREARM_FORWARD_TO_PALM_BASIS == BasisDirection.POS_Y || FOREARM_FORWARD_TO_PALM_BASIS == BasisDirection.NEG_Y) {
      palmVectorComponentAlongForearm = Mathf.Abs(forearmSpaceNormalizedVector.y);
    }
    else if (FOREARM_FORWARD_TO_PALM_BASIS == BasisDirection.POS_Z || FOREARM_FORWARD_TO_PALM_BASIS == BasisDirection.NEG_Z) {
      palmVectorComponentAlongForearm = Mathf.Abs(forearmSpaceNormalizedVector.z);
    }

    if (palmVectorComponentAlongForearm > 0.99f) {
      return true;
    }

    return false;
  }

  // Sets the given basis of the given vector to zero.
  private static void zeroOutVectorComponentAlongBasis(ref Vector3 vec, BasisDirection basisVector) {
    if (basisVector == BasisDirection.POS_X || basisVector == BasisDirection.NEG_X) {
      vec.x = 0;
    }
    else if (basisVector == BasisDirection.POS_Y || basisVector == BasisDirection.NEG_Y) {
      vec.y = 0;
    }
    else if (basisVector == BasisDirection.POS_Z || basisVector == BasisDirection.NEG_Z) {
      vec.z = 0;
    }
  }
}