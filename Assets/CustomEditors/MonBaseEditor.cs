using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(MonBase))]
[CanEditMultipleObjects]
public class MonBaseEditor : Editor
{
    SerializedProperty propertyName;
    SerializedProperty description;
    SerializedProperty frontSprite;
    SerializedProperty backSprite;
    SerializedProperty type1;
    SerializedProperty type2;

    SerializedProperty maxHp;
    SerializedProperty attack;
    SerializedProperty defense;
    SerializedProperty spAttack;
    SerializedProperty spDefense;
    SerializedProperty speed;

    SerializedProperty expYield;
    SerializedProperty growthRate;
    SerializedProperty catchRate;

    SerializedProperty learnableMoves;
    SerializedProperty learnableByItems;
    SerializedProperty evolutions;
    SerializedProperty movesLearnedUponEvolution;

    void OnEnable()
    {
        propertyName = serializedObject.FindProperty("name");
        description = serializedObject.FindProperty("description");
        frontSprite = serializedObject.FindProperty("frontSprite");
        backSprite = serializedObject.FindProperty("backSprite");
        type1 = serializedObject.FindProperty("type1");
        type2 = serializedObject.FindProperty("type2");
        
        maxHp = serializedObject.FindProperty("maxHp");
        attack = serializedObject.FindProperty("attack");
        defense = serializedObject.FindProperty("defense");
        spAttack = serializedObject.FindProperty("spAttack");
        spDefense = serializedObject.FindProperty("spDefense");
        speed = serializedObject.FindProperty("speed");

        expYield = serializedObject.FindProperty("expYield");
        growthRate = serializedObject.FindProperty("growthRate");
        catchRate = serializedObject.FindProperty("catchRate");

        learnableMoves = serializedObject.FindProperty("learnableMoves");
        learnableByItems = serializedObject.FindProperty("learnableByItems");
        evolutions = serializedObject.FindProperty("evolutions");
        movesLearnedUponEvolution = serializedObject.FindProperty("movesLearnedUponEvolution");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(propertyName);
        
        //[TextArea]
        EditorGUILayout.PropertyField(propertyName);
        
        EditorGUILayout.PropertyField(frontSprite);
        EditorGUILayout.PropertyField(backSprite);
        Sprite front = frontSprite.objectReferenceValue as Sprite;
        Sprite back = backSprite.objectReferenceValue as Sprite;
        
        var frontTexture = new Texture2D(64, 64);
        var backTexture = new Texture2D(64, 64);
        var frontData = front.texture.GetPixels(0, 0, 64, 64);
        var backData = back.texture.GetPixels(0, 0, 64, 64);
        frontTexture.SetPixels(frontData);
        backTexture.SetPixels(backData);
        frontTexture.Apply(true);
        backTexture.Apply(true);

        GUI.DrawTexture(new Rect(0, 64, 64, 64), frontTexture);
        GUI.DrawTexture(new Rect(64, 64, 64, 64), backTexture);

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
        EditorGUILayout.PropertyField(type1);
        EditorGUILayout.PropertyField(type2);
        EditorGUILayout.Space();
        
        EditorGUILayout.PropertyField(maxHp);
        EditorGUILayout.PropertyField(attack);
        EditorGUILayout.PropertyField(defense);
        EditorGUILayout.PropertyField(spAttack);
        EditorGUILayout.PropertyField(spDefense);
        EditorGUILayout.PropertyField(speed);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(expYield);
        EditorGUILayout.PropertyField(growthRate);
        EditorGUILayout.PropertyField(catchRate);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(learnableMoves);
        EditorGUILayout.PropertyField(learnableByItems);
        EditorGUILayout.PropertyField(evolutions);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(movesLearnedUponEvolution);
        EditorGUILayout.Space();
        
        serializedObject.ApplyModifiedProperties();
    }
}
