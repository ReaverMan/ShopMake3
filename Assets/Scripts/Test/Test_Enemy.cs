using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_Enemy : TestBase
{
    public EnemyBace enemy;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        enemy.Demage(50);
    }
}
