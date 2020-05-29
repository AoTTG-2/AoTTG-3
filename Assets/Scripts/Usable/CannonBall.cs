using Assets.Scripts.Characters.Titan;
using Assets.Scripts.Gamemode.Options;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView), typeof(SphereCollider))]
public class CannonBall : Photon.MonoBehaviour
{
    [SerializeField]
    private float smoothingDelay = 10f;

    private List<TitanTrigger> myTitanTriggers;

    private Cannon cannon;
    private int heroViewId;

    private bool isCollider;
    private bool disabled;

    private Vector3 correctPos;
    private Vector3 correctVelocity;

    private int baseMask;
    private int groundMask;

    public static CannonBall Create(Vector3 position, Quaternion rotation, Vector3 velocity, Cannon cannon,
        int heroViewId)
    {
        var instance = PhotonNetwork.Instantiate("RC Resources/RC Prefabs/CannonBallObject",
            position,
            rotation,
            0).GetComponent<CannonBall>();
        instance.GetComponent<Rigidbody>().velocity = velocity;
        instance.cannon = cannon;
        instance.heroViewId = heroViewId;
        return instance;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(GetComponent<Rigidbody>().velocity);
        }
        else
        {
            correctPos = (Vector3)stream.ReceiveNext();
            correctVelocity = (Vector3)stream.ReceiveNext();
        }
    }

    private void Awake()
    {
        myTitanTriggers = new List<TitanTrigger>();

        baseMask = 1 << LayerMask.NameToLayer("PlayerAttackBox") | 1 << LayerMask.NameToLayer("EnemyBox");
        groundMask = 1 << LayerMask.NameToLayer("Ground");

        photonView.observed = this;

        correctPos = transform.position;
        correctVelocity = Vector3.zero;

        GetComponent<SphereCollider>().enabled = false;

        if (photonView.isMine)
            StartCoroutine(WaitAndSelfDestruct(10f));
    }

    private IEnumerator WaitAndSelfDestruct(float time)
    {
        yield return new WaitForSeconds(time);
        SelfDestruct();
    }

    private void SelfDestruct()
    {
        if (disabled)
            return;

        disabled = true;

        // TODO: Replace this with a factory method.
        foreach (EnemyCheckCollider collider in PhotonNetwork.Instantiate("FX/boom4", transform.position, transform.rotation, 0).GetComponentsInChildren<EnemyCheckCollider>())
            collider.dmg = 0;

        if (FengGameManagerMKII.Gamemode.Settings.PvpCannons)
            KillEnemyPlayersInRange(20f);

        ResetTitanTriggers();

        PhotonNetwork.Destroy(base.gameObject);
    }

    private void KillEnemyPlayersInRange(float range)
    {
        foreach (Hero player in FengGameManagerMKII.instance.getPlayers())
        {
            bool isOtherPlayerWithinRange = player && !player.photonView.isMine && Vector3.Distance(player.transform.position, base.transform.position) <= range;
            if (isOtherPlayerWithinRange)
            {
                PhotonPlayer owner = player.gameObject.GetPhotonView().owner;

                // TODO: Investigate whether all valid teams are > -1.
                int playerTeam = (int)(owner.CustomProperties[PhotonPlayerProperty.RCteam] ?? -1);
                int myTeam = (int)(PhotonNetwork.player.CustomProperties[PhotonPlayerProperty.RCteam] ?? -1);
                bool bothTeamsValid = playerTeam != -1 && myTeam != -1;
                bool myTeamExists = myTeam != 0;
                bool sameTeam = myTeam == playerTeam;
                bool teamsEnabled = FengGameManagerMKII.Gamemode.Settings.TeamMode != TeamMode.Disabled;
                bool canKillPlayer = !(teamsEnabled && bothTeamsValid && myTeamExists && sameTeam);

                if (canKillPlayer)
                    KillPlayer(player);
            }
        }
    }

    private void KillPlayer(Hero player)
    {
        var myName = $"{PhotonNetwork.player.CustomProperties[PhotonPlayerProperty.name] ?? "ERROR"} ";
        player.markDie();
        player.photonView.RPC("netDie2", PhotonTargets.All, new object[] { -1, myName });
        FengGameManagerMKII.instance.playerKillInfoUpdate(PhotonNetwork.player, 0);
    }

    private void ResetTitanTriggers()
    {
        for (int i = 0; i < myTitanTriggers.Count; i++)
            myTitanTriggers[i].SetCollision(false);
    }

    private void Update()
    {
        if (!photonView.isMine)
        {
            transform.position = Vector3.Lerp(transform.position, correctPos, Time.deltaTime * smoothingDelay);
            GetComponent<Rigidbody>().velocity = correctVelocity;
        }
    }

    private void FixedUpdate()
    {
        if (photonView.isMine && !disabled)
        {
            int mask = isCollider ? baseMask : (baseMask | groundMask);
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.6f, mask);
            bool hitSelf = false;
            for (int i = 0; i < hitColliders.Length; i++)
            {
                GameObject currentGobj = hitColliders[i].gameObject;
                bool isGroundLayer = currentGobj.layer == 9;
                bool isPlayerAttackBoxLayer = currentGobj.layer == 16;
                if (isPlayerAttackBoxLayer)
                {
                    TitanTrigger titanTrigger = currentGobj.GetComponent<TitanTrigger>();
                    if (!myTitanTriggers.Contains(titanTrigger))
                    {
                        titanTrigger.SetCollision(true);
                        myTitanTriggers.Add(titanTrigger);
                    }
                }
                else if (isGroundLayer && currentGobj.GetComponentInParent<Cannon>() == this.cannon)
                    hitSelf = true;
            }
            if (!(isCollider || hitSelf))
            {
                isCollider = true;
                GetComponent<SphereCollider>().enabled = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.isMine)
            return;

        Collider collider = collision.collider;
        bool isEnemyBoxLayer = collider.gameObject.layer == 10;
        if (isEnemyBoxLayer)
        {
            MindlessTitan titan = collision.gameObject.GetComponent<MindlessTitan>();
            if (titan != null)
            {
                titan.photonView.RPC("OnCannonHitRpc", titan.photonView.owner, heroViewId, collider.name);
                SelfDestruct();
            }
        }
        else
            SelfDestruct();
    }
}