using dr4g0nsoul.WorldBuilder2D.LevelEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorSettingsController
{

    #region Singleton

    private static readonly string s_prefLocation = "LevelEditor/LevelEditorSettings";
    private static LevelEditorSettingsController s_levelEditorSettingsController;
    public static LevelEditorSettingsController Instance
    {
        get
        {
            if (s_levelEditorSettingsController == null)
            {
                s_levelEditorSettingsController = new LevelEditorSettingsController();
            }
            return s_levelEditorSettingsController;
        }
    }

    #endregion

    private LevelEditorSettings levelEditorSettings;

    public LevelEditorSettingsController()
    {
        GetLevelEditorSettings();
    }

    #region General

    public LevelEditorSettings GetLevelEditorSettings()
    {
        if (levelEditorSettings == null)
        {
            levelEditorSettings = Resources.Load<LevelEditorSettings>(s_prefLocation);
        }
        return levelEditorSettings;
    }

    #endregion
}
