using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    [AddComponentMenu("XR/AR Gizmo Translation Interactable", 22)]
    [RequireComponent(typeof(ARSelectionInteractable))]
    public class ARGizmoPositioningInteractable : ARBaseGestureInteractable
    {
        [SerializeField]
        [Tooltip("The maxium translation distance of this object.")]
        float m_MaxTranslationDistance = 10f;

        public float maxTranslationDistance
        {
            get => m_MaxTranslationDistance;
            set => maxTranslationDistance = value;
        }

        const float k_PositionSpeed = 12f;
        const float k_DiffThreshold = 0.0001f;

        bool m_IsActive;

        Vector3 m_DesiredLocalPosition;
        float m_GroundingPlaneHeight;
        Vector3 m_DesiredAnchorPosition;
        Quaternion m_DesiredRotation;
        GestureTransformationHelper.Placement m_LastPlacement;

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                UpdatePosition();
            }
        }

        protected override bool CanStartManipulationForGesture(DragGesture gesture)
        {
            return gesture.targetObject != null && gesture.targetObject == gameObject && transform.parent != null;
        }

        protected override void OnStartManipulation(DragGesture gesture)
        {

        }

        protected override void OnContinueManipulation(DragGesture gesture)
        {

        }

        protected override void OnEndManipulation(DragGesture gesture)
        {

        }

        void UpdatePosition()
        {

        }
    }
}
