using ActiveSaga.Common.GameSession;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class RunGameLaunchReceiver : MonoBehaviour
{
    [Header("Run Scene References")]
    [SerializeField] private ContentDirector contentDirector;
    [SerializeField] private TileManager tileManager;

    [Header("Debug")]
    [SerializeField] private bool logLaunchData = true;

    private void Awake()
    {
        SelectedGameMode selectedMode = GameLaunchData.GameMode;
        SelectedGameDifficulty selectedDifficulty = GameLaunchData.Difficulty;

        if (logLaunchData)
        {
            Debug.Log(
                "RunGame received launch data. Mode: " + selectedMode +
                ", Difficulty: " + selectedDifficulty
            );
        }

        if (selectedMode != SelectedGameMode.Run)
        {
            Debug.LogWarning(
                "RunGame scene opened, but selected mode is not Run. Mode received: " + selectedMode
            );
        }

        GameDifficulty runDifficulty = ConvertDifficulty(selectedDifficulty);
        BiomeType biome = GetBiomeForDifficulty(runDifficulty);

        ApplyToContentDirector(runDifficulty, biome);
        ApplyToTileManager(biome);

        if (logLaunchData)
        {
            Debug.Log(
                "RunGame configured. Difficulty: " + runDifficulty +
                ", Biome: " + biome
            );
        }
    }

    private GameDifficulty ConvertDifficulty(SelectedGameDifficulty selectedDifficulty)
    {
        switch (selectedDifficulty)
        {
            case SelectedGameDifficulty.Easy:
                return GameDifficulty.Easy;

            case SelectedGameDifficulty.Medium:
                return GameDifficulty.Medium;

            case SelectedGameDifficulty.Hard:
                return GameDifficulty.Hard;

            default:
                Debug.LogWarning("No valid difficulty received. Falling back to Easy.");
                return GameDifficulty.Easy;
        }
    }

    private BiomeType GetBiomeForDifficulty(GameDifficulty difficulty)
    {
        switch (difficulty)
        {
            case GameDifficulty.Easy:
                return BiomeType.Forest;

            case GameDifficulty.Medium:
                return BiomeType.City;

            case GameDifficulty.Hard:
                return BiomeType.Space;

            default:
                return BiomeType.Forest;
        }
    }

    private void ApplyToContentDirector(GameDifficulty difficulty, BiomeType biome)
    {
        if (contentDirector == null)
        {
            Debug.LogError("RunGameLaunchReceiver: Missing ContentDirector reference.");
            return;
        }

        contentDirector.Configure(difficulty, biome);
    }

    private void ApplyToTileManager(BiomeType biome)
    {
        if (tileManager == null)
        {
            Debug.LogError("RunGameLaunchReceiver: Missing TileManager reference.");
            return;
        }

        tileManager.SetBiome(biome);
    }
}