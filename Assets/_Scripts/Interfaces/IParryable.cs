using UnityEngine;

public interface IParryable
{
    public bool CanBeParried { get; }

    public void OnParried(Vector2 parryPos);
}
