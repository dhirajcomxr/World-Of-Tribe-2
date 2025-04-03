using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class swordSlashes
{
    public ParticleSystem swordSlashe;
    public Collider slashCollider;
}

[System.Serializable]
public class Buffs
{
    public ParticleSystem powerUpEffect;
    public ParticleSystem powerEffect;
}

public class VFXManager : Singleton<VFXManager>
{
    public swordSlashes[] slashes;
    public ParticleSystem[] impactHit;
    public Buffs[] powerUpEffects;

    public Transform slashSpwanPos;

    public float slashColliderDisableDelayTime = 0.05f;

    public void SpwanSwordVFX(int slashNumber)
    {
        slashes[slashNumber].swordSlashe.Play();
        slashes[slashNumber].slashCollider.enabled = true;
        LeanTween.delayedCall(slashColliderDisableDelayTime, () => slashes[slashNumber].slashCollider.enabled = false);
    }

    public void SpwanHitEffect(int num)
    {
        impactHit[num].Play();
    }

    private int powerEffectCount = 0;
    public void UltimateEffect(int num)
    {
        powerEffectCount = num;
        
        LeanTween.delayedCall(.1f, () => powerUpEffects[num].powerEffect.Play());
    }

    public void StartSkillEfect()
    {
        powerUpEffects[1].powerEffect.Play();
        CombatManager.Instance.rotationCollider.enabled = true;
    }
    //Stop Ultimate VFX and Animation if duration is added
    public void StopUltimateVFX()
    {
        CombatManager.Instance.rotationCollider.enabled = false;
        powerUpEffects[powerEffectCount].powerEffect.Stop();
        powerUpEffects[1].powerEffect.Stop();
    }

    public void SpwanTimerAttackVFX()
    {
        Transform bigSlash = Instantiate(powerUpEffects[2].powerUpEffect.gameObject, slashSpwanPos.position, transform.rotation).transform;            
        Destroy(bigSlash.gameObject, 3.5f);
    }
}
