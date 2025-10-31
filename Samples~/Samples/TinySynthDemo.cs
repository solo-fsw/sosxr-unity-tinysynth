using SOSXR.TinySynth;
using UnityEngine;


public class TinySynthDemo : MonoBehaviour
{
    public TinySynthSound Sound;


    [ContextMenu(nameof(PlaySound))]
    private void PlaySound()
    {
        TinySynthPlayer.Play(Sound);
    }
}