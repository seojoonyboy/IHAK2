using UnityEditor;
using DataModules;
using UnityEngine;

public class MakeUpgradeInfoScriptableObject {
    [MenuItem("Assets/Create/Make UpgradeInfo Scriptable Object")]
    public static void Create() {
        UpgradeInfo asset = ScriptableObject.CreateInstance<UpgradeInfo>();

        AssetDatabase.CreateAsset(asset, "Assets/ScriptableObjects/NewUpgradeInfo.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
}
