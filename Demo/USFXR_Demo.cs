using UnityEngine;
using usfxr;


public class USFXR_Demo : MonoBehaviour
{
    [SerializeField] private TinySynthSound m_sound;


    [ContextMenu(nameof(PlaySound))]
    private void PlaySound()
    {
        TinySynthPlayer.Play(m_sound);
    }
}