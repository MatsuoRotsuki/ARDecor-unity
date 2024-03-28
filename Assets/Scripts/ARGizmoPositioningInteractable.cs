using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    [AddComponentMenu("XR/AR Gizmo Positioning Interactable", 22)]
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

        const float k_PositionSpeed = 0.5f;

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

        protected override bool CanStartManipulationForGesture(TwoFingerDragGesture gesture)
        {
            return IsGameObjectSelected();
        }

        protected override void OnStartManipulation(TwoFingerDragGesture gesture)
        {

        }

        protected override void OnContinueManipulation(TwoFingerDragGesture gesture)
        {
            var camera = xrOrigin != null
                ? xrOrigin.Camera
#pragma warning disable 618
                : (arSessionOrigin.Camera != null ? arSessionOrigin.camera : Camera.main);
#pragma warning restore 618
            if (camera == null)
                return;
            var cameraUp = camera.transform.up;
            var cameraForward = camera.transform.forward;
            var cameraRight = camera.transform.right;
            var cameraPosition = camera.transform.position;

            var interactableForward = transform.forward;
            var interactableUp = transform.up;
            var interactableRight = transform.right;

            Vector2 interactableForward2D = new Vector2(
                Vector3.Dot(interactableForward, cameraRight),
                Vector3.Dot(interactableForward, cameraUp)
            ).normalized;

            Vector2 interactableUp2D = new Vector2(
                Vector3.Dot(interactableUp, cameraRight),
                Vector3.Dot(interactableUp, cameraUp)
            ).normalized;

            Vector2 interactableRight2D = new Vector2(
                Vector3.Dot(interactableRight, cameraRight),
                Vector3.Dot(interactableRight, cameraUp)
            ).normalized;

            int alignAxes = 0;
            bool alignX = false, alignY = false, alignZ = false;
            if (Mathf.Abs(Vector2.Dot(interactableForward2D, gesture.delta.normalized)) > 0.9f)
            {
                alignZ = true;
                alignAxes++;
            }
            if (Mathf.Abs(Vector2.Dot(interactableUp2D, gesture.delta.normalized)) > 0.9f)
            {
                alignY = true;
                alignAxes++;
            }
            if (Mathf.Abs(Vector2.Dot(interactableRight2D, gesture.delta.normalized)) > 0.9f)
            {
                alignX = true;
                alignAxes++;
            }

            if (alignAxes == 1)
            {
                if (alignZ)
                {
                    transform.Translate(
                        interactableForward * Vector2.Dot(interactableForward2D, gesture.delta) * Time.deltaTime * 0.1f,
                        Space.World);
                }
                else if (alignY)
                {
                    transform.Translate(
                        interactableUp * Vector2.Dot(interactableUp2D, gesture.delta) * Time.deltaTime * 0.1f,
                        Space.World);
                }
                else if (alignX)
                {
                    transform.Translate(
                        interactableRight * Vector2.Dot(interactableRight2D, gesture.delta) * Time.deltaTime * 0.1f,
                        Space.World);
                }
            }
        }

        protected override void OnEndManipulation(TwoFingerDragGesture gesture)
        {

        }

        void UpdatePosition()
        {

        }
    }
}
