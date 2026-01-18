using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Controls the horizontal compass UI, updating it based on camera rotation
/// and managing waypoint markers.
/// </summary>
[ExecuteAlways]
public class Compass_Controller : MonoBehaviour {

	#region Serialized Fields

	[Header("UI Reference")]
	[SerializeField, Tooltip("Drag the UIDocument component that displays the compass.")]
	private UIDocument ui_document;

	[Header("Compass Settings")]
	[SerializeField, Tooltip("How wide the compass bar is in pixels. Should match the width set in USS (--compass-width).")]
	private float compass_width = 800f;

	[SerializeField, Tooltip("How much of the world the compass shows at once. Lower = zoomed in (ticks spread apart), Higher = zoomed out (ticks closer together). 150 means you see 150 degrees of the world.")]
	private float compass_fov = 150f;

	[Header("Editor Preview")]
	[SerializeField, Tooltip("Simulates where the player is looking in edit mode. 0 = North, 90 = East, 180 = South, 270 = West."), Range(0f, 360f)]
	private float editor_preview_heading = 0f;

	[Header("Player Reference")]
	[SerializeField, Tooltip("The player's position. Used to calculate distance to markers. If empty, uses the camera position.")]
	private Transform player_transform;

	[Header("Markers")]
	[SerializeField, Tooltip("Markers to display on the compass. Configure targets, icons, and colors.")]
	private List<Compass_Marker_Data> editor_markers = new List<Compass_Marker_Data>();

	#endregion

	#region Private State

	// UI Elements
	private VisualElement compass_strip;
	private VisualElement compass_markers;

	// Editor marker runtime references
	private List<Compass_Marker> editor_marker_instances = new List<Compass_Marker>();

	// Camera reference
	private Transform cam_transform;

	// Compass calculations
	private float pixels_per_degree;
	private float strip_offset;
	private float half_fov;

	// Markers
	private List<Compass_Marker> markers = new List<Compass_Marker>();

	// Cardinal directions with their angles
	private static readonly (string label, float angle)[] cardinals = {
		("N", 0f),
		("NE", 45f),
		("E", 90f),
		("SE", 135f),
		("S", 180f),
		("SW", 225f),
		("W", 270f),
		("NW", 315f)
	};

	#endregion

	#region Unity Lifecycle

	private void Awake() {
		initialize_settings();
	}

	private void OnEnable() {
		initialize_settings();

		if (ui_document == null) {
			ui_document = GetComponent<UIDocument>();
		}

		if (ui_document != null && ui_document.rootVisualElement != null) {
			ui_document.rootVisualElement.RegisterCallback<GeometryChangedEvent>(on_geometry_changed);
		}

		#if UNITY_EDITOR
		EditorApplication.update += OnEditorUpdate;
		#endif
	}

	#if UNITY_EDITOR
	private void OnEditorUpdate() {
		if (Application.isPlaying) {
			return;
		}

		// Check if compass needs regeneration
		if (ui_document == null) {
			ui_document = GetComponent<UIDocument>();
		}

		if (ui_document == null || ui_document.rootVisualElement == null) {
			return;
		}

		VisualElement root = ui_document.rootVisualElement;

		if (root.childCount == 0) {
			return;
		}

		// Re-fetch strip reference
		VisualElement current_strip = root.Q<VisualElement>("compass-strip");

		// Check if we need to reinitialize
		bool needs_init = compass_strip == null ||
		                  current_strip != compass_strip ||
		                  (current_strip != null && current_strip.childCount == 0);

		if (needs_init && current_strip != null) {
			initialize_compass();
			editor_marker_instances.Clear();
			sync_editor_markers();
		}

		// Always update rotation and markers in editor
		if (compass_strip != null) {
			update_compass_rotation();
			update_markers();
		}
	}
	#endif

	private void initialize_settings() {
		if (Camera.main != null) {
			cam_transform = Camera.main.transform;
		}

		pixels_per_degree = compass_width / compass_fov;
		half_fov = compass_fov * 0.5f;

		// Strip needs to cover -180 to 540 degrees for seamless wrapping
		// That's 720 degrees total
		strip_offset = 180f * pixels_per_degree;
	}

	private void OnDisable() {
		if (ui_document != null && ui_document.rootVisualElement != null) {
			ui_document.rootVisualElement.UnregisterCallback<GeometryChangedEvent>(on_geometry_changed);
		}

		#if UNITY_EDITOR
		EditorApplication.update -= OnEditorUpdate;
		#endif
	}

