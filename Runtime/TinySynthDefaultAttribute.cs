using System;

namespace SOSXR.TinySynth
{
    /// <summary>
    ///     Marks a <see cref="TinySynthSound" /> field with its default value, used by the
    ///     editor to reset individual parameters via the 'R' button.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TinySynthDefaultAttribute : Attribute
    {
        /// <summary>The default value for the decorated field, in the same unit as the field itself.</summary>
        public readonly float value;

        /// <summary>Initialises the attribute with the given default value.</summary>
        /// <param name="value">Default value for the decorated field.</param>
        public TinySynthDefaultAttribute(float value)
        {
            this.value = value;
        }
    }
}

