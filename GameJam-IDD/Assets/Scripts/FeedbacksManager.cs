using UnityEngine;
using MoreMountains.Feedbacks;

public class FeedbacksManager : MonoBehaviour
{
    [Header("Player")]
    public float minVelocityToPlayFeedback;
    private Player player;
    private Rigidbody2D rb;
    public MMFeedbacks jumpFeedback;
    public MMFeedbacks landingFeedback;
    public MMFeedbacks deathFeedback;
    
    private void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        rb = player.GetComponent<Rigidbody2D>();
    }
    public void PlayJumpFeedback(Component sender, object data)
    {
        if(sender is  Player)
        {
            jumpFeedback.PlayFeedbacks();
        }
    }
    public void PlayLandingFeedback(Component sender, object data)
    {
        landingFeedback.PlayFeedbacks();
    }
    public void PlayDeathFeedback(Component sender, object data)
    {
        Debug.Log("dsada");
        deathFeedback.PlayFeedbacks();
    }
}
