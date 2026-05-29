using UnityEngine;

// [VI] Không dùng class, mà dùng interface
// [EN] Don't use class, use interface
public interface IFrogInteractable
{
    // [VI] Ép buộc mọi script kế thừa phải có hàm này
    // [EN] Forces any inheriting script to have this function
    void OnTongueHit();
}