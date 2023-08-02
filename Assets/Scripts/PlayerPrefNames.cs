using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefNames : MonoBehaviour
{
    public static readonly string MUSIC_VOLUME = "MusicVolume";
    public static readonly string SOUND_EFFECT_VOLUME = "SoundEffectsVolume";
    
    public static readonly string TERMINAL_FONT_SIZE = "TerminalFontSize";
    public static readonly string HUB_FONT_SIZE = "HubFontSize";
    public static readonly string DIRECTIONS_FONT_SIZE = "PuzzleDirectionsFontSize";
    public static readonly string CONSOLE_FONT_SIZE = "PuzzleConsoleFontSize";
    public static readonly string CODE_FONT_SIZE = "PuzzleCodeFontSize";

    public static readonly string TERMINAL_PLAIN_COLOR = "terminalPlainColor";
    public static readonly string TERMINAL_COMMAND_COLOR = "terminalCommandColor";
    public static readonly string TERMINAL_CARET_COLOR = "terminalCaretColor";
    public static readonly string TERMINAL_UNLOCKED_FILE_COLOR = "terminalUnlockedFileColor";
    public static readonly string TERMINAL_LOCKED_FILE_COLOR = "terminalLockedFileColor";
    public static readonly string TERMINAL_UNLOCKED_DIRECTORY_COLOR = "terminalUnlockedDirectoryColor";
    public static readonly string TERMINAL_LOCKED_DIRECTORY_COLOR = "terminalLockedDirectoryColor";
    public static readonly string CODE_PLAIN_COLOR = "codePlainColor";
    public static readonly string CODE_KEYWORD_COLOR = "codeKeywordColor";
    public static readonly string CODE_FUNCTION_COLOR = "codeFunctionColor";
    public static readonly string CODE_STRING_COLOR = "codeStringColor";
    public static readonly string CODE_CARET_COLOR = "codeCaretColor";

    public enum PYTHON_COLORS
    {
        KEYWORD,
        FUNCTION
    }
}
