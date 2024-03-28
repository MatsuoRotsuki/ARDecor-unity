using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    [AddComponentMenu("XR/AR Translation Extension Interactable", 22)]
    [RequireComponent(typeof(ARSelectionInteractable))]
    public class ARTranslationExtensionInteractable : ARBaseGestureInteractable
    {
        [SerializeField]
        [Tooltip("Controls whether Unity constrains the object vertically, horizontally, or free to move in all axes.")]
        GestureTransformationHelper.GestureTranslationMode m_ObjectGestureTranslationMode;

        public GestureTransformationHelper.GestureTranslationMode objectGestureTranslationMode
        {
            get => m_ObjectGestureTranslationMode;
            set => m_ObjectGestureTranslationMode = value;
        }

        [SerializeField]
        [Tooltip("The maxium translation distance of this object.")]
        float m_MaxTranslationDistance = 10f;

        public float maxTranslationDistance
        {
            get => m_MaxTranslationDistance;
            set => maxTranslationDistance = value;
        }

        [SerializeField]
        [Tooltip("The LayerMask that Unity uses during an additional ray cast when a user touch does not hit any AR trackable planes.")]
        LayerMask m_FallbackLayerMask;

        public LayerMask fallbackLayerMask
        {
            get => m_FallbackLayerMask;
            set => m_FallbackLayerMask = value;
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
            m_GroundingPlaneHeight = transform.parent.position.y;
        }

        protected override void OnContinueManipulation(DragGesture gesture)
        {
            if (transform.parent == null)
            {
                Debug.LogError("Translation Interactable needs a parent object.", this);
                return;
            }

            m_IsActive = true;

            var desiredPlacement = xrOrigin != null
                ? GestureTransformationHelper.GetBestPlacementPosition(
                    transform.parent.position, gesture.position, m_GroundingPlaneHeight, 0.1f,
                    maxTranslationDistance, objectGestureTranslationMode, xrOrigin, gameObject, fallbackLayerMask: m_FallbackLayerMask)
#pragma warning disable 618
                : GestureTransformationHelper.GetBestPlacementPosition(
                    transform.parent.position, gesture.position, m_GroundingPlaneHeight, 0.1f,
                    maxTranslationDistance, objectGestureTranslationMode, arSessionOrigin, gameObject, fallbackLayerMask: m_FallbackLayerMask);
#pragma warning restore 618

            if (desiredPlacement.hasHoveringPosition && desiredPlacement.hasPlacementPosition)
            {
                // If desired position is lower than current position, don't drop it until it's finished.
                m_DesiredLocalPosition = transform.parent.InverseTransformPoint(desiredPlacement.hoveringPosition);
                m_DesiredAnchorPosition = desiredPlacement.placementPosition;

                m_GroundingPlaneHeight = desiredPlacement.updatedGroundingPlaneHeight;

                // Rotate if the plane direction has changed.
                if (((desiredPlacement.placementRotation * Vector3.up) - transform.up).magnitude > k_DiffThreshold)
                    m_DesiredRotation = desiredPlacement.placementRotation;
                else
                    m_DesiredRotation = transform.rotation;

                m_LastPlacement = desiredPlacement;
            }
        }

        protected override void OnEndManipulation(DragGesture gesture)
        {
            if (!m_LastPlacement.hasPlacementPosition)
                return;

            var oldAnchor = transform.parent.gameObject;
            bool isLastAnchorStackable = oldAnchor.TryGetComponent<StackableObject>(out var lastStackable);

            var desiredPose = new Pose(m_DesiredAnchorPosition, m_LastPlacement.placementRotation);

            if (!m_LastPlacement.hasStackable)
            {
                //Next anchor is not StackableObject
                var desiredLocalPosition = transform.parent.InverseTransformPoint(desiredPose.position);

                if (desiredLocalPosition.magnitude > maxTranslationDistance)
                    desiredLocalPosition = desiredLocalPosition.normalized * maxTranslationDistance;
                desiredPose.position = transform.parent.TransformPoint(desiredLocalPosition);

                var anchor = new GameObject("PlacementAnchor").transform;
                anchor.position = m_LastPlacement.placementPosition;
                anchor.rotation = m_LastPlacement.placementRotation;
                transform.parent = anchor;
                //anchor.transform.parent = 
                m_DesiredLocalPosition = Vector3.zero;
            }
            else
            {
                //Next anchor is StackableObject
                var anchor = m_LastPlacement.stackable.transform;
                transform.parent = anchor;
                m_DesiredLocalPosition = anchor.InverseTransformPoint(desiredPose.position);
            }

            if (!isLastAnchorStackable)
                Destroy(oldAnchor);

            // Rotate if the plane direction has changed.
            if (((desiredPose.rotation * Vector3.up) - transform.up).magnitude > k_DiffThreshold)
                m_DesiredRotation = desiredPose.rotation;
            else
                m_DesiredRotation = transform.rotation;

            // Make sure position is updated one last time.
            m_IsActive = true;
        }

        void UpdatePosition()
        {
            if (!m_IsActive)
                return;

            //Lerp position
            var oldLocalPosition = transform.localPosition;
            var newLocalPosition = Vector3.Lerp(
                oldLocalPosition, m_DesiredLocalPosition, Time.deltaTime * k_PositionSpeed);

            var diffLength = (m_DesiredLocalPosition - newLocalPosition).magnitude;
            if (diffLength < k_DiffThreshold)
            {
                newLocalPosition = m_DesiredLocalPosition;
                m_IsActive = false;
            }

            transform.localPosition = newLocalPosition;

            // Lerp rotation.
            var oldRotation = transform.rotation;
            var newRotation =
                Quaternion.Lerp(oldRotation, m_DesiredRotation, Time.deltaTime * k_PositionSpeed);
            transform.rotation = newRotation;
        }
    }
}
