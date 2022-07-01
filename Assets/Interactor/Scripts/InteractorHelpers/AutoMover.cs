using UnityEngine;
using System.Collections;

namespace razz
{
    public class AutoMover : MonoBehaviour
    {
        private float _rot = -1f;

        public enum MoveType { Once, Loop, Wave }
        public enum Reverse { Default = 1, Reversed = -1 }
        public MoveType moveType;
        public EaseType positionEase;
        public EaseType rotationEase;
        public Reverse leftRight;
        public float duration;
        [Range(0, 1)] public float offset;
        public Vector3 fromPosition;
        public Vector3 toPosition;
        public Vector3 fromRotation;
        public Vector3 toRotation;
        public bool reversable;

        [HideInInspector] public bool started;
        [HideInInspector] public bool half;
        [HideInInspector] public bool ended;
        [HideInInspector] public bool reverse;

        public void StartMovement(float y)
        {
            if (_rot < 0)
            {
                _rot = y;
                return;
            }

            if (leftRight == Reverse.Reversed)
            {
                if (y < _rot)
                {
                    reverse = true;

                    if (fromPosition != toPosition)
                        StartCoroutine(Position());
                    if (fromRotation != toRotation)
                        StartCoroutine(Rotation());

                    _rot = y;
                }
                else if (y > _rot)
                {
                    reverse = false;

                    if (fromPosition != toPosition)
                        StartCoroutine(Position());
                    if (fromRotation != toRotation)
                        StartCoroutine(Rotation());

                    _rot = y;
                }
            }
            else
            {
                if (y > _rot)
                {
                    reverse = true;

                    if (fromPosition != toPosition)
                        StartCoroutine(Position());
                    if (fromRotation != toRotation)
                        StartCoroutine(Rotation());

                    _rot = y;
                }
                else if (y < _rot)
                {
                    reverse = false;

                    if (fromPosition != toPosition)
                        StartCoroutine(Position());
                    if (fromRotation != toRotation)
                        StartCoroutine(Rotation());

                    _rot = y;
                }
            }
        }

        public void StartMovement()
        {
            ended = false;
            started = true;
            
            if (fromPosition != toPosition)
                StartCoroutine(Position());
            if (fromRotation != toRotation)
                StartCoroutine(Rotation());

            if (reversable)
                reverse = !reverse;
        }

        public void MovementHalf()
        {
            half = true;
        }

        public void MovementEnd()
        {
            ended = true;
        }

        public void ResetBools()
        {
            ended = false;
            half = false;
            started = false;

            if (!reversable)
            {
                transform.localPosition = fromPosition;
                transform.localRotation = Quaternion.Euler(fromRotation);
            }
        }

        IEnumerator Position()
        {
            var from = fromPosition;
            var to = toPosition;

            if (moveType == MoveType.Once)
            {
                if (!reverse)
                {
                    StartCoroutine(Auto.MoveTo(this.transform, to, duration, positionEase, this));
                }
                else
                {
                    StartCoroutine(Auto.MoveTo(this.transform, from, duration, positionEase, this));
                }
                yield return 0;
            }
            else if (moveType == MoveType.Loop)
            {
                while (true)
                {
                    transform.localPosition = Auto.Loop(duration, from, to, offset);
                    yield return 0;
                }
            }
            else if(moveType == MoveType.Wave)
            {
                while (true)
                {
                    transform.localPosition = Auto.Wave(duration, from, to, offset);
                    yield return 0;
                }
            }
        }

        IEnumerator Rotation()
        {
            var from = Quaternion.Euler(fromRotation);
            var to = Quaternion.Euler(toRotation);

            if (moveType == MoveType.Once)
            {
                if (!reverse)
                {
                    StartCoroutine(Auto.RotateTo(this.transform, to, duration, rotationEase));
                }
                else
                {
                    StartCoroutine(Auto.RotateTo(this.transform, from, duration, rotationEase));
                }
                yield return 0;
            }
            else if (moveType == MoveType.Loop)
            {
                while (true)
                {
                    transform.localRotation = Auto.Loop(duration, from, to, offset);
                    yield return 0;
                }
            }
            else if(moveType == MoveType.Wave)
            {
                while (true)
                {
                    transform.localRotation = Auto.Wave(duration, from, to, offset);
                    yield return 0;
                }
            }
        }
    }
}
