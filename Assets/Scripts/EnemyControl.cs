using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControl : MonoBehaviour
{
	public GameObject player;
	public NavMeshAgent Agent;

	private void Start()
	{
		player = GameObject.Find("body");
		Agent = GetComponent<NavMeshAgent>();
	}

	private void Update()
	{
		Agent.SetDestination(player.transform.position);
	}
}