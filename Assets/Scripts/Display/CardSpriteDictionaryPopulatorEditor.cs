using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardSpriteDictionaryPopulator))]
public class CardSpriteDictionaryPopulatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();  // Draw the default inspector UI

        CardSpriteDictionaryPopulator populator = (CardSpriteDictionaryPopulator)target;

        // Add a button to the Inspector
        if (GUILayout.Button("Populate Card Sprite Dictionary"))
        {
            populator.PopulateDictionary();  // Call the method to populate the dictionary
        }
    }
}
