using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Kirurobo;

public class VrmUiController : MonoBehaviour
{

    public WindowController windowController;

    public RectTransform panel;
    public Text informationText;
    public Text warningText;
    public Button closeButton;
    public Toggle transparentToggle;
    public Toggle topmostToggle;
    public Toggle maximizeToggle;
    public Button openButton;
    public Button quitButton;
    public Text titleText;

    public CameraController.ZoomMode zoomMode { get; set; }
    public Dropdown zoomModeDropdown;
    public Dropdown languageDropdown;

    public Toggle motionToggleRandom;
    public Toggle faceToggleRandom;

    public Button tabButtonModel;
    public Button tabButtonControl;
    public RectTransform modelPanel;
    public RectTransform controlPanel;

    private float mouseMoveSS = 0f;             // Sum of mouse trajectory squares. [px^2]
    private float mouseMoveSSThreshold = 16f;   // Threshold to be regarded as not moving. [px^2]
    private Vector3 lastMousePosition;

    private bool isDebugMode = false;   // Show debug Info.

    private VRMLoader.VRMPreviewLocale vrmLoaderLocale;
    private VRMLoader.VRMPreviewUI vrmLoaderUI;
    private VrmUiLocale uiLocale;

    private TabPanelManager tabPanelManager;

    /// <summary>
    /// ランダムモーションが有効かを取得／設定
    /// </summary>
    public bool enableRandomMotion
    {
        get
        {
            if (motionToggleRandom) return motionToggleRandom.isOn;
            return false;
        }
        set
        {
            if (motionToggleRandom) motionToggleRandom.isOn = value;
        }
    }

    /// <summary>
    /// ランダム表情が有効かを取得／設定
    /// </summary>
    public bool enableRandomEmotion
    {
        get
        {
            if (faceToggleRandom) return faceToggleRandom.isOn;
            return false;
        }
        set
        {
            if (faceToggleRandom) faceToggleRandom.isOn = value;
        }
    }

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {

        windowController = FindObjectOfType<WindowController>();
        windowController.OnStateChanged += windowController_OnStateChanged;

        zoomMode = CameraController.ZoomMode.Zoom;

        vrmLoaderLocale = this.GetComponentInChildren<VRMLoader.VRMPreviewLocale>();
        vrmLoaderUI = this.GetComponentInChildren<VRMLoader.VRMPreviewUI>();
        uiLocale = this.GetComponentInChildren<VrmUiLocale>();
        tabPanelManager = this.GetComponentInChildren<TabPanelManager>();

        // Initialize toggles.
        UpdateUI();

        // Set event listeners.
        if (closeButton) { closeButton.onClick.AddListener(Close); }
        if (quitButton) { quitButton.onClick.AddListener(Quit); }
        if (transparentToggle) { transparentToggle.onValueChanged.AddListener(windowController.SetTransparent); }
        if (maximizeToggle) { maximizeToggle.onValueChanged.AddListener(windowController.SetMaximized); }
        if (topmostToggle) { topmostToggle.onValueChanged.AddListener(windowController.SetTopmost); }
        if (zoomModeDropdown)
        {
            zoomModeDropdown.onValueChanged.AddListener(val => SetZoomMode(val));
            zoomModeDropdown.value = 0;
        }
        if (languageDropdown)
        {
            languageDropdown.onValueChanged.AddListener(val => SetLanguage(val));
            languageDropdown.value = 1;
        }

        // Show menu on startup.
        Show(null);
    }

    /// <summary>
    /// マウスホイールでのズーム方法を選択
    /// </summary>
    /// <param name="no">選択肢の番号（Dropdownを編集したら下記も要編集）</param>
    private void SetZoomMode(int no)
    {
        if (no == 1)
        {
            zoomMode = CameraController.ZoomMode.Dolly;
        }
        else
        {
            zoomMode = CameraController.ZoomMode.Zoom;
        }
    }

    /// <summary>
    /// UI言語選択
    /// </summary>
    /// <param name="no">選択肢の番号（Dropdownを編集したら下記も要編集）</param>
    private void SetLanguage(int no)
    {
        string lang;
        switch (no)
        {
            case 0:
                lang = "en";
                break;
            case 1:
                lang = "ja";
                break;
            default:
                lang = "en";
                break;
        }

        if (vrmLoaderLocale) vrmLoaderLocale.SetLocale(lang);
        if (uiLocale) uiLocale.SetLocale(lang);
    }

    private void windowController_OnStateChanged()
    {
        UpdateUI();
    }

    /// <summary>
    /// UIの状況を現在のウィンドウ状態に合わせて更新
    /// </summary>
    public void UpdateUI()
    {
        if (!windowController) return;

        if (transparentToggle) { transparentToggle.isOn = windowController.isTransparent; }
        if (maximizeToggle) { maximizeToggle.isOn = windowController.isMaximized; }
        if (topmostToggle) { topmostToggle.isOn = windowController.isTopmost; }
    }


    /// <summary>
    /// メニューを閉じる
    /// </summary>
    private void Close()
    {
        panel.gameObject.SetActive(false);
    }

    /// <summary>
    /// 終了ボタンが押された時の処理。エディタ上であれば再生停止とする。
    /// </summary>
    private void Quit()
    {
#if UNITY_EDITOR
        // Stop playing for the editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit application for the standalone player
        Application.Quit();
#endif
    }


    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // マウス右ボタンクリックでメニューを表示させる。閾値以下の移動ならクリックとみなす。
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(1))
        {
            mouseMoveSS += (Input.mousePosition - lastMousePosition).sqrMagnitude;
        }
        if (Input.GetMouseButtonUp(1))
        {
            if (mouseMoveSS < mouseMoveSSThreshold)
            {
                Show();
            }
            mouseMoveSS = 0f;
        }

        // [ESC] でメニューを閉じる
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }

        // 裏機能
        // メニューのタイトルがマウスカーソル直下の色になる
        if (isDebugMode && windowController && titleText)
        {
            titleText.color = windowController.pickedColor;
            //Debug.Log(windowController.pickedColor);
        }
    }

    /// <summary>
    /// フォーカスが外れたときの処理
    /// </summary>
    /// <param name="focus"></param>
    private void OnApplicationFocus(bool focus)
    {
        // フォーカスが外れたらメニューを閉じる
        if (!focus)
        {
            Close();
        }
    }

    /// <summary>
    /// メニューを表示する
    /// </summary>
    public void Show()
    {
        if (panel)
        {
            panel.gameObject.SetActive(true);
        }

        // 裏機能
        // メニューが表示されるとき、[Shift]が押されていればデバッグ表示を有効にする
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            isDebugMode = true;
        }
        else
        {
            isDebugMode = false;
        }
    }

    /// <summary>
    /// Show the meta information
    /// </summary>
    /// <param name="meta"></param>
    public void Show(VRM.VRMMetaObject meta)
    {
        if (meta)
        {
            if (vrmLoaderUI) vrmLoaderUI.setMeta(meta);
            if (tabPanelManager) tabPanelManager.Select(0); // 0番がモデル情報のパネルという前提
        }

        Show();
    }

    /// <summary>
    /// Set the warning text
    /// </summary>
    /// <param name="message"></param>
    public void SetWarning(string message)
    {
        if (warningText)
        {
            warningText.text = message;
        }
    }
}
