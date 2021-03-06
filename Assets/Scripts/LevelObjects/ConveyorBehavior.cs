﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ConveyorBehavior : MonoBehaviour
{
	public List<HingeJoint2D> conveyorMotors;
	public float ConveyorSpeed;
	public bool running;

	public Transform start;
	public Transform end;
	public float axlesPerUnit;
	public float linksPerUnit;
	public float linkDistance;

	public float axleRadius;

	public GameObject axlePrefab;
	public GameObject beltLinkPrefab;

	public GameObject axleContainer;
	public GameObject beltContainer;
	public GameObject guideContainer;

	public string guideLayer;

	private float oldSpeed;

	void Start()
	{
		if (running)
		{
			StartBelt();
		}
		else
		{
			StopBelt();
		}
	}

	void Update()
	{
		if (running && oldSpeed != ConveyorSpeed)
		{
			conveyorMotors.ForEach(m => {
				JointMotor2D motor = m.motor;
				motor.motorSpeed = ConveyorSpeed;
				m.motor = motor;
			});
			oldSpeed = ConveyorSpeed;
		}

		if (!running)
		{
			StopBelt();
		}
	}

	[InputSocket]
	public void StopBelt()
	{
		running = false;
		conveyorMotors.ForEach(m => {
			JointMotor2D motor = m.motor;
			motor.motorSpeed = 0;
			m.motor = motor;
		});
		oldSpeed = 0;

		audio.Stop();
	}

	[InputSocket]
	public void StartBelt()
	{
		running = true;
		oldSpeed = ConveyorSpeed;
		conveyorMotors.ForEach(m => {
			JointMotor2D motor = m.motor;
			motor.motorSpeed = ConveyorSpeed;
			m.motor = motor;
		});

		audio.Play();
	}
	
	[InputSocket]
	public void SetSpeed(string speed)
	{
		ConveyorSpeed = float.Parse(speed);
	}
}
