using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    /// <summary>
    /// <see cref="UnityEvent"/> that is invoked when the user places an object.
    /// </summary>
    [Serializable]
    public class ARObjectPlacementExtensionEvent : UnityEvent<ARObjectPlacementEventExtensionArgs>
    {
    }

    /// <summary>
    /// Event data associated with the event when the user places an object.
    /// </summary>
    public class ARObjectPlacementEventExtensionArgs
    {
        /// <summary>
        /// The Interactable that placed the object.
        /// </summary>
        public ARPlacementExtensionInteractable placementInteractable { get; set; }

        /// <summary>
        /// The object that was placed.
        /// </summary>
        public GameObject placementObject { get; set; }
    }

    /// <summary>
    /// Controls the placement of Prefabs via a tap gesture.
    /// </summary>
    [AddComponentMenu("XR/AR Placement Interactable", 22)]
    public class ARPlacementExtensionInteractable : ARBaseGestureInteractable
    {
        [SerializeField]
        [Tooltip("A GameObject to place when a ray cast from a user touch hits a plane.")]
        GameObject m_PlacementPrefab;

        /// <summary>
        /// A <see cref="GameObject"/> to place when a ray cast from a user touch hits a plane.
        /// </summary>
        public GameObject placementPrefab
        {
            get => m_PlacementPrefab;
            set => m_PlacementPrefab = value;
        }

        [SerializeField]
        [Tooltip("The LayerMask that Unity uses during an additional ray cast when a user touch does not hit any AR trackable planes.")]
        LayerMask m_FallbackLayerMask;

        /// <summary>
        /// The <see cref="LayerMask"/> that Unity uses during an additional ray cast
        /// when a user touch does not hit any AR trackable planes.
        /// </summary>
        public LayerMask fallbackLayerMask
        {
            get => m_FallbackLayerMask;
            set => m_FallbackLayerMask = value;
        }

        [SerializeField]
        ARObjectPlacementExtensionEvent m_ObjectPlaced = new ARObjectPlacementExtensionEvent();

        /// <summary>
        /// Gets or sets the event that is called when this Interactable places a new <see cref="GameObject"/> in the world.
        /// </summary>
        public ARObjectPlacementExtensionEvent objectPlaced
        {
            get => m_ObjectPlaced;
            set => m_ObjectPlaced = value;
        }

        readonly ARObjectPlacementEventExtensionArgs m_ObjectPlacementEventArgs = new ARObjectPlacementEventExtensionArgs();

        static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        /// <summary>
        /// Gets the pose for the object to be placed from a ray cast hit triggered by a <see cref="TapGesture"/>.
        /// </summary>
        /// <param name="gesture">The tap gesture that triggers the ray cast.</param>
        /// <param name="pose">When this method returns, contains the pose of the placement object based on the ray cast hit.</param>
        /// <returns>Returns <see langword="true"/> if there is a valid ray cast hit that hit the front of a plane.
        /// Otherwise, returns <see langword="false"/>.</returns>
        protected virtual bool TryGetPlacementPose(TapGesture gesture, out Pose pose)
        {
            // Raycast against the location the player touched to search for planes.
            var hit = xrOrigin != null
                ? GestureTransformationUtility.Raycast(gesture.startPosition, s_Hits, xrOrigin, TrackableType.PlaneWithinPolygon, m_FallbackLayerMask)
#pragma warning disable 618 // Calling deprecated property to help with backwards compatibility.
                : arSessionOrigin != null && GestureTransformationUtility.Raycast(gesture.startPosition, s_Hits, arSessionOrigin, TrackableType.PlaneWithinPolygon, m_FallbackLayerMask);
#pragma warning restore 618
            if (hit)
            {
                pose = s_Hits[0].pose;

                // Use hit pose and camera pose to check if hit test is from the
                // back of the plane, if it is, no need to create the anchor.
                // ReSharper disable once LocalVariableHidesMember -- hide deprecated camera property
                var camera = xrOrigin != null
                    ? xrOrigin.Camera
#pragma warning disable 618 // Calling deprecated property to help with backwards compatibility.
                    : (arSessionOrigin != null ? arSessionOrigin.camera : Camera.main);
#pragma warning restore 618
                if (camera == null)
                    return false;

                return Vector3.Dot(camera.transform.position - pose.position, pose.rotation * Vector3.up) >= 0f;
            }

            pose = default;
            return false;
        }

        /// <summary>
        /// Instantiates the placement object and positions it at the desired pose.
        /// </summary>
        /// <param name="pose">The pose at which the placement object will be instantiated.</param>
        /// <returns>Returns the instantiated placement object at the input pose.</returns>
        /// <seealso cref="placementPrefab"/>
        protected virtual GameObject PlaceObject(Pose pose)
        {
            var placementObject = Instantiate(m_PlacementPrefab, pose.position, pose.rotation);

            // Create anchor to track reference point and set it as the parent of placementObject.
            var anchor = new GameObject("PlacementAnchor").transform;
            anchor.position = pose.position;
            anchor.rotation = pose.rotation;
            placementObject.transform.parent = anchor;

            // Use Trackables object in scene to use as parent
            var trackablesParent = xrOrigin != null
                ? xrOrigin.TrackablesParent
#pragma warning disable 618 // Calling deprecated property to help with backwards compatibility.
                : (arSessionOrigin != null ? arSessionOrigin.trackablesParent : null);
#pragma warning restore 618
            if (trackablesParent != null)
                anchor.parent = trackablesParent;

            return placementObject;
        }

        /// <summary>
        /// Unity calls this method automatically when the user places an object.
        /// </summary>
        /// <param name="args">Event data containing a reference to the instantiated placement object.</param>
        protected virtual void OnObjectPlaced(ARObjectPlacementEventExtensionArgs args)
        {
            m_ObjectPlaced?.Invoke(args);
        }

        /// <inheritdoc />
        protected override bool CanStartManipulationForGesture(TapGesture gesture)
        {
            return gesture.targetObject == null;
        }

        /// <inheritdoc />
        protected override void OnEndManipulation(TapGesture gesture)
        {
            base.OnEndManipulation(gesture);

            if (gesture.isCanceled)
                return;

#pragma warning disable 618 // Calling deprecated property to help with backwards compatibility.
            if (xrOrigin == null && arSessionOrigin == null)
                return;
#pragma warning restore 618

            if (TryGetPlacementPose(gesture, out var pose))
            {
                var placementObject = PlaceObject(pose);

                m_ObjectPlacementEventArgs.placementInteractable = this;
                m_ObjectPlacementEventArgs.placementObject = placementObject;
                OnObjectPlaced(m_ObjectPlacementEventArgs);
            }
        }
    }
}