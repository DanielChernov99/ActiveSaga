using UnityEngine;
using UnityEngine.SceneManagement;
using ActiveSaga.Common.GameSession;
using ActiveSaga.Common.UI;

namespace ActiveSaga.FightGame
{
    public class FightGameResultsUI : EndGameResultsViewBase
    {
        [Header("Finish Game")]
        [SerializeField] private string mainSceneName = "Main New";

        private bool finishGameClicked;

        public void FinishGame()
        {
            if (finishGameClicked)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(mainSceneName))
            {
                Debug.LogError("FightGameResultsUI: Main scene name is empty.");
                return;
            }

            finishGameClicked = true;

            Time.timeScale = 1f;
            SceneManager.LoadScene(mainSceneName);
        }

        protected override void BuildGameSpecificStats(
            GameStatsSnapshot statsSnapshot,
            out ResultStat stat1,
            out ResultStat stat2,
            out ResultStat stat3
        )
        {
            FightGameStatsSnapshot fightStats = statsSnapshot as FightGameStatsSnapshot;

            if (fightStats == null)
            {
                stat1 = new ResultStat("WAVES COMPLETED", "-");
                stat2 = new ResultStat("ENEMIES KILLED", "-");
                stat3 = new ResultStat("SUCCESSFUL DODGES", "-");
                return;
            }

            stat1 = new ResultStat("WAVES COMPLETED", fightStats.wavesCompleted.ToString());
            stat2 = new ResultStat("ENEMIES KILLED", fightStats.enemiesKilled.ToString());
            stat3 = new ResultStat("SUCCESSFUL DODGES", fightStats.successfulDodges.ToString());
        }
    }
}