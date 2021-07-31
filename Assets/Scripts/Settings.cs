using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum InputType
{
    Keyboard,
    Gamepad,
}

public class Settings
{
    static Settings m_instance = null;
    public static Settings instance
    {
        get
        {
            if (m_instance == null)
                m_instance = new Settings();
            return m_instance;
        }
    }

    [Serializable]
    class SettingsDatas
    {
        public float m_musicVolume = 0.5f;
        public float m_soundVolume = 0.5f;
        public InputType m_inputType = InputType.Keyboard;
    }

    SettingsDatas m_settings = new SettingsDatas();

    Settings()
    {
        Load();
    }

    void Load()
    {
        if (PlayerPrefs.HasKey("Settings"))
        {
            var str = PlayerPrefs.GetString("Settings");
            if (str != null && str.Length > 0)
                m_settings = JsonUtility.FromJson<SettingsDatas>(str);
        }
    }

    void Save()
    {
        var str = JsonUtility.ToJson(m_settings);
        PlayerPrefs.SetString("Settings", str);

        Event<SettingsChangedEvent>.Broadcast(new SettingsChangedEvent());
    }

    public float musicVolume
    {
        get { return m_settings.m_musicVolume; }
        set
        {
            m_settings.m_musicVolume = value;
            Save();
        }
    }

    public float soundVolume
    {
        get { return m_settings.m_soundVolume; }
        set
        {
            m_settings.m_soundVolume = value;
            Save();
        }
    }

    public InputType inputType
    {
        get { return m_settings.m_inputType; }
        set
        {
            m_settings.m_inputType = value;
            Save();
        }
    }
}