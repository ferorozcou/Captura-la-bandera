using UnityEngine;


public static class NPCAnimatorHelper // para resetear animaciones de los npcs
{
    public static void ResetToIdle(Animator animator)
    {
        if (animator == null) return;

        // reset de parametros
        foreach (var p in animator.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Bool)
                animator.SetBool(p.name, false);
            else if (p.type == AnimatorControllerParameterType.Float)
                animator.SetFloat(p.name, 0f);
            else if (p.type == AnimatorControllerParameterType.Trigger)
                animator.ResetTrigger(p.name);
        }
    }

    public static void TryPlayIdle(Animator animator)
    {
        if (animator == null) return; // intentar fporzar idle

        string[] names = {
            "root|combat idle",
            "root|combat Idle",
            "root|Idle",
            "Idle",
            "idle",
            "combat idle"
        };

        foreach (string n in names)
        {
            try
            {
                animator.Play(n, 0, 0f);
                return; // éxito
            }
            catch { }
        }
        ResetToIdle(animator);
    }
}