	private void Update() {
		// Try to initialize if not yet done
		if (compass_strip == null || compass_markers == null) {
			try_initialize();
		}

		// Regenerate if elements were cleared (e.g., USS reload)
		if (compass_strip != null && compass_strip.childCount == 0) {
			initialize_settings();
			generate_compass_elements();
			// Clear and recreate marker instances
			editor_marker_instances.Clear();
			sync_editor_markers();
		}

		update_compass_rotation();

		#if UNITY_EDITOR
		// Only sync editor markers in edit mode (they don't change during play)
		if (!Application.isPlaying) {
			sync_editor_markers();
		}
		#endif

		update_markers();
	}

	private void try_initialize() {
		if (ui_document == null) {
			ui_document = GetComponent<UIDocument>();
		}

		if (ui_document == null || ui_document.rootVisualElement == null) {
			return;
		}

		VisualElement root = ui_document.rootVisualElement;

		if (root.childCount == 0) {
			return;
		}

		if (compass_strip == null) {
			compass_strip = root.Q<VisualElement>("compass-strip");
		}

		if (compass_markers == null) {
			compass_markers = root.Q<VisualElement>("compass-markers");
		}

		// Initialize compass elements if strip was just found
		if (compass_strip != null && compass_strip.childCount == 0) {
			// Get width from wrapper
			VisualElement compass_wrapper = root.Q<VisualElement>("compass-wrapper");

			if (compass_wrapper != null && compass_wrapper.resolvedStyle.width > 0) {
				compass_width = compass_wrapper.resolvedStyle.width;
			}

			initialize_settings();
			generate_compass_elements();

			// Sync editor markers on initial setup
			editor_marker_instances.Clear();
			sync_editor_markers();
		}
	}

	#if UNITY_EDITOR
	private void OnValidate() {
		if (!Application.isPlaying) {
			// Try to initialize if UI references were lost (e.g., after recompile)
			if (compass_strip == null || compass_markers == null) {
				try_initialize();
			}

			if (compass_strip != null) {
				initialize_settings();
				update_compass_rotation();
				sync_editor_markers();
			}
		}
	}
	#endif

	private void sync_editor_markers() {
		if (compass_markers == null) {
			return;
		}

		// Remove markers that are no longer in the editor list
		for (int i = editor_marker_instances.Count - 1; i >= 0; i--) {
			Compass_Marker instance = editor_marker_instances[i];
			bool found = false;

			foreach (Compass_Marker_Data data in editor_markers) {
				if (data.target == instance.target) {
					found = true;
					break;
				}
			}

			if (!found) {
				remove_marker(instance);
				editor_marker_instances.RemoveAt(i);
			}
		}

		// Add or update markers from the editor list
		foreach (Compass_Marker_Data data in editor_markers) {
			if (data.target == null) {
				continue;
			}

			// Find existing instance
			Compass_Marker existing = null;

			foreach (Compass_Marker instance in editor_marker_instances) {
				if (instance.target == data.target) {
					existing = instance;
					break;
				}
			}

			if (existing == null) {
				// Create new marker
				Compass_Marker new_marker = add_marker(data.target, data.icon, data.color);

				if (new_marker != null) {
					editor_marker_instances.Add(new_marker);
				}
			} else {
				// Update existing marker if properties changed
				if (existing.icon != data.icon || existing.color != data.color) {
					existing.icon = data.icon;
					existing.color = data.color;
					apply_marker_visuals(existing);
				}
			}
		}
	}

	private void apply_marker_visuals(Compass_Marker marker) {
		if (marker.icon_element == null) {
			return;
		}

		if (marker.icon != null) {
			marker.icon_element.style.backgroundImage = new StyleBackground(marker.icon);
			marker.icon_element.style.unityBackgroundImageTintColor = marker.color;
			marker.icon_element.style.backgroundColor = StyleKeyword.None;
		} else {
			marker.icon_element.style.backgroundImage = StyleKeyword.None;
			marker.icon_element.style.backgroundColor = marker.color;
		}
	}

	#endregion

	#region Initialization

	private void on_geometry_changed(GeometryChangedEvent evt) {
		// Re-fetch references in case the visual tree was rebuilt (e.g., USS reload)
		VisualElement root = ui_document.rootVisualElement;
		VisualElement new_strip = root.Q<VisualElement>("compass-strip");

		// Check if we need to reinitialize (first time or after reload)
		bool needs_init = compass_strip == null || new_strip != compass_strip || (compass_strip != null && compass_strip.childCount == 0);

		if (needs_init) {
			initialize_compass();
			// Clear cached marker instances so they get recreated
			editor_marker_instances.Clear();
			sync_editor_markers();
		}
	}

