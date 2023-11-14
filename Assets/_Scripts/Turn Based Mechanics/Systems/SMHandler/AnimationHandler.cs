using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationHandler : MonoBehaviour {

    [SerializeField] private BattleStateMachine battleStateMachine;
    [SerializeField] private SkillAnimationMap skillAnimationMap;

    #region Events
    public event Action<int, Actor> DamageEvent;
    public event Action<int, Actor> HealEvent;
    public event Action<EffectBlueprint, Actor> EffectEvent;
    #endregion Events
    public Dictionary<SkillObject, Dictionary<ActorData, SkillAnimation>> SkillAMap { get; private set; }

    public event System.Action OnSkillHit;

    void Awake() {
        SkillAMap = SKAEUtils.ProcessInternalDictionary(skillAnimationMap.animationMap);
    }

    void Start() {
        battleStateMachine.CurrInput.OnSkillAnimation += OnSkillTrigger;
    }

    public void OnSkillTrigger(ActiveSkillPrep skillPrep) {
        SkillAction skillAction = skillPrep.skill;
        BonbonObject bonbon = skillPrep.bonbon;
        AIActionValue[] avs = skillPrep.targets.Select(target => skillAction.ComputeSkillActionValues(target)).ToArray();
        try {
            SkillAnimation sa = SkillAMap[skillAction.SkillData][skillAction.Caster.Data];
            skillAction.Caster.GetComponentInChildren<Animator>(true).SetTrigger(sa.AnimationTrigger);
            battleStateMachine.StartBattle(sa.AnimationDuration);
            StartCoroutine(HitDelay(sa.HitDelay));
            if (bonbon != null) ; /// Do VFXs

            foreach (DelaySkillAnimation delaySkillAnimation in sa.DelaySkills) {
                foreach (IEnumerator ie in delaySkillAnimation.GetCoroutines(this, avs, skillPrep.targets)) {
                    StartCoroutine(ie);
                }
            }
        } catch (KeyNotFoundException) {
            Debug.LogWarning($"Animation Undefined for {skillAction.SkillData.Name} -> {skillAction.Caster.Data.DisplayName}");
        }
    }

    private IEnumerator HitDelay(float delay) {
        yield return new WaitForSeconds(delay);
        OnSkillHit?.Invoke();
    }

    #region Events
    public void TriggerDamage(int damage, Actor actor) {
        DamageEvent.Invoke(damage, actor);
    }

    public void TriggerHeal(int heal, Actor actor) {
        HealEvent.Invoke(heal, actor);
    }

    public void TriggerEffect(EffectBlueprint effect, Actor actor) {
        EffectEvent.Invoke(effect, actor);
    }
    #endregion Events
}
