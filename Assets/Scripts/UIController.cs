using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    private VisualElement _bottomContainer;

    private Button _openBtn;

    private Button _closeBtn;

    private VisualElement _bottomSheet;

    private VisualElement _scrim;

    private VisualElement _image;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _bottomContainer = root.Q<VisualElement>("bottomContainer");

        _openBtn = root.Q<Button>("openBtn");
        _closeBtn = root.Q<Button>("closeBtn");

        _bottomSheet = root.Q<VisualElement>("bottomSheet");
        _scrim = root.Q<VisualElement>("backdrop");

        _image = root.Q<VisualElement>("image");

        _bottomContainer.style.display = DisplayStyle.None;
        _openBtn.RegisterCallback<ClickEvent>(OnOpenBtnClicked);
        _closeBtn.RegisterCallback<ClickEvent>(OnCloseBtnClicked);
        _bottomSheet.RegisterCallback<TransitionEndEvent>(OnBottomSheetDown);
    }

    private void OnDisable()
    {
        _openBtn.UnregisterCallback<ClickEvent>(OnOpenBtnClicked);
        _closeBtn.UnregisterCallback<ClickEvent>(OnCloseBtnClicked);
        _bottomSheet.UnregisterCallback<TransitionEndEvent>(OnBottomSheetDown);
    }

    private void OnOpenBtnClicked(ClickEvent evt)
    {
        _bottomContainer.style.display = DisplayStyle.Flex;
        _bottomSheet.AddToClassList("bottomsheet--up");
        _scrim.AddToClassList("scrim--fadein");

        AnimateImage();
    }

    private void AnimateImage()
    {
        _image.ToggleInClassList("image--down");
        _image.RegisterCallback<TransitionEndEvent>
        (
            evt => _image.ToggleInClassList("image--down")
        );
    }

    private void OnCloseBtnClicked(ClickEvent evt)
    {
        _bottomSheet.RemoveFromClassList("bottomsheet--up");
        _scrim.RemoveFromClassList("scrim--fadein");
    }

    private void OnBottomSheetDown(TransitionEndEvent evt)
    {
        if (!_bottomSheet.ClassListContains("bottomsheet--up"))
        {
            _bottomContainer.style.display = DisplayStyle.None;
        }
    }
}
