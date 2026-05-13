using UnityEngine;

public static class OpenPawnShop
{
    public static void Execute()
    {
        PawnShopUI.Instance?.Open();
    }
}
