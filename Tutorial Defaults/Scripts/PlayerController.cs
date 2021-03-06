using UnityEngine;

[RequireComponent(typeof(HealthController))]
public class PlayerController : MonoBehaviour
{
    public int score = 0;
    public int frags = 0;
    public int assists = 0;
    public delegate void OnPlayerDeathDelegate();
    public event OnPlayerDeathDelegate deathEvent;
    private HealthController healthController;
    private void Start() 
    {        
        healthController = GetComponent<HealthController>();
        if(!healthController)
        {
            healthController = gameObject.AddComponent<HealthController>();
        }
      //  healthController = healthController == null ?  healthController = gameObject.AddComponent<HealthController>() : healthController = this.healthController;
    }

    void Update() 
    {
        if(this.healthController.lives <= 0)
        {
            Death();
        }
    }
    private void Death() 
    {
        if(deathEvent != null)
        {
            deathEvent();
        }
    }
    
}
