﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTrail : MonoBehaviour
{
    [SerializeField] private Transform anchors;
    [SerializeField] private float moveSpeed = 1;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool stopOnReachEnd;

    Vector3[] global_reachmarks;

    int
        current_aim = 0,
        aim_modifier = 1,
        marks_quantity;

    private void Start()
    {
        if ((marks_quantity = anchors.childCount) == 0)
            this.enabled = false;

        global_reachmarks = new Vector3[marks_quantity + 1];
        global_reachmarks[0] = transform.position;
        for (int i = 0; i < marks_quantity; i++)
        {
            global_reachmarks[i + 1] = anchors.GetChild(i).position;
            anchors.GetChild(i).position = transform.position;
        }
        marks_quantity++;

        LineRenderer lineRenderer = anchors.GetComponent<LineRenderer>();
        if(lineRenderer) 
        { 
            lineRenderer.positionCount = global_reachmarks.Length;
            lineRenderer.SetPositions(global_reachmarks); 
            lineRenderer.loop = loop;
        }
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, global_reachmarks[current_aim], moveSpeed / 50);
        if (transform.position == global_reachmarks[current_aim])
        {
            if (loop)
            {
                current_aim = (current_aim + 1) % marks_quantity;
            }
            else
            {
                current_aim += aim_modifier;

                if (current_aim + 1 > marks_quantity || current_aim < 0)
                {
                    if(!stopOnReachEnd)
                    {
                        aim_modifier *= -1;
                        current_aim += aim_modifier;
                    }
                    else enabled = false; 
                }
            }
        }
    }
}
