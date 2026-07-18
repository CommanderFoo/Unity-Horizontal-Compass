using UnityEngine;
using UnityEngine.UIElements;

namespace net.pixeldepth.horizontal_compass {

	/// <summary>
	/// Data class representing a waypoint marker on the compass.
	/// </summary>
	public class Compass_Marker {

		#region Public Fields

		/// <summary>
		/// The world-space transform this marker tracks.
		/// </summary>
		public Transform target;

		/// <summary>
		/// A fixed world-space position this marker tracks when <see cref="use_world_position"/> is true.
		/// </summary>
		public Vector3 world_position;

		/// <summary>
		/// When true, the marker tracks <see cref="world_position"/> instead of <see cref="target"/>.
		/// </summary>
		public bool use_world_position;

		/// <summary>
		/// Optional icon to display for this marker.
		/// </summary>
		public Texture2D icon;

		/// <summary>
		/// Color tint for the marker.
		/// </summary>
		public Color color;

		/// <summary>
		/// The UI element representing this marker on the compass.
		/// </summary>
		public VisualElement element;

		/// <summary>
		/// The icon element within the marker.
		/// </summary>
		public VisualElement icon_element;

		/// <summary>
		/// The label showing distance to target.
		/// </summary>
		public Label distance_label;

		/// <summary>
		/// Cached visibility state to avoid repeated class changes.
		/// </summary>
		public bool is_visible = true;

		/// <summary>
		/// Cached distance for change detection.
		/// </summary>
		public int cached_distance = -1;

		/// <summary>
		/// Cached in-range state, used to fire the within-range event only on transitions.
		/// </summary>
		public bool in_range;

		#endregion

		#region Constructor

		public Compass_Marker(Transform target, Texture2D icon = null, Color? color = null) {
			this.target = target;
			this.icon = icon;
			this.color = color ?? Color.white;
		}

		public Compass_Marker(Vector3 world_position, Texture2D icon = null, Color? color = null) {
			this.world_position = world_position;
			this.use_world_position = true;
			this.icon = icon;
			this.color = color ?? Color.white;
		}

		#endregion

	}

}
