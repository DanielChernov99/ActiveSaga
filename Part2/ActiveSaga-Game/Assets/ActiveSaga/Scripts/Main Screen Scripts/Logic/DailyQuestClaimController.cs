using UnityEngine;

namespace ActiveSaga.MainScreen.Logic
{
    public class DailyQuestClaimController : MonoBehaviour
    {
        public void ClaimQuest(int questIndex)
        {
            Debug.Log(
                "Claim clicked for quest index " +
                questIndex +
                ". Currently quest rewards are handled automatically by the server when a game session is completed."
            );
        }
    }
}