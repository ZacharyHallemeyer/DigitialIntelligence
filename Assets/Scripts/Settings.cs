using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class Settings : MonoBehaviour
{
    // Sliders
    public Slider musicSlider;
    public Slider soundEffectsSlider;
    public Slider terminalFSSlider;
    public Slider hubFSSlider;
    public Slider consoleFSSlider;
    public Slider directionFSSlider;
    public Slider codeFSSlider;

    // Hex images
    public Image terminalPlainColorImage;
    public Image terminalCommadColorImage;
    public Image terminalCaretColorImage;
    public Image terminalUnlockedFileColorImage;
    public Image terminalLockedFileColorImage;
    public Image terminalLockedDirectoryColorImage;
    public Image terminalUnlockedDirectoryColorImage;
    public Image codePlainColorImage;
    public Image codeKeywordColorImage;
    public Image codeFunctionColorImage;
    public Image codeStringColorImage;
    public Image codeCaretColorImage;

    // Hex inputs
    public TMP_InputField terminalPlainColorInput;
    public TMP_InputField terminalCommadColorInput;
    public TMP_InputField terminalCaretColorInput;
    public TMP_InputField terminalUnlockedFileColorInput;
    public TMP_InputField terminalLockedFileColorInput;
    public TMP_InputField terminalLockedDirectoryColorInput;
    public TMP_InputField terminalUnlockedDirectoryColorInput;
    public TMP_InputField codePlainColorInput;
    public TMP_InputField codeKeywordColorInput;
    public TMP_InputField codeFunctionColorInput;
    public TMP_InputField codeStringColorInput;
    public TMP_InputField codeCaretColorInput;

    private AudioManager audioManager;


    // Start is called before the first frame update
    void Start()
    {
        audioManager = AudioManager.instance;
        SetSliders();
        SetColors();
    }

    // Sets the sliders to player preferences
    public void SetSliders()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", .75f);
        soundEffectsSlider.value = PlayerPrefs.GetFloat("SoundEffectsVolume", .75f);
        terminalFSSlider.value = PlayerPrefs.GetFloat("TerminalFontSize", 15);
        hubFSSlider.value = PlayerPrefs.GetFloat("HubFontSize", 15);
        consoleFSSlider.value = PlayerPrefs.GetFloat("PuzzleConsoleFontSize", 15);
        directionFSSlider.value = PlayerPrefs.GetFloat("PuzzleDirectionsFontSize", 15);
        codeFSSlider.value = PlayerPrefs.GetFloat("PuzzleCodeFontSize", 15);
    }

    // Sets music volume and preferences to value in music slider
    public virtual void SetMusicVolume()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        if (audioManager == null)
            audioManager = FindObjectOfType<AudioManager>();
        audioManager.SetMusicVolume();
    }

    // Sets sound effects volume and preferences to value in sound effects slider
    public virtual void SetSoundEffectsVolume()
    {
        PlayerPrefs.SetFloat("SoundEffectsVolume", soundEffectsSlider.value);
        if (audioManager == null)
            audioManager = FindObjectOfType<AudioManager>();
        audioManager.SetSoundEffectVolume();
    }

    // Sets sound effects volume and preferences to value in sound effects slider
    public virtual void SetHubFontSize()
    {
        PlayerPrefs.SetFloat("HubFontSize", hubFSSlider.value);
    }

    // Sets sound effects volume and preferences to value in sound effects slider
    public virtual void SetTerminalFontSize()
    {
        PlayerPrefs.SetFloat("TerminalFontSize", terminalFSSlider.value);
    }

    // Sets sound effects volume and preferences to value in sound effects slider
    public virtual void SetConsoleFontSize()
    {
        PlayerPrefs.SetFloat("PuzzleConsoleFontSize", consoleFSSlider.value);
    }

    // Sets sound effects volume and preferences to value in sound effects slider
    public virtual void SetDirectionsFontSize()
    {
        PlayerPrefs.SetFloat("PuzzleDirectionsFontSize", directionFSSlider.value);
    }

    // Sets sound effects volume and preferences to value in sound effects slider
    public virtual void SetCodeFontSize()
    {
        PlayerPrefs.SetFloat("PuzzleCodeFontSize", codeFSSlider.value);
    }

    public virtual void SetColors()
    {
        SetHexColor();
    }

    public virtual void SetHexColor()
    {
        if(IsHexCode(terminalPlainColorInput.text))
        {
            PlayerPrefs.SetString(PlayerPrefNames.TERMINAL_PLAIN_COLOR, terminalPlainColorInput.text);
            terminalPlainColorImage.color = HexToColor(terminalPlainColorInput.text);
        }
        else
        {
            terminalPlainColorImage.color = HexToColor(PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_PLAIN_COLOR));
            terminalPlainColorInput.text = PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_PLAIN_COLOR);
        }

        if (IsHexCode(terminalCommadColorInput.text))
        {
            PlayerPrefs.SetString(PlayerPrefNames.TERMINAL_COMMAND_COLOR, terminalCommadColorInput.text);
            terminalCommadColorImage.color = HexToColor(terminalCommadColorInput.text);
        }
        else
        {
            terminalCommadColorImage.color = HexToColor(PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_COMMAND_COLOR));
            terminalCommadColorInput.text = PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_COMMAND_COLOR);
        }

        if (IsHexCode(terminalCaretColorInput.text))
        {
            PlayerPrefs.SetString(PlayerPrefNames.TERMINAL_CARET_COLOR, terminalCaretColorInput.text);
            terminalCaretColorImage.color = HexToColor(terminalCaretColorInput.text);
        }
        else
        {
            terminalCaretColorImage.color = HexToColor(PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_CARET_COLOR));
            terminalCaretColorInput.text = PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_CARET_COLOR);
        }

        if (IsHexCode(terminalUnlockedFileColorInput.text))
        {
            PlayerPrefs.SetString(PlayerPrefNames.TERMINAL_UNLOCKED_FILE_COLOR, terminalUnlockedFileColorInput.text);
            terminalUnlockedFileColorImage.color = HexToColor(terminalUnlockedFileColorInput.text);
        }
        else
        {
            terminalUnlockedFileColorImage.color = HexToColor(PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_UNLOCKED_FILE_COLOR));
            terminalUnlockedFileColorInput.text = PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_UNLOCKED_FILE_COLOR);
        }

        if (IsHexCode(terminalLockedFileColorInput.text))
        {
            PlayerPrefs.SetString(PlayerPrefNames.TERMINAL_LOCKED_FILE_COLOR, terminalLockedFileColorInput.text);
            terminalLockedFileColorImage.color = HexToColor(terminalLockedFileColorInput.text);
        }
        else
        {
            terminalLockedFileColorImage.color = HexToColor(PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_LOCKED_FILE_COLOR));
            terminalLockedFileColorInput.text = PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_LOCKED_FILE_COLOR);
        }

        if (IsHexCode(terminalLockedDirectoryColorInput.text))
        {
            PlayerPrefs.SetString(PlayerPrefNames.TERMINAL_LOCKED_DIRECTORY_COLOR, terminalLockedDirectoryColorInput.text);
            terminalLockedDirectoryColorImage.color = HexToColor(terminalLockedDirectoryColorInput.text);
        }
        else
        {
            terminalLockedDirectoryColorImage.color = HexToColor(PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_LOCKED_DIRECTORY_COLOR));
            terminalLockedDirectoryColorInput.text = PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_LOCKED_DIRECTORY_COLOR);
        }

        if (IsHexCode(terminalUnlockedDirectoryColorInput.text))
        {
            PlayerPrefs.SetString(PlayerPrefNames.TERMINAL_UNLOCKED_DIRECTORY_COLOR, terminalLockedDirectoryColorInput.text);
            terminalUnlockedDirectoryColorImage.color = HexToColor(terminalUnlockedDirectoryColorInput.text);
        }
        else
        {
            terminalUnlockedDirectoryColorImage.color = HexToColor(PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_UNLOCKED_DIRECTORY_COLOR));
            terminalUnlockedDirectoryColorInput.text = PlayerPrefs.GetString(PlayerPrefNames.TERMINAL_UNLOCKED_DIRECTORY_COLOR);
        }

        if (IsHexCode(codePlainColorInput.text))
        {
            PlayerPrefs.SetString(PlayerPrefNames.CODE_PLAIN_COLOR, codePlainColorInput.text);
            codePlainColorImage.color = HexToColor(codePlainColorInput.text);
        }
        else
        {
            codePlainColorImage.color = HexToColor(PlayerPrefs.GetString(PlayerPrefNames.CODE_PLAIN_COLOR));
            codePlainColorInput.text = PlayerPrefs.GetString(PlayerPrefNames.CODE_PLAIN_COLOR);
        }

        if (IsHexCode(codeKeywordColorInput.text))
        {
            PlayerPrefs.SetString(PlayerPrefNames.CODE_KEYWORD_COLOR, codeKeywordColorInput.text);
            codeKeywordColorImage.color = HexToColor(codeKeywordColorInput.text);
        }
        else
        {
            codeKeywordColorImage.color = HexToColor(PlayerPrefs.GetString(PlayerPrefNames.CODE_KEYWORD_COLOR));
            codeKeywordColorInput.text = PlayerPrefs.GetString(PlayerPrefNames.CODE_KEYWORD_COLOR);
        }

        if (IsHexCode(codeFunctionColorInput.text))
        {
            PlayerPrefs.SetString(PlayerPrefNames.CODE_FUNCTION_COLOR, codeFunctionColorInput.text);
            codeFunctionColorImage.color = HexToColor(codeFunctionColorInput.text);
        }
        else
        {
            codeFunctionColorImage.color = HexToColor(PlayerPrefs.GetString(PlayerPrefNames.CODE_FUNCTION_COLOR));
            codeFunctionColorInput.text = PlayerPrefs.GetString(PlayerPrefNames.CODE_FUNCTION_COLOR);
        }

        if (IsHexCode(codeStringColorInput.text))
        {
            PlayerPrefs.SetString(PlayerPrefNames.CODE_STRING_COLOR, codeStringColorInput.text);
            codeStringColorImage.color = HexToColor(codeStringColorInput.text);
        }
        else
        {
            codeStringColorImage.color = HexToColor(PlayerPrefs.GetString(PlayerPrefNames.CODE_STRING_COLOR));
            codeStringColorInput.text = PlayerPrefs.GetString(PlayerPrefNames.CODE_STRING_COLOR);
        }

        if (IsHexCode(codeCaretColorInput.text))
        {
            PlayerPrefs.SetString(PlayerPrefNames.CODE_CARET_COLOR, codeCaretColorInput.text);
            codeCaretColorImage.color = HexToColor(codeCaretColorInput.text);
        }
        else
        {
            codeCaretColorImage.color = HexToColor(PlayerPrefs.GetString(PlayerPrefNames.CODE_CARET_COLOR));
            codeCaretColorInput.text = PlayerPrefs.GetString(PlayerPrefNames.CODE_CARET_COLOR);
        }

    }

    public static bool IsHexCode(string input)
    {
        // Regular expression pattern for hex code (#RRGGBB or #AARRGGBB)
        string pattern = @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{8})$";

        // Use Regex.IsMatch to check if the input matches the pattern
        return Regex.IsMatch(input, pattern);
    }

    public static Color HexToColor(string hex)
    {
        Color color = Color.black;

        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        else
        {
            Debug.LogError("Invalid hex code: " + hex);
            return Color.black;
        }
    }
}
