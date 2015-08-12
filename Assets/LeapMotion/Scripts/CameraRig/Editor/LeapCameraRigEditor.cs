using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(LeapCameraRig))]
public class LeapCameraRigEditor : Editor {

  public override void OnInspectorGUI() {
    base.OnInspectorGUI();

    if (GUILayout.Button("Load Camera Rig")) {
      string path = EditorUtility.OpenFilePanel("Select Camera Rig Prefab", Application.dataPath, "prefab");
      if (path != null && path != "") {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FileUtil.GetProjectRelativePath(path));

        if (prefab.GetComponentsInChildren<OVRCameraRig>(true).Length == 0 || prefab.GetComponentsInChildren<OVRManager>(true).Length == 0) {
          EditorUtility.DisplayDialog("Bad Prefab", "Prefab must be an OVR Camera Rig!", "Ok");
        } else {
          loadPrefabDialog(prefab);
        }
      }
    }

    SerializedProperty _previouslyAppliedPrefabProperty = serializedObject.FindProperty("_targetPrefab");
    GameObject previousPrefab = _previouslyAppliedPrefabProperty.objectReferenceValue as GameObject;
    if (previousPrefab != null) {
      if (GUILayout.Button("Re-apply " + previousPrefab.name)) {
        loadPrefabDialog(previousPrefab);
      }
    }
  }

  private void loadPrefabDialog(GameObject prefab) {
    LeapCameraRig cameraRig = target as LeapCameraRig;

    string destroyList = "";
    foreach (GameObject obj in cameraRig.instancedGameObjects) {
      destroyList += obj.name + "\n";
    }

    if (destroyList != "") {
      destroyList += "\n";
    }

    foreach (Component c in cameraRig.instancedComponents) {
      destroyList += c.gameObject.name + "." + c.GetType() + "\n";
    }

    if (destroyList != "") {
      if (!EditorUtility.DisplayDialog("Are you sure?", "This will destroy/modify the following objects:\n\n" + destroyList, "Replace", "Cancel")) {
        return;
      }
    }
    
    cameraRig.setTargetPrefab(prefab);
    EditorApplication.update += loadAdditivie;
  }

  private void loadAdditivie() {
    EditorApplication.update -= loadAdditivie;
    (target as LeapCameraRig).loadPrefabAdditive();
  }
}
