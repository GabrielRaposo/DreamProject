public interface IStompable
{
    float GetYStompRange();
    void OnStompEvent(PlayerPlatformer player);
    void OnTouchEvent(PlayerPlatformer player);
}
