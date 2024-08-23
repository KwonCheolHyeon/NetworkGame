
public interface ICharacterState
{
    void EnterState(PlayerScript character);  // ���¿� ������ �� ȣ��Ǵ� �޼���
    void UpdateState(PlayerScript character); // ���°� ���ӵǴ� ���� �� �����Ӹ��� ȣ��Ǵ� �޼���
    void FixedUpdateState(PlayerScript character); // ���°� ���ӵǴ� ���� �� �����Ӹ��� ȣ��Ǵ� �޼���
    void ExitState(PlayerScript character);   // ���¸� ��� �� ȣ��Ǵ� �޼���
}

public enum eGameCharacterType 
{
    PLAYER,
    Enemy
}