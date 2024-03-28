using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{

    public Button spawnBtn;

    public Button cancelBtn;

    public Button deleteSelectedBtn;

    public Button deleteAnchorBtn;

    public Button duplicateBtn;

    public Button takeScreenshotBtn;

    public Button listSpawnedBtn;

    public Button loadSaveBtn;

    public Button settingsBtn;

    public Button doneBtn;

    public GameObject mainMenu;

    public GameObject listObjectMenu;

    public GameObject interactingMenu;

    public GameObject interactingAnchorMenu;

    public GameObject listSpawnedModal;

    public GameObject exitConfirmModal;

    GameObject m_SelectedObject;

    private void Awake()
    {
        spawnBtn.onClick.AddListener(SpawnButtonClicked);
        cancelBtn.onClick.AddListener(CancelButtonClicked);
        deleteSelectedBtn.onClick.AddListener(DeleteSelectedBtnClicked);
        deleteAnchorBtn.onClick.AddListener(DeleteAnchorBtnClicked);
        duplicateBtn.onClick.AddListener(DuplicateBtnClicked);
        takeScreenshotBtn.onClick.AddListener(TakeScreenshotBtnClicked);
        listSpawnedBtn.onClick.AddListener(ListSpawnedBtnClicked);
        loadSaveBtn.onClick.AddListener(LoadSaveBtnClicked);
        settingsBtn.onClick.AddListener(SettingsBtnClicked);
        doneBtn.onClick.AddListener(DoneBtnClicked);
    }

    public void SpawnButtonClicked()
    {
        mainMenu.SetActive(false);
        listObjectMenu.SetActive(true);
        listObjectMenu.transform.localPosition = new Vector2(0, -Screen.height);
        listObjectMenu.transform.LeanMoveLocalY(0, 0.5f).setEaseOutExpo().delay = 0.1f;
    }

    public void CancelButtonClicked()
    {
        listObjectMenu.transform.LeanMoveLocalY(-Screen.height, 0.5f).setEaseInExpo().setOnComplete(RedirectToMainMenu);
    }
    void RedirectToMainMenu()
    {
        listObjectMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OnSelectObject(GameObject gameObject)
    {
        m_SelectedObject = gameObject;
        mainMenu.SetActive(false);
        interactingMenu.SetActive(true);
    }

    public void OnDeselectObject()
    {
        m_SelectedObject = null;
        interactingMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void DeleteSelectedBtnClicked()
    {
        if (m_SelectedObject != null)
        {
            Destroy(m_SelectedObject);
        }

        m_SelectedObject = null;
        interactingMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OnSelectAnchor(GameObject gameObject)
    {
        m_SelectedObject = gameObject;
        mainMenu.SetActive(false);
        interactingAnchorMenu.SetActive(true);
    }

    public void OnDeselectAnchor()
    {
        m_SelectedObject = null;
        interactingAnchorMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void DeleteAnchorBtnClicked()
    {
        if (m_SelectedObject != null)
        {
            Destroy(m_SelectedObject);
        }

        m_SelectedObject = null;
        interactingAnchorMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void DuplicateBtnClicked()
    {

    }

    public void TakeScreenshotBtnClicked()
    {

    }

    public void ListSpawnedBtnClicked()
    {
        var m_FurnitureManager = FindObjectOfType<FurnituresManager>();
        if (m_FurnitureManager != null)
        {
            m_FurnitureManager.OnClickSave();
        }
    }

    public void LoadSaveBtnClicked()
    {
        var m_FurnitureManager = FindObjectOfType<FurnituresManager>();
        if (m_FurnitureManager != null)
        {
            m_FurnitureManager.OnClickLoad();
        }
    }

    public void SettingsBtnClicked()
    {

    }

    public void DoneBtnClicked()
    {
        if (m_SelectedObject.TryGetComponent<GeneralAnchor>(out var generalAnchor))
        {
            generalAnchor.OnComplete();
        }
        m_SelectedObject = null;
        interactingAnchorMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
}
