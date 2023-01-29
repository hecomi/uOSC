using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uOSC
{

[CustomEditor(typeof(uOscServer))]
public class uOscServerEditor : Editor
{
    uOscServer server { get { return target as uOscServer; } }
    Queue<Message> messages = new Queue<Message>();
    Vector2 messageScrollPos = Vector2.zero;
    StringBuilder messageStringBuilder = new StringBuilder();

    void OnEnable()
    {
        server._onDataReceivedEditor.AddListener(OnMessage);
    }

    void OnDisable()
    {
        server._onDataReceivedEditor.RemoveListener(OnMessage);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (EditorUtil.Foldout("Server", true))
        {
            ++EditorGUI.indentLevel;
            DrawServer();
            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;
        }

        if (EditorUtil.Foldout("Events", false))
        {
            ++EditorGUI.indentLevel;
            DrawEvents();
            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;
        }

        if (EditorUtil.Foldout("Status", false))
        {
            ++EditorGUI.indentLevel;
            DrawStatus();
            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;
        }

        serializedObject.ApplyModifiedProperties();
    }

    void OnMessage(Message msg)
    {
        messages.Enqueue(msg);

        while (messages.Count > 100)
        {
            messages.Dequeue();
        }
    }

    void DrawServer()
    {
        EditorUtil.DrawProperty(serializedObject, nameof(uOscServer.port));
        EditorUtil.DrawProperty(serializedObject, nameof(uOscServer.autoStart));
    }

    void DrawEvents()
    {
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(10f, false);
        EditorUtil.DrawProperty(serializedObject, nameof(uOscServer.onDataReceived));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(10f, false);
        EditorUtil.DrawProperty(serializedObject, nameof(uOscServer.onServerStarted));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(10f, false);
        EditorUtil.DrawProperty(serializedObject, nameof(uOscServer.onServerStopped));
        EditorGUILayout.EndHorizontal();
    }

    void DrawStatus()
    {
        var skin = GUI.skin.label;
        skin.richText = true;
        var status = server.isRunning ?
            "<color=#5d5>Running</color>" :
            "<color=#888>Stop</color>";
        EditorGUILayout.LabelField("Status", status, skin);

        if (EditorUtil.SimpleFoldout("Messages", false))
        {
            EditorGUILayout.BeginVertical(GUILayout.MinHeight(200f));

            messageStringBuilder.Clear();
            messageScrollPos = EditorGUILayout.BeginScrollView(messageScrollPos, GUI.skin.box);
            foreach (var msg in messages.Reverse())
            {
                messageStringBuilder.AppendLine(msg.ToString());
            }
            var messageText = messageStringBuilder.ToString();
            GUILayout.Label(messageText);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Copy", GUILayout.MinWidth(100f)))
            {
                EditorGUIUtility.systemCopyBuffer = messageText;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            Repaint();
        }
    }
}

}
