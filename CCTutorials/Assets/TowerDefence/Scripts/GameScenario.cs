using UnityEngine;

[CreateAssetMenu]
public class GameScenario : ScriptableObject
{
    [SerializeField]
    private EnemyWave[] waves = { };
    [SerializeField, Range(0, 10)]
    private int cycles = 1;
    [SerializeField, Range(0f, 1f)]
    private float cycleSpeedUp = 0.5f;
    [SerializeField, TextArea]
    private string startText = default;
    [SerializeField]
    public float initialDelay = default;
    [SerializeField]
    private float waveDelay = default;

    public State Begin() => new State(this);

    [System.Serializable]
    public struct State
    {

        GameScenario scenario;

        int cycle, index;

        float timeScale;

        EnemyWave.State wave;

        private float waveDelay;

        public float timer;

        public State(GameScenario scenario)
        {
            this.scenario = scenario;
            cycle = 0;
            index = 0;
            timeScale = 1f;
            Debug.Assert(scenario.waves.Length > 0, "Empty scenario!");
            wave = scenario.waves[0].Begin();
            waveDelay = scenario.waveDelay * 60f;
            timer = scenario.initialDelay * 60f;
            Game.DisplayScenarioText(scenario.startText + scenario.initialDelay.ToString() + " minutes before the enemy reaches our borders!");
        }

        public bool Progress()
        {
            if(timer >= 0)
            {
                timer -= timeScale * Time.deltaTime;
                Game.UpdateTimerBar(scenario, timer);
                return true;
            }
            else
            {
                if(timer!= -1)
                {
                    Game.UpdateTimerBar(scenario, 0f);
                }
                float deltaTime = wave.Progress(timeScale * Time.deltaTime);
                while (deltaTime >= 0f)
                {
                    if (++index >= scenario.waves.Length)
                    {
                        if (++cycle >= scenario.cycles && scenario.cycles > 0)
                        {
                            return false;
                        }
                        index = 0;
                        timeScale += scenario.cycleSpeedUp;
                    }
                    else
                    {
                        timer = waveDelay;
                        wave = scenario.waves[index].Begin();
                    }
                    //deltaTime = wave.Progress(deltaTime);
                }
                return true;
            }
        }
    }
}
