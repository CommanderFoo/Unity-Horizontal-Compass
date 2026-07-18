using UnityEngine;

namespace net.pixeldepth.horizontal_compass.examples {

	/// <summary>
	/// Example component that subscribes to Compass_Controller marker events and logs them.
	/// Attach it to a GameObject, assign a Compass_Controller, enter Play mode, and watch the
	/// Console as markers enter/leave the compass and come within range.
	///
	/// Note: the marker events fire in Play mode only. For the within-range event, set the
	/// compass's Marker Range Threshold above 0.
	/// </summary>
	public class Compass_Event_Demo : MonoBehaviour {

		[SerializeField, Tooltip("The compass to listen to. Uses one on this GameObject if left empty.")]
		private Compass_Controller compass;

		private void OnEnable() {
			if (compass == null) {
				compass = GetComponent<Compass_Controller>();
			}

			if (compass == null) {
				Debug.LogWarning("Compass_Event_Demo: no Compass_Controller assigned.");

				return;
			}

			compass.On_Marker_Entered_View += handle_marker_entered_view;
			compass.On_Marker_Exited_View += handle_marker_exited_view;
			compass.On_Marker_Within_Range += handle_marker_within_range;
		}

		private void OnDisable() {
			if (compass == null) {
				return;
			}

			compass.On_Marker_Entered_View -= handle_marker_entered_view;
			compass.On_Marker_Exited_View -= handle_marker_exited_view;
			compass.On_Marker_Within_Range -= handle_marker_within_range;
		}

		private void handle_marker_entered_view(Compass_Marker marker) {
			Debug.Log(string.Format("[Compass] Marker entered view ({0}).", describe(marker)));
		}

		private void handle_marker_exited_view(Compass_Marker marker) {
			Debug.Log(string.Format("[Compass] Marker left view ({0}).", describe(marker)));
		}

		private void handle_marker_within_range(Compass_Marker marker) {
			Debug.Log(string.Format("[Compass] Marker within range ({0}).", describe(marker)));
		}

		private string describe(Compass_Marker marker) {
			if (marker.use_world_position) {
				return string.Format("world position {0}, {1}m away", marker.world_position, marker.cached_distance);
			}

			string target_name = marker.target != null ? marker.target.name : "unknown";

			return string.Format("target '{0}', {1}m away", target_name, marker.cached_distance);
		}

	}

}
