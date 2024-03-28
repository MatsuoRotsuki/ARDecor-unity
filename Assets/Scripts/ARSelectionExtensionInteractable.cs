namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    /// <summary>
    /// Controls the selection of an object via a Tap gesture.
    /// </summary>
    [AddComponentMenu("XR/AR Selection Extension Interactable", 22)]
    public class ARSelectionExtensionInteractable : ARBaseGestureInteractable
    {
        [SerializeField, Tooltip("The visualization GameObject that will become active when the object is selected.")]
        GameObject m_SelectionVisualization;
        /// <summary>
        /// The visualization <see cref="GameObject"/> that will become active when the object is selected.
        /// </summary>
        public GameObject selectionVisualization
        {
            get => m_SelectionVisualization;
            set => m_SelectionVisualization = value;
        }

        bool m_GestureSelected;

        /// <inheritdoc />
        public override bool IsSelectableBy(IXRSelectInteractor interactor) => interactor is ARGestureInteractor && m_GestureSelected;

        /// <inheritdoc />
        protected override bool CanStartManipulationForGesture(TapGesture gesture) => true;

        /// <inheritdoc />
        protected override void OnEndManipulation(TapGesture gesture)
        {
            base.OnEndManipulation(gesture);

            if (gesture.isCanceled)
                return;
            if (gestureInteractor == null)
                return;

            if (gesture.targetObject == gameObject)
            {
                // Toggle selection
                m_GestureSelected = !m_GestureSelected;
            }
            else
                m_GestureSelected = false;
        }

        /// <inheritdoc />
        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);
            if (m_SelectionVisualization != null)
                m_SelectionVisualization.SetActive(true);
        }

        /// <inheritdoc />
        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            base.OnSelectExiting(args);
            if (m_SelectionVisualization != null)
                m_SelectionVisualization.SetActive(false);
        }
    }
}
