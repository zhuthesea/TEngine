using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TEngine
{
    public class Settings : MonoBehaviour
    {
        private static Settings _instance;

        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<Settings>();

                    if (_instance != null)
                    {
                        return _instance;
                    }
                }

                return _instance;
            }
        }

        [SerializeField] private AudioSetting audioSetting;
        
        [SerializeField] private ProcedureSetting procedureSetting;

        [SerializeField] private UpdateSetting updateSetting;
        
        public static AudioSetting AudioSetting => Instance.audioSetting;
        
        public static ProcedureSetting ProcedureSetting => Instance.procedureSetting;

        public static UpdateSetting UpdateSetting => Instance.updateSetting;
    }
}