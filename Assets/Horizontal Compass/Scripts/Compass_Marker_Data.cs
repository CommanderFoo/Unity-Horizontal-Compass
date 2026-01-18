using System;
using UnityEngine;

/// <summary>
/// Serializable data for configuring compass markers in the editor.
/// </summary>
[Serializable]
public class Compass_Marker_Data {

	[Tooltip("The world-space transform this marker tracks.")]
	public Transform target;

	[Tooltip("Optional icon to display. Leave empty for default circle.")]
	public Texture2D icon;

	[Tooltip("Color tint for the marker icon.")]
	public Color color = Color.white;

}
