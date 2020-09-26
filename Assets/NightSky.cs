using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightSky : MonoBehaviour
{
	protected float time = 0;
	// Start is called before the first frame update
	void Start()
	{
	}

    // Update is called once per frame
	void Update()
	{
		time += Time.deltaTime;
		transform.rotation = Quaternion.AngleAxis(time, Vector3.up);//Quaternion.Euler(time, time, time);
	}
}
