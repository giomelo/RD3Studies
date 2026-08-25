using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BTree.BehaviourTree
{
    public class BlackBoard : MonoBehaviour
    {
        private static BlackBoard _instance;

        public static BlackBoard Instance
        {
            get
            {
                if (!_instance)
                {
                    var blackBoard = FindObjectsOfType<BlackBoard>();
                    if (blackBoard != null)
                    {
                        if (blackBoard.Length > 1)
                        {
                            _instance = blackBoard[0];
                        }
                    }
                    var go = new GameObject("BlackBoard", typeof(BlackBoard));
                    _instance = go.GetComponent<BlackBoard>();
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
            set => _instance = value;
        }

        public int timeOfDay;
        [SerializeField]private TextMeshProUGUI clock;
        public Stack<PatronBehaviour> patrons = new Stack<PatronBehaviour>();

        public int openTime = 6;
        public int closeTime = 22;


        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            StartCoroutine(UpdateClock());
        }

        private IEnumerator UpdateClock()
        {
            while (true)
            {
                timeOfDay++;
                if (timeOfDay > 23) timeOfDay = 0;
                clock.text = timeOfDay + ":00";
                
                if(timeOfDay == closeTime)
                    patrons.Clear();
                
                
                yield return new WaitForSeconds(1f);
            }
        }


        public bool RegisterPatron(PatronBehaviour p)
        {
           patrons.Push(p);
           return true;
        }

        public void DeregisterPatron()
        {
           // patrons.Pop();
        }
    }
}
