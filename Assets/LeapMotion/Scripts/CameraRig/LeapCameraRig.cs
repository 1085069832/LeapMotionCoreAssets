using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(OVRCameraRig))]
public class LeapCameraRig : MonoBehaviour {

  [SerializeField, HideInInspector]
  protected GameObject _targetPrefab = null;

  [SerializeField, HideInInspector]
  protected List<GameObject> _instancedGameObjects = new List<GameObject>();

  [SerializeField, HideInInspector]
  protected List<Component> _instancedComponents = new List<Component>();

  public IEnumerable<GameObject> instancedGameObjects {
    get {
      return _instancedGameObjects;
    }
  }

  public IEnumerable<Component> instancedComponents {
    get {
      return _instancedComponents;
    }
  }

  private GameObject instantiatedPrefab;
  private OVRCameraRig instanceCameraRig;

  private OVRCameraRig myCameraRig {
    get {
      return GetComponent<OVRCameraRig>();
    }
  }

  public void setTargetPrefab(GameObject prefab) {
    _targetPrefab = prefab;
  }

  public void loadPrefabOverwrite() {
    destroyComponentsExcept(myCameraRig.leftEyeAnchor, typeof(Camera));
    destroyComponentsExcept(myCameraRig.rightEyeAnchor, typeof(Camera));
    destroyComponentsExcept(myCameraRig.centerEyeAnchor);

    destroyChildren(myCameraRig.leftEyeAnchor);
    destroyChildren(myCameraRig.rightEyeAnchor);
    destroyChildren(myCameraRig.centerEyeAnchor);

    destroyComponentsExcept(myCameraRig.transform, typeof(LeapCameraRig), typeof(OVRCameraRig), typeof(OVRManager));

    _instancedGameObjects.Clear();
    _instancedComponents.Clear();

    loadPrefabAdditive();
  }

  public void loadPrefabAdditive() {
    foreach (GameObject go in _instancedGameObjects) {
      if (go != null) {
        DestroyImmediate(go);
      }
    }

    foreach (Component co in _instancedComponents) {
      if (co != null) {
        DestroyImmediate(co);
      }
    }

    _instancedGameObjects.Clear();
    _instancedComponents.Clear();

    instantiatedPrefab = Instantiate(_targetPrefab);
    instanceCameraRig = instantiatedPrefab.GetComponentsInChildren<OVRCameraRig>(true)[0];

    copyComponents(instanceCameraRig.transform, myCameraRig.transform, typeof(LeapCameraRig), typeof(OVRCameraRig), typeof(OVRManager));
    _oldToNewComponent[instanceCameraRig] = myCameraRig;
    _oldToNewComponent[instanceCameraRig.GetComponent<OVRManager>()] = myCameraRig.GetComponent<OVRManager>();

    ComponentUtility.CopyComponent(instanceCameraRig.leftEyeAnchor.GetComponent<Camera>()).MustBeTrue();
    ComponentUtility.PasteComponentValues(myCameraRig.leftEyeAnchor.GetComponent<Camera>()).MustBeTrue();
    _oldToNewComponent[instanceCameraRig.leftEyeAnchor.GetComponent<Camera>()] = myCameraRig.leftEyeAnchor.GetComponent<Camera>();

    ComponentUtility.CopyComponent(instanceCameraRig.rightEyeAnchor.GetComponent<Camera>()).MustBeTrue();
    ComponentUtility.PasteComponentValues(myCameraRig.rightEyeAnchor.GetComponent<Camera>()).MustBeTrue();
    _oldToNewComponent[instanceCameraRig.rightEyeAnchor.GetComponent<Camera>()] = myCameraRig.rightEyeAnchor.GetComponent<Camera>();

    copyComponents(instanceCameraRig.leftEyeAnchor, myCameraRig.leftEyeAnchor, typeof(Camera));
    copyComponents(instanceCameraRig.rightEyeAnchor, myCameraRig.rightEyeAnchor, typeof(Camera));
    copyComponents(instanceCameraRig.centerEyeAnchor, myCameraRig.centerEyeAnchor);

    fixReferences(myCameraRig.leftEyeAnchor);
    fixReferences(myCameraRig.rightEyeAnchor);
    fixReferences(myCameraRig.centerEyeAnchor);
    fixReferences(myCameraRig.transform);

    transferChildren(instanceCameraRig.leftEyeAnchor, myCameraRig.leftEyeAnchor);
    transferChildren(instanceCameraRig.rightEyeAnchor, myCameraRig.rightEyeAnchor);
    transferChildren(instanceCameraRig.centerEyeAnchor, myCameraRig.centerEyeAnchor);

    DestroyImmediate(instantiatedPrefab);
  }

  private void destroyComponentsExcept(Transform t, params System.Type[] except) {
    foreach (Component component in t.GetComponents<Component>()) {
      if (component is Transform) continue;

      bool shouldContinue = false;
      for (int i = 0; i < except.Length; i++) {
        if (component.GetType() == except[i]) {
          shouldContinue = true;
          break;
        }
      }

      if (shouldContinue) {
        continue;
      }

      DestroyImmediate(component);
    }
  }

  private void destroyChildren(Transform parent) {
    foreach (Transform t in parent.GetComponentsInChildren<Transform>()) {
      if (t != parent && t != null) {
        DestroyImmediate(t.gameObject);
      }
    }
  }

  private void transferChildren(Transform from, Transform to) {
    foreach (Transform t in from.GetComponentsInChildren<Transform>()) {
      if (t.parent == from) {
        foreach (Transform tt in t.GetComponentsInChildren<Transform>()) {
          _instancedGameObjects.Add(tt.gameObject);
        }
        t.SetParent(to, false);
        t.SetAsLastSibling();
      }
    }
  }

  private void fixReferences(Transform obj) {
    foreach (Component component in obj.GetComponents<Component>()) {
      SerializedObject serializedComponent = new SerializedObject(component);
      SerializedProperty iterator = serializedComponent.GetIterator();
      while (iterator.Next(true)) {
        if (iterator.propertyType == SerializedPropertyType.ObjectReference) {
          Component oldReference = iterator.objectReferenceValue as Component;
          Component newReference;
          if (oldReference != null) {
            if (_oldToNewComponent.TryGetValue(oldReference, out newReference)) {
              iterator.objectReferenceValue = newReference;
            }
          }
        }
      }
      serializedComponent.ApplyModifiedProperties();
    }
  }

  private Dictionary<Component, Component> _oldToNewComponent = new Dictionary<Component, Component>();
  private void copyComponents(Transform from, Transform to, params System.Type[] except) {
    HashSet<Component> _accountedComponents = new HashSet<Component>(to.GetComponents<Component>());

    foreach (Component oldComponent in from.GetComponents<Component>()) {
      if (oldComponent is Transform) continue;

      bool shouldContinue = false;
      for (int i = 0; i < except.Length; i++) {
        if (oldComponent.GetType() == except[i]) {
          shouldContinue = true;
          break;
        }
      }

      if (shouldContinue) {
        continue;
      }

      ComponentUtility.CopyComponent(oldComponent).MustBeTrue();
      ComponentUtility.PasteComponentAsNew(to.gameObject).MustBeTrue();

      foreach (Component newComponent in to.GetComponents<Component>()) {
        if (!_accountedComponents.Contains(newComponent)) {
          _instancedComponents.Add(newComponent);
          _accountedComponents.Add(newComponent);
          _oldToNewComponent[oldComponent] = newComponent;
        }
      }
    }
  }
}
