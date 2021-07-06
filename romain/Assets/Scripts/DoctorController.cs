using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoctorController : PlaybleCharacter
{
    public override void Action()
    {
        base.Action();

        // chack if a citizen can be vaccinated and look at him
        foreach (CitizenController citizen in GameManager.citizenPool)
        {
            if (Vector3.Distance(transform.position, citizen.transform.position) < actionRange)
            {
                Vector3 targetDir = (citizen.transform.position - transform.position).normalized;
                targetDir.y = transform.position.y;
                transform.rotation = Quaternion.LookRotation(targetDir);
                citizen.SetState(CitizenState.Vaccinated);
                GameManager.instance.PlaySound(0, transform.position);
            }
        }
    }
}
