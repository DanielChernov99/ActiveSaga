using System;
using ActiveSaga.MainScreen.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ActiveSaga.MainScreen.UI
{
    public class DailyQuestsScreenUI : MonoBehaviour
    {
        [Serializable]
        public class QuestSlot
        {
            public GameObject root;

            [Header("Texts")]
            public TMP_Text descriptionText;
            public TMP_Text rewardText;
            public TMP_Text completedText;

            [Header("Medal")]
            public Image medalImage;
            public Sprite incompleteMedalSprite;
            public Sprite completedMedalSprite;
        }

        [Header("Data")]
        [SerializeField] private DashboardDataManager dashboardDataManager;

        [Header("Quest Slots")]
        [SerializeField] private QuestSlot[] questSlots = new QuestSlot[3];

        private void OnEnable()
        {
            if (dashboardDataManager == null)
            {
                dashboardDataManager = DashboardDataManager.Instance;
            }

            if (dashboardDataManager == null)
            {
                return;
            }

            dashboardDataManager.OnDashboardDataLoaded += Render;

            if (dashboardDataManager.CurrentData != null)
            {
                Render(dashboardDataManager.CurrentData);
            }
        }

        private void OnDisable()
        {
            if (dashboardDataManager != null)
            {
                dashboardDataManager.OnDashboardDataLoaded -= Render;
            }
        }

        private void Render(DashboardData data)
        {
            DailyQuestEntryData[] quests = data.dailyQuests;

            for (int i = 0; i < questSlots.Length; i++)
            {
                QuestSlot slot = questSlots[i];

                if (slot == null)
                {
                    continue;
                }

                bool hasQuest = quests != null && i < quests.Length && quests[i] != null;

                if (slot.root != null)
                {
                    slot.root.SetActive(hasQuest);
                }

                if (!hasQuest)
                {
                    continue;
                }

                RenderSlot(slot, quests[i]);
            }
        }

        private void RenderSlot(QuestSlot slot, DailyQuestEntryData questEntry)
        {
            QuestData quest = questEntry.questId;

            if (quest == null)
            {
                SetText(slot.descriptionText, "Missing quest data");
                SetText(slot.rewardText, "");
                SetText(slot.completedText, "");
                SetMedal(slot, false);
                return;
            }

            SetText(slot.descriptionText, quest.description);
            SetText(slot.rewardText, BuildRewardText(quest));

            if (questEntry.isCompleted)
            {
                SetText(slot.completedText, "Completed");
                SetMedal(slot, true);
            }
            else
            {
                SetText(slot.completedText, "");
                SetMedal(slot, false);
            }
        }

        private string BuildRewardText(QuestData quest)
        {
            return "+" + quest.xpReward + " XP   +" + quest.coinsReward + " Coins";
        }

        private void SetMedal(QuestSlot slot, bool isCompleted)
        {
            if (slot.medalImage == null)
            {
                return;
            }

            if (isCompleted && slot.completedMedalSprite != null)
            {
                slot.medalImage.sprite = slot.completedMedalSprite;
                return;
            }

            if (!isCompleted && slot.incompleteMedalSprite != null)
            {
                slot.medalImage.sprite = slot.incompleteMedalSprite;
            }
        }

        private void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}