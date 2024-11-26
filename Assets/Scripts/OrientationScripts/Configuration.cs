using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

public class Configuration : MonoBehaviour {
    AndroidConfiguration m_PrevConfig;

	public UnityEvent<AndroidConfiguration> OnUIChanges;
	public UnityEvent OnScreenSizeChanges;
	public UnityEvent<AndroidUIModeNight> OnUINightMode;

    private ScreenOrientation lastOrientation;

    public void Start() {
        if (Application.platform == RuntimePlatform.Android) {
            m_PrevConfig = new AndroidConfiguration(AndroidApplication.currentConfiguration);
            AndroidApplication.onConfigurationChanged += OnConfigurationChanged;
        }

        lastOrientation = UnityEngine.Device.Screen.orientation;
    }

    public void OnDisable() {
        if (Application.platform == RuntimePlatform.Android) {
            AndroidApplication.onConfigurationChanged -= OnConfigurationChanged;
        }
    }

    private void OnConfigurationChanged(AndroidConfiguration newConfig) {
        if (m_PrevConfig.orientation != newConfig.orientation ||
            m_PrevConfig.screenLayoutSize != newConfig.screenLayoutSize) {
            OnUIChanges?.Invoke(newConfig);
        }

        if (m_PrevConfig.uiModeNight != newConfig.uiModeNight) {
            OnUINightMode?.Invoke(newConfig.uiModeNight);
        }

        if (m_PrevConfig.screenHeightDp != newConfig.screenHeightDp ||
            m_PrevConfig.screenWidthDp != newConfig.screenWidthDp) {
            OnScreenSizeChanges?.Invoke();
        }

        m_PrevConfig.CopyFrom(newConfig);
    }


    private void Update() {
        if (!Application.isEditor)
            return;

        if (UnityEngine.Device.Screen.orientation != lastOrientation) {
            lastOrientation = UnityEngine.Device.Screen.orientation;
            OnUIChanges?.Invoke(new AndroidConfiguration());
        }
    }
}