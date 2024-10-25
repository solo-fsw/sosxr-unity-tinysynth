using UnityEngine;
using SOSXR.TinySynth;


public class TinySynthDemo : MonoBehaviour
{
    public TinySynthSound Sound;


    [ContextMenu(nameof(PlaySound))]
    private void PlaySound()
    {
        TinySynthPlayer.Play(Sound);
    }
}