# usfxr

This project is a reimagining of [usfxr](https://github.com/zeh/usfxr) tailored for my own prototyping needs. 

What's it do
------------

usfxr lets you quickly generate placeholder (or permanent, I don't judge) sound effects right inside the Unity editor. 


Installation
------------
 Add [this repository](https://github.com/grapefrukt/usfxr.git) as a package in the [Unity Package Manager](https://docs.unity3d.com/2019.3/Documentation/Manual/upm-ui-giturl.html). 
 
 Usage
 ------------
 Add a `SfxrPlayer` to an object in your scene, the main camera is a good spot to put it. 
 
 Now, in any MonoBehaviour you wish to play a sound effect, add a public `SfxrParams` field:
    
	public SfxrParams sfxJump;

This will now get a nice property drawer in the editor where you can tweak its properties or apply any of the provided presets. 

To play this sound effect, call the static function on `SfxrPlayer`:

    SfxrPlayer.Play(sfxJump);
    
That's it!

 Issues
 ------------
 
 * Sounds are not cached until they are played, this causes latency spikes
 * Playback is monophonic, only one sample at a time 
 * No support for import/export as strings
 * Undo/redo history
 * Mutations do not work yet
 * Editor UI implementation is rather bad