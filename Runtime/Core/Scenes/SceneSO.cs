﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	[CreateAssetMenu(fileName = "Context/Create Context")]
	public class SceneSO : ScriptableObject
	{
	#if UNITY_EDITOR
	
			public UnityEditor.SceneAsset Scene;
	
			public string GUID;
	
	#endif

		public string ScenePath;

		public string SceneName;

		public bool IsGameContext = true;
	}
}

