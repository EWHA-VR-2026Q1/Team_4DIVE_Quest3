using UnityEngine;
using UnityEditor;

public class TerrainLayerSetup
{
    [MenuItem("Tools/Fix Scene2_A Terrain Layers")]
    static void FixTerrainLayers()
    {
        var dirtTex  = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/HW_09/eunheay/Fantasy Forest Environment Free Sample/Textures/dirt01.tga");
        var grassTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/HW_09/eunheay/Fantasy Forest Environment Free Sample/Textures/grass01.tga");

        if (dirtTex == null)  { Debug.LogError("[TerrainSetup] dirt01.tga 못 찾음");  return; }
        if (grassTex == null) { Debug.LogError("[TerrainSetup] grass01.tga 못 찾음"); return; }

        string savePath = "Assets/HW_09/eunheay/";

        var dirtLayer = new TerrainLayer();
        dirtLayer.diffuseTexture = dirtTex;
        dirtLayer.tileSize = new Vector2(15, 15);
        AssetDatabase.CreateAsset(dirtLayer, savePath + "dirt_layer.terrainlayer");

        var grassLayer = new TerrainLayer();
        grassLayer.diffuseTexture = grassTex;
        grassLayer.tileSize = new Vector2(15, 15);
        AssetDatabase.CreateAsset(grassLayer, savePath + "grass_layer.terrainlayer");

        AssetDatabase.SaveAssets();

        var terrain = Object.FindObjectOfType<Terrain>();
        if (terrain == null) { Debug.LogError("[TerrainSetup] Terrain 오브젝트 없음"); return; }

        terrain.terrainData.terrainLayers = new TerrainLayer[] { dirtLayer, grassLayer };
        EditorUtility.SetDirty(terrain.terrainData);
        AssetDatabase.SaveAssets();

        Debug.Log($"[TerrainSetup] 완료! {terrain.name}에 dirt+grass 레이어 적용됨");
    }
}
