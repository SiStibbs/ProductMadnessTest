using System.Collections;
using UnityEngine;

namespace WheelSpin
{
    /// <summary>
    /// A class to control the spinning of the wheel.
    /// </summary>
    public class WheelSpinController : MonoBehaviour
    {
        public Transform wheelTransform;
        public float maxSpinSpeed = 200f;
        public AnimationCurve animCurve;

        private bool isSpinning;
        private float spinSpeed;
        private float targetAngle;
        private float currentAngle;

        private float lerpAngle;
        private float elapsed = 0;
        private float duration;

        private int[] wheelValues = { 10, 3, 4, 1, 3, 5, 7, 8, 2, 10, 6, 2, 9, 4, 5, 6, 8, 2 };

        private System.Action onSpinComplete;

        /// <summary>
        /// Control the wheel rotation
        /// </summary>
        void Update()
        {
            if (isSpinning)
            {
                if (targetAngle != float.MaxValue)
                {
                    //Lerp to the desired position
                    if (elapsed < duration)
                    {
                        elapsed += Time.deltaTime;
                        float t = elapsed / duration;
                        t = animCurve.Evaluate(t);
                        lerpAngle = Mathf.Lerp(currentAngle, targetAngle, t);
                        wheelTransform.eulerAngles = new Vector3(0, 0, lerpAngle);
                    }
                    else
                    {
                        //Desired position reached
                        SpinComplete();
                    }
                }
                else
                {
                    //Continuous rotation
                    wheelTransform.Rotate(0, 0, spinSpeed * Time.deltaTime);
                    currentAngle = wheelTransform.rotation.eulerAngles.z;
                }
            }
        }

        /// <summary>
        /// Start spinning the wheel
        /// </summary>
        /// <param name="multiplier">The multiplier to stop on</param>
        /// <param name="onSpinComplete">Spin complete callback</param>
        public void SpinWheel(int multiplier, System.Action onSpinComplete)
        {
            this.onSpinComplete = onSpinComplete;
            targetAngle = float.MaxValue;
            spinSpeed = maxSpinSpeed;
            isSpinning = true;
            StartCoroutine(StopWheelDelayed(multiplier));
        }

        private void SpinComplete()
        {
            isSpinning = false;
            onSpinComplete?.Invoke();
        }

        /// <summary>
        /// Stop the wheel after an amount of time at the correct position
        /// </summary>
        /// <param name="multiplier">The desired amount to stop the wheel on</param>
        /// <returns></returns>
        private IEnumerator StopWheelDelayed(int multiplier)
        {
            yield return new WaitForSeconds(Random.Range(1f, 2f));

            //Get current rotation and wheel position
            float rot = wheelTransform.rotation.eulerAngles.z;
            int pos = Mathf.RoundToInt((rot + 5) / 20);

            for (int i = pos; i < wheelValues.Length + pos; i++)
            {
                //Find next sector containing the correct multiplier
                if (GetWheelPos(i) == multiplier)
                {
                    //Get the desired amount of rotation, and amount of time to complete the rotation
                    int diff = i + 20 - pos;
                    duration = diff * 0.5f;
                    elapsed = 0;
                    targetAngle = i * 20 + 720;
                    break;
                }
            }
        }

        /// <summary>
        /// Get a wheel pos when the index be greater than the array size
        /// </summary>
        /// <param name="index">The index to find</param>
        /// <returns>The wheel position</returns>
        private int GetWheelPos(int index)
        {
            if (index > wheelValues.Length - 1)
            {
                index -= wheelValues.Length;
            }
            return wheelValues[index];
        }
    }
}