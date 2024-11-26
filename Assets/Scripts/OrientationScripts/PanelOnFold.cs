using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;

public class PanelOnFold : MonoBehaviour {
	public Camera mainCamera;
	public Camera splitFillCamera;
	public RectTransform panelRect;

	[Min(0.0f)]
	public float minHingeAngle = 0.0f;
	[Min(0.0f)]
	public float maxHingeAngle = 0.0f;

	public List<GameObject> disableOnFold;
	public List<GameObject> enableOnFold;

	[Range(0.0f, 180.0f)]
	public float simulateHingeAngle = 0.0f;
	private float _lastSimulatedAngle = 0.0f;

    public float currentHingeAngle {
		get {
			if (Application.isEditor)
				return simulateHingeAngle;
			else
				return null != HingeAngle.current ? HingeAngle.current.angle.ReadValue() : 0.0f;
		}
	}

    private void Awake() {
		ResetCameras();

        if (HingeAngle.current != null) {
            InputSystem.EnableDevice(HingeAngle.current);
        }
    }

	void ResetCameras() {
		mainCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		splitFillCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		splitFillCamera.enabled = false;
		panelRect.gameObject.SetActive(false);
		panelRect.anchorMax = new Vector2(0.0f, 1.0f);
		foreach (var go in disableOnFold) { go.SetActive(true); }
		foreach (var go in enableOnFold) { go.SetActive(false); }
	}

	public void OnOrientationChange(AndroidConfiguration orientationInfo) {
		bool horizontalFold = true;

		if (orientationInfo.orientation == AndroidOrientation.Portrait) {
            horizontalFold = false;
		} else if (orientationInfo.orientation == AndroidOrientation.Undefined) {
			// Might be running from the editor, try checking the UnityEngine.Device.Screen.orientation, and then test fold angle
			if (UnityEngine.Device.Screen.orientation == ScreenOrientation.Portrait || UnityEngine.Device.Screen.orientation == ScreenOrientation.PortraitUpsideDown)
				horizontalFold = false;
			else
				horizontalFold = currentHingeAngle > minHingeAngle && currentHingeAngle < maxHingeAngle;
        }

		OnFoldChange(horizontalFold);
    }

    void OnFoldChange(bool horizontalMode) {
		// If we are in a separating state and half-opened, split the screen
		if (horizontalMode) {
			// If the screen is already split, don't re-apply
			if (splitFillCamera.enabled)
				return;

			float yAnchor = 0.5f;

			// Resize the main camera to fit in the "top" portion of the screen
			mainCamera.rect = new Rect(0.0f, yAnchor, 1.0f, 1.0f);

			// ...while the panelRect is set on the lower portion of the screen.
			// Since panelRect is set to render as a ScreenSpace-Camera, let the camera rect determine render size
			splitFillCamera.rect = new Rect(0.0f, -yAnchor, 1.0f, 1.0f);
			
			panelRect.gameObject.SetActive(true);
			foreach (var go in disableOnFold) { go.SetActive(false); }
			foreach (var go in enableOnFold) { go.SetActive(true); }

			panelRect.anchorMin = Vector2.zero;
			panelRect.anchorMax = new Vector2(1.0f, 1.0f);
			panelRect.ForceUpdateRectTransforms();

			splitFillCamera.enabled = true;
		} else {
			ResetCameras();
		}
	}

	void TestHingeAngle() {
		if (Screen.orientation != ScreenOrientation.LandscapeLeft && Screen.orientation != ScreenOrientation.LandscapeRight)
			return;

		if (currentHingeAngle > minHingeAngle && currentHingeAngle < maxHingeAngle) {
			if (!splitFillCamera.enabled)
				OnFoldChange(true);
		} else if (splitFillCamera.enabled) {
			OnFoldChange(false);
		}
	}

	private void Update() {
		if (Application.isEditor) {
			if (!Mathf.Approximately(_lastSimulatedAngle, currentHingeAngle)) {
				TestHingeAngle();
                _lastSimulatedAngle = currentHingeAngle;
			}
		} else {
            // Check for changes to the hinge angle, and if we're in horizontal mode then activate the split camera
            TestHingeAngle();
        }
	}
}