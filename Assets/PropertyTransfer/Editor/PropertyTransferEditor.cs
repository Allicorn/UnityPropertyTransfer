using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;

namespace AllicornsPropertyTransferTool
{
	[CustomEditor(typeof(PropertyTransfer))]
	[DisallowMultipleComponent]
	public class PropertyTransferEditor : Editor
	{
		SerializedProperty propSource;
		SerializedProperty propSourceType;
		SerializedProperty propDestination;
		SerializedProperty propDestinationType;

		private void OnEnable()
		{
			propSource = serializedObject.FindProperty("source");
			propSourceType = serializedObject.FindProperty("sourceType");
			propDestination = serializedObject.FindProperty("destination");
			propDestinationType = serializedObject.FindProperty("destinationType");
		}

		public override void OnInspectorGUI()
		{
			GUILayoutOption[] nops = new GUILayoutOption[0];

			EditorGUILayout.LabelField("Copy all serialized properties from one Component to another. ", nops);

			Assembly asm = typeof(PropertyTransfer).Assembly;
			PropertyTransfer pt = (PropertyTransfer)serializedObject.targetObject;

			List<string> srcComponentNames = new List<string>();
			srcComponentNames.Add("-- select a component --");
			if (pt.source != null)
			{
				Component[] srcComponents = pt.source.GetComponents<Component>();
				foreach (Component c in srcComponents)
					srcComponentNames.Add(c.ToString());
			}

			List<string> dstComponentNames = new List<string>();
			dstComponentNames.Add("-- select a component --");
			if (pt.destination != null)
			{
				Component[] dstComponents = pt.destination.GetComponents<Component>();
				foreach (Component c in dstComponents)
					dstComponentNames.Add(c.ToString());
			}

			int srcPopup = 0;
			for (int i = 0; i < srcComponentNames.Count; i += 1)
			{
				string s = srcComponentNames[i];
				if (s.Contains("("))
				{
					string cn = GetComponentName(s);
					if (pt.sourceType == cn)
					{
						srcPopup = i;
						break;
					}
				}
			}
			int dstPopup = 0;
			for (int i = 0; i < dstComponentNames.Count; i += 1)
			{
				string s = dstComponentNames[i];
				if (s.Contains("("))
				{
					string cn = GetComponentName(s);
					if (pt.destinationType == cn)
					{
						dstPopup = i;
						break;
					}
				}
			}

			int srcPopupOld = srcPopup;
			int dstPopupOld = dstPopup;

			serializedObject.Update();
			EditorGUILayout.PropertyField(propSource);
			serializedObject.ApplyModifiedProperties();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Component");
			srcPopup = EditorGUILayout.Popup(srcPopup, srcComponentNames.ToArray(), nops);
			EditorGUILayout.EndHorizontal();

			serializedObject.Update();
			EditorGUILayout.PropertyField(propDestination);
			serializedObject.ApplyModifiedProperties();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Component");
			dstPopup = EditorGUILayout.Popup(dstPopup, dstComponentNames.ToArray(), nops);
			EditorGUILayout.EndHorizontal();

			bool valid = true;

			if (srcPopup != srcPopupOld)
			{
				string s = GetComponentName(srcComponentNames[srcPopup]);
				if (string.IsNullOrEmpty(s))
					valid = false;
				else
					pt.sourceType = s;
			}
			else if (srcPopup == 0)
				valid = false;

			if (dstPopup != dstPopupOld)
			{
				string s = GetComponentName(dstComponentNames[dstPopup]);
				if (string.IsNullOrEmpty(s))
					valid = false;
				else
					pt.destinationType = s;
			}
			else if (dstPopup == 0)
				valid = false;

			if (valid)
				if (GUILayout.Button("Transfer Properties"))
					Transfer();
		}

		private void Transfer()
		{

			Assembly asm = typeof(PropertyTransfer).Assembly;
			PropertyTransfer pt = (PropertyTransfer)serializedObject.targetObject;

			string srcName = pt.sourceType + ", " + asm.ToString();
			Type srcType = Type.GetType(srcName);
			if (srcType == null)
				Debug.Log("Couldn't resolve type: " + srcName);

			string dstName = pt.destinationType + ", " + asm.ToString();
			Type dstType = Type.GetType(dstName);
			if (dstType == null)
				Debug.Log("Couldn't resolve type: " + dstName);

			object src = Convert.ChangeType(pt.source, srcType);
			object dst = Convert.ChangeType(pt.destination, dstType);

			FieldInfo[] srcFIs = srcType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(fi => Attribute.IsDefined(fi, typeof(SerializeField))).ToArray();
			FieldInfo[] dstFIs = dstType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(fi => Attribute.IsDefined(fi, typeof(SerializeField))).ToArray();

			Dictionary<string, FieldInfo> copy = new Dictionary<string, FieldInfo>();
			foreach (FieldInfo fi in srcFIs)
				copy.Add(fi.Name, fi);
			foreach (FieldInfo fi in dstFIs)
				if (copy.ContainsKey(fi.Name))
					fi.SetValue(dst, copy[fi.Name].GetValue(src));
		}

		private string GetComponentName(string s)
		{
			if (s.Contains("("))
			{
				string cn = s.Substring(s.IndexOf("(") + 1);
				cn = cn.Substring(0, cn.LastIndexOf(")"));
				return cn;
			}
			else
			{
				return "";
			}
		}
	}
}