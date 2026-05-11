using UnityEngine;

namespace ActiveSaga.Common.GameSession
{
    public abstract class GameStatsTracker : MonoBehaviour
    {
        public abstract GameType GameType { get; }

        public virtual void ResetStats()
        {
        }

        public abstract GameStatsSnapshot BuildSnapshot();
    }
}