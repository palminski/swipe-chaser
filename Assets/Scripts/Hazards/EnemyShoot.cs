using System.Collections.Generic;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    public float shotInterval = 1f;
    public List<ShotStep> shotPattern;
    private int shotIndex = 0;
    private float nextShot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nextShot = shotInterval;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextShot)
        {
            nextShot = Time.time + shotInterval;
            Shoot();
        }
    }

    void Shoot()
    {
        if (shotPattern.Count <= 0) return;
        ShotStep shotStep = shotPattern[shotIndex];
        List<ShotInfo> shots = shotStep.shotInfos;
        foreach (ShotInfo shot in shots)
        {
            Bullet _bullet = Instantiate(shot.bullet, transform.position, transform.rotation);
            _bullet.bulletAngle = shot.angle;
            _bullet.speed = shot.speed;
        }
        if (shotStep.timeUntilNextShot > 0) nextShot = Time.time + shotStep.timeUntilNextShot; 
        shotIndex++;
        if (shotIndex >= shotPattern.Count)
        {
            shotIndex = 0;
        }
    }
}

[System.Serializable]
public class ShotStep
{
    public List<ShotInfo> shotInfos;
    public float timeUntilNextShot;
}

[System.Serializable]
public struct ShotInfo
{
    public Bullet bullet;
    public float angle;
    public float speed;
}
