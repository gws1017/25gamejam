using UnityEngine;

public class PlayerCharacter : BaseCharacter
{
    //�÷��̾� ����
    private int level;
    private int currentExp;
    private int maxExxp;

    public int CurrentExp => currentExp;
    public int Level => level;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreamentExp(int exp)
    {
        currentExp += exp;
        currentExp = Mathf.Clamp(currentExp, 0, maxExxp);
        LevelUp();
    }

    public void LevelUp()
    {
        if (currentExp >= maxExxp)
        {
            currentExp = 0;
            level++;
        }
    }
}
