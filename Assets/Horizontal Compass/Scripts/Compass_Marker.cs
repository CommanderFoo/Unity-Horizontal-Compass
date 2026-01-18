using UnityEngine;
using UnityEngine.UIElements;

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

	#endregion

	#region Constructor

	public Compass_Marker(Transform target, Texture2D icon = null, Color? color = null) {
		this.target = target;
		this.icon = icon;
		this.color = color ?? Color.white;
	}

	#endregion

}
