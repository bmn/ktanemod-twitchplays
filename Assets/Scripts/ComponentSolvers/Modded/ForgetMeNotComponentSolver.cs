﻿using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class ForgetMeNotComponentSolver : ComponentSolver
{
    public ForgetMeNotComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        _buttons = (Array)_buttonsField.GetValue(bombComponent.GetComponent(_componentType));
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        if (!inputCommand.StartsWith("press ", StringComparison.InvariantCultureIgnoreCase))
        {
            yield break;
        }
        inputCommand = inputCommand.Substring(6);

        int beforeButtonStrikeCount = StrikeCount;

        string[] sequence = inputCommand.Split(new string[] { "" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string buttonString in sequence)
        {
            int val = -1;
            if(int.TryParse(buttonString, out val) && val >= 0 && val <= 9)
            {
                MonoBehaviour button = (MonoBehaviour)_buttons.GetValue(val);

                yield return buttonString;

                if (Canceller.ShouldCancel)
                {
                    Canceller.ResetCancel();
                    yield break;
                }

                DoInteractionStart(button);
                yield return new WaitForSeconds(0.1f);
                DoInteractionEnd(button);

                //Escape the sequence if a part of the given sequence is wrong
                if (StrikeCount != beforeButtonStrikeCount)
                {
                    yield break;
                }
            }
        }
    }

    static ForgetMeNotComponentSolver()
    {
        _componentType = ReflectionHelper.FindType("AdvancedMemory");
        _buttonsField = _componentType.GetField("Buttons", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private static Type _componentType = null;
    private static FieldInfo _buttonsField = null;

    private Array _buttons = null;
}
