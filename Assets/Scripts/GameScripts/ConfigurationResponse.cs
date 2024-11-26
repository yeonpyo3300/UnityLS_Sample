using System.Collections;
using UnityEngine;

public class ConfigurationResponse : MonoBehaviour {
    public GameObject rootPauseScreen;
	public TMPro.TMP_Text labelPauseContinue;
	public float autoUnpauseTime = 1.0f;

	private void Awake() {
		UnpauseGame();
	}

	public void PauseGame(bool autoUnpause) {
        rootPauseScreen.SetActive(true);
        Time.timeScale = 0.0f;
		labelPauseContinue.text = "(tap to continue)";

		if (autoUnpause)
			StartCoroutine(UnpauseInTime(autoUnpauseTime));
    }

    public void UnpauseGame() {
		rootPauseScreen.SetActive(false);
		Time.timeScale = 1.0f;
	}

	IEnumerator UnpauseInTime(float time) {
		while (time > 0.0f && rootPauseScreen.activeInHierarchy) {
			time -= Time.unscaledDeltaTime;
			labelPauseContinue.text = string.Format("(game continues in {0:0.0}...)", time);
			yield return null;
		}

		UnpauseGame();
	}
}