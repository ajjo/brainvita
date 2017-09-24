using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ball : MonoBehaviour {
		
	public int _x;
	public int _y;
	public int _index;
	public bool _empty;
	public bool _invalid;
	public Transform _t;
	public int _w;
	public List<Vector3> _moves;

	private MeshRenderer mesh;

	void Awake() {
		mesh = transform.GetComponent<MeshRenderer>();
	}

	public void appear() {
		mesh.enabled = true;
		_empty = false;
	}

	public void disappear(bool invalid = false) {
		mesh.enabled = false;
		_empty = true;
		_invalid = invalid;
	}

	public bool active() {
		return mesh.enabled;
	}
}
