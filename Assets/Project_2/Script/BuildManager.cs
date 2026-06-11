using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;
    private TurretBluePrint turretToBuild;

    public bool CanBuild { get { return turretToBuild != null; } }
    private void Start()
    {
        turretToBuild = null;
    }
    private void Awake()
    {
        if(instance != null)
        {
            Debug.Log("Too many GameManager");
            return;
        }
        instance = this;
    }


    public void SetTurretToBuild(TurretBluePrint blueprint)
    {
        turretToBuild = blueprint;
    }

    public TurretBluePrint GetTurretToBuild()
    {
        return turretToBuild;
    }
}