	private void initialize_compass() {
		VisualElement root = ui_document.rootVisualElement;
		VisualElement compass_wrapper = root.Q<VisualElement>("compass-wrapper");

		compass_strip = root.Q<VisualElement>("compass-strip");
		compass_markers = root.Q<VisualElement>("compass-markers");

		if (compass_strip == null) {
			Debug.LogError("Compass_Controller: Could not find compass-strip element.");
			return;
		}

		// Get actual compass width from the wrapper
		if (compass_wrapper != null && compass_wrapper.resolvedStyle.width > 0) {
			compass_width = compass_wrapper.resolvedStyle.width;
			pixels_per_degree = compass_width / compass_fov;
			strip_offset = 180f * pixels_per_degree;
		}

		generate_compass_elements();
	}

	private void generate_compass_elements() {
		compass_strip.Clear();

		// Calculate strip width for -180 to 540 degrees (720 degrees total)
		float strip_width = 720f * pixels_per_degree;

		compass_strip.style.width = strip_width;

		// Generate elements from -180 to 540 degrees
		for (int degree = -180; degree <= 540; degree += 5) {
			float x_pos = (degree + 180f) * pixels_per_degree;

			// Create tick mark
			VisualElement tick = create_tick(degree, x_pos);

			compass_strip.Add(tick);

			// Add degree label every 15 degrees (but not at cardinals)
			if (degree % 15 == 0 && !is_cardinal_direction(normalize_angle(degree))) {
				Label degree_label = create_degree_label(degree, x_pos);

				compass_strip.Add(degree_label);
			}
		}

		// Add cardinal labels (need multiple instances for wrapping)
		foreach ((string label, float angle) in cardinals) {
			// Add at the base angle
			add_cardinal_label(label, angle);
			// Add wrapped instance at angle - 360 (for negative range)
			add_cardinal_label(label, angle - 360f);
			// Add wrapped instance at angle + 360 (for positive overflow)
			add_cardinal_label(label, angle + 360f);
		}
	}

	private VisualElement create_tick(int degree, float x_pos) {
		VisualElement tick = new VisualElement();

		tick.AddToClassList("compass-tick");

		int normalized = normalize_angle(degree);

		if (is_cardinal_direction(normalized)) {
			tick.AddToClassList("compass-tick-large");
		} else if (normalized % 15 == 0) {
			tick.AddToClassList("compass-tick-medium");
		} else {
			tick.AddToClassList("compass-tick-small");
		}

		tick.style.left = x_pos;
		tick.pickingMode = PickingMode.Ignore;

		return tick;
	}

	private Label create_degree_label(int degree, float x_pos) {
		int display_degree = normalize_angle(degree);
		Label label = new Label(display_degree.ToString());

		label.AddToClassList("compass-degree");
		label.style.left = x_pos;
		label.pickingMode = PickingMode.Ignore;

		return label;
	}

	private void add_cardinal_label(string text, float angle) {
		// Only add if within our range (-180 to 540)
		if (angle < -180f || angle > 540f) {
			return;
		}

		float x_pos = (angle + 180f) * pixels_per_degree;
		Label label = new Label(text);

		label.AddToClassList("compass-label");
		label.AddToClassList("compass-label-cardinal");
		label.style.left = x_pos;
		label.pickingMode = PickingMode.Ignore;

		compass_strip.Add(label);
	}

	private bool is_cardinal_direction(int degree) {
		return degree == 0 || degree == 45 || degree == 90 || degree == 135 || degree == 180 || degree == 225 || degree == 270 || degree == 315;
	}

	private int normalize_angle(int degree) {
		int normalized = degree % 360;

		if (normalized < 0) {
			normalized += 360;
		}
		
		return normalized;
	}

	#endregion

	#region Compass Rotation

	private void update_compass_rotation() {
		if (compass_strip == null) {
			return;
		}

		float current_yaw;

		#if UNITY_EDITOR
		if (!Application.isPlaying) {
			current_yaw = editor_preview_heading;
		} else {
			current_yaw = cam_transform != null ? get_camera_yaw() : editor_preview_heading;
		}
		#else
		if (cam_transform == null) {
			return;
		}

		current_yaw = get_camera_yaw();
		#endif

		// Calculate position to center the current heading
		// The strip is positioned so that 0 degrees is at strip_offset from the left
		float left_position = -current_yaw * pixels_per_degree - strip_offset + (compass_width / 2f);

		compass_strip.style.left = left_position;
	}

	private float get_camera_yaw() {
		Vector3 forward = cam_transform.forward;
		float yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;

		// Normalize to 0-360
		if (yaw < 0f) {
			yaw += 360f;
		}

		return yaw;
	}

	#endregion

	#region Marker Management

