/*******************************************************************************
 *
 *  File Name: AudioManager.cs
 *
 *  Description: Deals with all the audio in the game
 *
 *******************************************************************************/
using UnityEngine;

namespace GSP
{
    /*******************************************************************************
     *
     * Name: AudioManager
     * 
     * Description: Manages all the game's audio effects and music.
     * 
     *******************************************************************************/
    public class AudioManager : MonoBehaviour
	{
        public AudioSource sfxSource;			// Sound effects player
        public AudioSource musicSource;			// Background music player
        public AudioClip menuMusic;				// Main Menu music
        public AudioClip desertMusic;			// Desert map music
        public AudioClip snowMusic;				// Snow map music
        public AudioClip euroMusic;				// European map music
        public AudioClip metroMusic;			// Metropolis map music
        public AudioClip victoryMusic;			// Victory music
        public AudioClip drawMusic;				// Tie/Draw music
        public AudioClip lossMusic;				// Loss music
        public AudioClip coinSFX;				// Coin Jangling SFX
        public AudioClip walkingSFX;			// Walking noise SFX
        public AudioClip swordHit1SFX;			// One variation of a sword clash
        public AudioClip swordHit2SFX;			// Second variation of a sword clash
        public AudioClip swordHit3SFX;			// Third variation of a sword clash
        public AudioClip diceSFX;				// Rolling dice SFX
        public AudioClip fishingSFX;			// Fish catch SFX
        public AudioClip miningSFX;				// Ore mining SFX
        public AudioClip shearingSFX;			// Wool shearing SFX
        public AudioClip woodSFX;				// Woodcutting SFX
        public AudioClip explosionSFX;			// Cake chart explosion
		static AudioManager instance = null;    // Singleton reference
        public float lowPitchRange = 0.95f;		// Low end of change spectrum
        public float highPitchRange = 1.05f;	// High end of change spectrum
		
		// Use this for initialization
		void Awake() 
		{
			// Set instance if one does not exist
			if(instance == null)
			{
				instance = this;

				// Play menu music
				PlayMenu();
			} // end if
			
			// If one does exist, and it does not equal this, destroy this new object
			else if (instance != this)
			{
				Destroy(gameObject);
			} // end else if
			
			// Protect AudioManager from destruction each restart
            DontDestroyOnLoad(gameObject);
		} // end Awake

		// Audio sound functions
		public void MuteMusic()
		{
			musicSource.mute = !musicSource.mute; 
		} //end MuteMusic

		public bool IsMusicMuted()
		{
			return musicSource.mute;
		} //end IsMusicMuted

		public void MuteSFX()
		{
			sfxSource.mute = !sfxSource.mute;
		} //end MuteSFX

		public bool IsSFXMuted()
		{
			return sfxSource.mute;
		} //end IsMusicMuted

		public void MusicVolume(float vol)
		{
			musicSource.volume = vol;
		} //end MusicVolume

		public float MusicLevel()
		{
			return musicSource.volume;
		} //end MusicLevel

		public void SFXVolume(float vol)
		{
			sfxSource.volume = vol;
		} //end SFXVolume

		public float SFXLevel()
		{
			return sfxSource.volume;
		} //end SFXLevel

        // SFX clip functions
		// Sets a single passed clip to a source clip and plays it
		public void PlaySingle(AudioClip clip)
		{
            sfxSource.pitch = Random.Range(lowPitchRange, highPitchRange);
			sfxSource.clip = clip;
            sfxSource.Play();
		} // end PlaySingle
		
		// Picks one clip out of the parameters and plays it as the source clip
		public void RandomizeSFX(params AudioClip [] clips)
		{
			// Select random clip
            int randomIndex = Random.Range(0, clips.Length);
			
			// Select random pitch so sounds don't get old
            float randomPitch = Random.Range(lowPitchRange, highPitchRange);
			
			// Apply and play clip
			sfxSource.pitch = randomPitch;
            sfxSource.clip = clips[randomIndex];
            sfxSource.Play();
		} // end RandomizeSFX

		// Background music functions
		public void PlayMenu()
		{
			musicSource.Stop();
			musicSource.clip = menuMusic;
            musicSource.PlayDelayed(1.0f);
		} // end PlayMenu

		public void PlayDesert()
		{
            musicSource.Stop();
			musicSource.clip = desertMusic;
            musicSource.PlayDelayed(1.0f);
		} // end PlayDesert

		public void PlaySnow()
		{
            musicSource.Stop();
			musicSource.clip = snowMusic;
            musicSource.PlayDelayed(1.0f);
		} // end PlaySnow

		public void PlayEuro()
		{
            musicSource.Stop();
			musicSource.clip = euroMusic;
            musicSource.PlayDelayed(1.0f);
		} // end PlayEuro

		public void PlayMetro()
		{
            musicSource.Stop();
			musicSource.clip = metroMusic;
            musicSource.PlayDelayed(1.0f);
		} // end PlayMetro

		public void PlayVictory()
		{
            musicSource.Stop();
			musicSource.clip = victoryMusic;
            musicSource.PlayDelayed(1.0f);
		} // end PlayDesert

		public void PlayDraw()
		{
            musicSource.Stop();
            musicSource.clip = drawMusic;
            musicSource.PlayDelayed(1.0f);
		} // end PlayDraw

		public void PlayLoss()
		{
            musicSource.Stop();
			musicSource.clip = lossMusic;
            musicSource.PlayDelayed(1.0f);
		} // end PlayLoss

        public void PlayCoin()
        {
            PlaySingle(coinSFX);
        } // end PlayCoin

        public void PlayWalk()
        {
            PlaySingle(walkingSFX);
        } // end PlayWalk

        public void PlaySword()
        {
            RandomizeSFX(swordHit1SFX, swordHit2SFX, swordHit3SFX);
        } // end PlaySword

        public void PlayDie()
        {
            PlaySingle(diceSFX);
        } // end PlayDie

        public void PlayFish()
        {
            PlaySingle(fishingSFX);
        } // end PlayFish

        public void PlayMine()
        {
            PlaySingle(miningSFX);
        } // end PlayMine

        public void PlayShear()
        {
            PlaySingle(shearingSFX);
        } // end PlayShear

        public void PlayWood()
        {
            PlaySingle(woodSFX);
        } // end PlayWood

        public void PlayExplosion()
        {
            PlaySingle(explosionSFX);
        } // end PlayExplosion

        // Gets the AudioManger instance reference
        public static AudioManager Instance
        {
            get { return instance; }
        } // end Instance
	} // end AudioManager
} // end GSP