using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusController : PlaybleCharacter
{
    public override void Action()
    {
        base.Action();

        // chack is citizen can be infected and look at him
        foreach (CitizenController citizen in GameManager.citizenPool)
        {
            if (Vector3.Distance(transform.position, citizen.transform.position) < actionRange && citizen.state != CitizenState.Vaccinated)
            {
                Vector3 targetDir = (citizen.transform.position - transform.position).normalized;
                targetDir.y = transform.position.y;
                transform.rotation = Quaternion.LookRotation(targetDir);
                citizen.SetState(CitizenState.Infected);
                GameManager.instance.PlaySound(1, transform.position);
            }
        }
    }
}
