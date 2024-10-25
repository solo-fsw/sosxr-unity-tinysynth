using System;


namespace SOSXR.TinySynth
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TinySynthDefaultAttribute : Attribute
    {
        public readonly float value;


        public TinySynthDefaultAttribute(float value)
        {
            this.value = value;
        }
    }
}