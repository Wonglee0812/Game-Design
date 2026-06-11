using UnityEngine;

public class Shop : MonoBehaviour
{
    public void SelectTurretToBuild(TurretBluePrint blueprintData)
    {
        Debug.Log("Sẵn sàng xây: " + blueprintData.turretName + " | Giá: " + blueprintData.cost);
        BuildManager.instance.SetTurretToBuild(blueprintData);
    }
}
