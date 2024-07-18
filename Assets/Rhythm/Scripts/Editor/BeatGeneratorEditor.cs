using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BeatGenerator))]
public class BeatGeneratorEditor : Editor
{
    private BeatGenerator m_Generator;

    private void OnEnable()
    {
        m_Generator = (BeatGenerator)target;

        EditorApplication.update += UpdateProgressBar;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateProgressBar;
    }

    private void UpdateProgressBar()
    {
        Repaint();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ShowProgressBar();
        ShowGenerateButtons();
        ShowSaveButton();
    }

    private void ShowGenerateButtons()
    {
        EditorGUILayout.Space();

        float buttonHeight = EditorGUIUtility.singleLineHeight * 1.5f;

        if (!EditorApplication.isPlaying)
        {
            if (GUILayout.Button("Enter Play Mode", GUILayout.Height(buttonHeight)))
            {
                EditorApplication.isPlaying = true;
            }
        }
        else
        {
            GUILayout.Label("Generate", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();


            if (GUILayout.Button("Start", GUILayout.Height(buttonHeight)))
            {
                m_Generator.StartGenerating();
            }

            if (GUILayout.Button("Stop", GUILayout.Height(buttonHeight)))
            {
                m_Generator.StopGenerating();
            }

            GUILayout.EndHorizontal();
        }
    }

    private void ShowProgressBar()
    {
        if (!Application.isPlaying || !m_Generator.Source.clip)
            return;

        EditorGUILayout.Space();

        float normalizedProgressValue = Mathf.Clamp01(m_Generator.Source.time / m_Generator.Source.clip.length);
        float percentageProgressValue = normalizedProgressValue * 100.0f;
        string progressText = $"{percentageProgressValue:F2}%";

        GUILayout.Label("Progress", EditorStyles.boldLabel);
        Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        EditorGUI.ProgressBar(rect, normalizedProgressValue, progressText);
    }

    private void ShowSaveButton()
    {
        if (!Application.isPlaying || m_Generator.BassData.Count == 0)
            return;

        float buttonHeight = EditorGUIUtility.singleLineHeight * 1.5f;

        if (GUILayout.Button("Save Beat Data", GUILayout.Height(buttonHeight)))
        {
            m_Generator.GeneratePeakData();
            m_Generator.GenerateBeatData();
            SaveBeatData();
        }
    }

    private void SaveBeatData()
    {
        List<Vector2> beatData = m_Generator.BeatData;

        Beat[] beats = new Beat[beatData.Count];
        for (int i = 0; i < beats.Length; i++)
            beats[i] = new(beatData[i].x, beatData[i].y);

        AudioData audioData = ScriptableObject.CreateInstance<AudioData>();

        audioData.clip = m_Generator.Source.clip;
        audioData.beats = beats;

        string path = $"Assets/{audioData.clip.name}.asset";
        AssetDatabase.CreateAsset(audioData, path);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = audioData;
    }
}