	/// <summary>
	/// Add a waypoint marker to the compass.
	/// </summary>
	/// <param name="target">The transform to track.</param>
	/// <param name="icon">Optional icon texture.</param>
	/// <param name="color">Optional color tint. Defaults to white.</param>
	/// <returns>The created marker for later reference.</returns>
	public Compass_Marker add_marker(Transform target, Texture2D icon = null, Color? color = null) {
		if (compass_markers == null) {
			return null;
		}

		Color marker_color = color ?? Color.white;
		Compass_Marker marker = new Compass_Marker(target, icon, marker_color);

		// Create UI elements
		marker.element = new VisualElement();
		marker.element.AddToClassList("compass-marker");

		marker.icon_element = new VisualElement();
		marker.icon_element.AddToClassList("compass-marker-icon");

		// Apply icon and color
		if (icon != null) {
			marker.icon_element.style.backgroundImage = new StyleBackground(icon);
			marker.icon_element.style.unityBackgroundImageTintColor = marker_color;
			marker.icon_element.style.backgroundColor = StyleKeyword.None;
		} else {
			marker.icon_element.style.backgroundColor = marker_color;
		}

		marker.distance_label = new Label();
		marker.distance_label.AddToClassList("compass-marker-distance");

		marker.element.Add(marker.icon_element);
		marker.element.Add(marker.distance_label);

		compass_markers.Add(marker.element);
		markers.Add(marker);

		return marker;
	}

	/// <summary>
	/// Remove a waypoint marker from the compass.
	/// </summary>
	/// <param name="marker">The marker to remove.</param>
	public void remove_marker(Compass_Marker marker) {
		if (marker == null) {
			return;
		}

		if (marker.element != null && compass_markers != null) {
			compass_markers.Remove(marker.element);
		}

		markers.Remove(marker);
	}

	/// <summary>
	/// Remove all waypoint markers from the compass.
	/// </summary>
	public void clear_markers() {
		foreach (Compass_Marker marker in markers) {
			if (marker.element != null && compass_markers != null) {
				compass_markers.Remove(marker.element);
			}
		}

		markers.Clear();
	}

	private void update_markers() {
		if (compass_markers == null) {
			return;
		}

		// Get current yaw - use editor preview in edit mode
		float current_yaw;
		Transform player;

		#if UNITY_EDITOR
		if (!Application.isPlaying) {
			current_yaw = editor_preview_heading;
			player = player_transform != null ? player_transform : transform;
		} else {
			if (cam_transform == null) {
				return;
			}

			current_yaw = get_camera_yaw();
			player = player_transform != null ? player_transform : cam_transform;
		}
		#else
		if (cam_transform == null) {
			return;
		}

		current_yaw = get_camera_yaw();
		player = player_transform != null ? player_transform : cam_transform;
		#endif

		float half_width = compass_width * 0.5f;
		Vector3 player_pos = player.position;

		foreach (Compass_Marker marker in markers) {
			if (marker.target == null) {
				set_marker_visible(marker, false);
				continue;
			}

			// Calculate angle from player to target
			Vector3 dir_to_target = marker.target.position - player_pos;
			dir_to_target.y = 0f; // Ignore vertical difference

			float target_angle = Mathf.Atan2(dir_to_target.x, dir_to_target.z) * Mathf.Rad2Deg;

			if (target_angle < 0f) {
				target_angle += 360f;
			}

			// Calculate relative angle from current heading
			float relative_angle = Mathf.DeltaAngle(current_yaw, target_angle);

			// Check if marker is within visible range
			bool in_view = Mathf.Abs(relative_angle) <= half_fov;

			#if UNITY_EDITOR
			// In editor, clamp marker to edges instead of hiding
			if (!Application.isPlaying && !in_view) {
				relative_angle = Mathf.Clamp(relative_angle, -half_fov, half_fov);
				in_view = true;
			}
			#endif

			if (!in_view) {
				set_marker_visible(marker, false);
				continue;
			}

			set_marker_visible(marker, true);

			// Position marker on compass
			marker.element.style.left = (relative_angle * pixels_per_degree) + half_width;

			// Update distance only when it changes (rounded to nearest meter)
			int distance = Mathf.RoundToInt(dir_to_target.magnitude);

			if (distance != marker.cached_distance) {
				marker.cached_distance = distance;
				marker.distance_label.text = format_distance(distance);
			}
		}
	}

	private void set_marker_visible(Compass_Marker marker, bool visible) {
		if (marker.is_visible == visible) {
			return;
		}

		marker.is_visible = visible;

		if (visible) {
			marker.element.RemoveFromClassList("compass-marker-hidden");
		} else {
			marker.element.AddToClassList("compass-marker-hidden");
		}
	}

	private string format_distance(int distance) {
		return $"{distance:N0}m";
	}

	#endregion

}
