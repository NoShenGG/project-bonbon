﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Representation of an active Effect in an Actor;
/// </summary>
[CreateAssetMenu(menuName = "Effects/Effect")]
public class EffectBlueprint : ScriptableObject {

    public new string name;

    /// <summary> Duration of the effect in turns; </summary>
    public int duration;
    /// <summary> Modifiers applied while the effect is active; </summary>
    public PassiveModifier modifiers;
    /// <summary> Actions carried at the beginning of every turn; </summary>
    public List<ImmediateAction.EffectOnly> actions;
    public enum EffectType { Buff, Debuff }
    public EffectType effectType;

    /// <summary>
    /// Compute action values for each element of the effect;
    /// </summary>
    /// <param name="actionValue"> AI Value bundle passed down for data collection; </param>
    public void ComputeEffectValue(ref AIActionValue actionValue, StatIteration casterData) {
        AIActionValue compoundValue = new AIActionValue();
        foreach (ImmediateAction.EffectOnly action in actions) {
            for (int i = 0; i < duration; i++) action.ComputeActionValue(ref compoundValue, casterData, i);
        }
    }

    public Effect InstantiateEffect(StatIteration originStats) {
        return new Effect(this, originStats);
    }

    #if UNITY_EDITOR

    public static GUIContent GUIContent(object effectBlueprint) {
        EffectBlueprint bp = effectBlueprint as EffectBlueprint;
        return new GUIContent(bp.name);
    }

    #endif
}