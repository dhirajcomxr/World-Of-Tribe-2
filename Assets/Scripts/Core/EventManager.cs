using System;

public class EventManager : Singleton<EventManager>
{
    public event Action meleAttackButton;
    public event Action skillOneAttackButton;
    public event Action skillTwoAttackButton;
    public event Action skillThreeAttackButton;
    public event Action skillFourAttackButton;

    public void OnMeleAttackTrigger()
    {
        meleAttackButton?.Invoke();
    }

    public void OnSkillAttackTrigger(int skillNumber)
    {
        switch (skillNumber)
        {
            case 1:
                skillOneAttackButton?.Invoke();
                break;
            case 2:
                skillTwoAttackButton?.Invoke();
                break;
            case 3:
                skillThreeAttackButton?.Invoke();
                break;
            case 4:
                skillFourAttackButton?.Invoke();
                break;
            default:
                skillOneAttackButton?.Invoke();
                break;
        }
        
    }
}
