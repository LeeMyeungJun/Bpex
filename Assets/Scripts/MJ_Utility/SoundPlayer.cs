// Copyright (C) 2018-2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;


public static class SoundPlayer
{
	private static SoundSystem soundSystem;
	private static BackgroundMusic bgmSystem;

	public static void Initialize()
	{
		if (SoundSystem.Inst == null)
			SoundSystem.Load();
		if (BackgroundMusic.Inst == null)
			BackgroundMusic.Load();

	}

	public static SoundFx PlaySoundFx(string soundName, bool loop = false)
	{
		if (SoundSystem.Inst == null)
		{
			Initialize();
			if (SoundSystem.Inst == null)
				return null;
		}
		return SoundSystem.Inst.PlaySoundFx(soundName, loop);
	}

	public static SoundFx PlaySoundFx(string soundName, Vector3 pos, bool loop = false)
	{
		if (SoundSystem.Inst == null)
		{
			Initialize();
			if (SoundSystem.Inst == null)
				return null;
		}
		return SoundSystem.Inst.PlaySoundFx(soundName, pos, loop);
	}

	public static SoundFx PlaySoundFx(string soundName, Transform parent, bool loop = false)
	{
		if (SoundSystem.Inst == null)
		{
			Initialize();
			if (SoundSystem.Inst == null)
				return null;
		}
		return SoundSystem.Inst.PlaySoundFx(soundName, parent, loop);
	}

	public static void SetSoundEnabled(bool soundEnabled)
	{
		if (SoundSystem.Inst == null)
			return;
		SoundSystem.Inst.SetSoundEnabled(soundEnabled);
	}

	public static void SetMusicEnabled(bool musicEnabled)
	{
		if (SoundSystem.Inst == null)
			return;
		SoundSystem.Inst.SetMusicEnabled(musicEnabled);
	}
}
