using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    private void FixedUpdate()
    {
        transform.position = Player.Instance.transform.position;
    }
}
