using Core;
using UnityEngine;

public class SettingsLocalData : ILocalData
{
    public bool isSoundOn;

    public bool isSoundEffectOn;

    public int MusicPlayingIndex;

    public SettingsLocalData()
    {
        isSoundOn = true;
        isSoundEffectOn = true;
        MusicPlayingIndex = 0;
    }

    public void InitAfterLoadData()
    {
    }
}
