using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Reference
{
    private const string PlayerTag = "Player";
    public static GameObject Player()
    {
        return GameObject.FindWithTag(PlayerTag);
    }
}
