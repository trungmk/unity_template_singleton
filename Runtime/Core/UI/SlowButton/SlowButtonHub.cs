using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowButtonHub : MonoBehaviour {

	static SlowButtonHub _instance;
	public static SlowButtonHub Instance {
		get {
			if (_instance == null) {
				GameObject g = new GameObject ("SlowButtonHub");
				_instance = g.AddComponent<SlowButtonHub> ();
				DontDestroyOnLoad(g);
			}
			return _instance;
		}
	}

	public const float defaultPauseTime = 0.35f;
	public float timer = 0;

	public bool CanClick () {
		if (timer > 0)
			return false;
		return true;
	}

	public void OnClick (float pauseTime = defaultPauseTime) {
		timer = pauseTime;
	}

	void FixedUpdate()
	{
		if (timer > 0) {
			timer -= Time.fixedDeltaTime;
		}
	}
}
