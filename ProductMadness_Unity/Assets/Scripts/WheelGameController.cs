using Server.API;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WheelSpin
{
    /// <summary>
    /// Controller for the wheel game
    /// </summary>
    public class WheelGameController : MonoBehaviour
    {
        public WheelSpinController wheelController;
        public Text initialText;
        public Text multiplierText;
        public Text balanceText;
        public Button spinButton;

        private GameplayApi api;
        private long balance;
        private int multiplier = 4;
        private int initialWin;

        void Start()
        {
            api = new GameplayApi();
            api.Initialise().Then(() => SetupGame());
            spinButton.interactable = false;
        }

        /// <summary>
        /// Make calls to get player data
        /// </summary>
        private void SetupGame()
        {
            api.GetPlayerBalance().Then(balance =>
            {
                Debug.Log("Balance: " + balance);
                this.balance = balance;
            });

            api.GetInitialWin().Then((initialWin) =>
            {
                initialText.text = initialWin.ToString();
                this.initialWin = initialWin;
            });

            api.GetMultiplier().Then((multiplier) =>
            {
                this.multiplier = multiplier;
                spinButton.interactable = true;
            });
        }

        /// <summary>
        /// Begin spinning the wheel
        /// </summary>
        public void SpinWheel()
        {
            spinButton.interactable = false;
            wheelController.SpinWheel(multiplier, () =>
            {
                //Spin Complete
                multiplierText.text = multiplier.ToString();
                balance += initialWin * multiplier;
                balanceText.text = (initialWin * multiplier).ToString();
                spinButton.interactable = true;
                Debug.Log("Player Balance: " + balance);
                api.SetPlayerBalance(balance)
                    .Then(() =>
                    {
                        api.GetMultiplier().Then((multiplier) =>
                        {
                            this.multiplier = multiplier;
                        });
                    });
            });
        }
    }
}