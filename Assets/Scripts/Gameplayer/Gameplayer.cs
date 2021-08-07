using System;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun.Escape.GM.Game;

namespace PlayerSpace.Gameplayer
{
    public class Gameplayer : MonoBehaviourPunCallbacks, IPunObservable
    {
        [SerializeField] bool testMode = false;
        #region ID Variables
        public byte playerID = 0;
        public byte teamID = 0;
        public byte currentRoomID = 0;
        #endregion

        #region Classes Variables
        Rewired.Player input = null;
        Control control = null;
        PhotonView pv;
        #endregion

        #region Callbacks
        List<Action<Gameplayer>> gameActions = null;
        void StartItemCallback() => gameActions[0](this);
        void CaughtCallBack() => gameActions[1](this);
        void GoalCallback() => gameActions[2](this);

        List<Action<Gameplayer>> changeLevel = null;
        void GoNextRoom() => changeLevel[0](this);
        void GoPrevRoom() => changeLevel[1](this);
        bool isTeleporting = false;
        #endregion

        #region IPunObservable implementation
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(playerID);
                stream.SendNext(teamID);
                stream.SendNext(currentRoomID);
            }
            else
            {
                this.playerID = (byte)stream.ReceiveNext();
                this.teamID = (byte)stream.ReceiveNext();
                this.currentRoomID = (byte)stream.ReceiveNext();
            }
        }
        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
        {
            if (!pv.IsMine && targetPlayer == pv.Owner)
            {
                // id = (int)changedProps["NewID"];
                // OnSelectIndexChanged(selectIndex);
            }
        }
        #endregion

        #region RPCs
        [PunRPC]
        public void RpcAssignTeam(object[] data)
        {
            byte id = (byte)data[0];
            byte teamID = (byte)data[1];

            this.playerID = id;
            this.teamID = teamID;

            AssignController(0);
        }
        #endregion

        #region Public Methods
        public void HunterDebuff(int playerNum)
        {
            control.HunterDebuff(playerNum);
        }
        public void SetCamera()
        {
            Camera.main.enabled = false;
        }
        public void AssignController(int controllerID)
        {
            if (pv.IsMine)
            {
                input = ReInput.players.GetPlayer(controllerID);

                bool isKeyboard = input.controllers.joystickCount > 0 ? false : true;
                control.AssignControllerType(isKeyboard);
            }
        }
        public void AssignTeam(byte id, List<Action<Gameplayer>> callbacks, List<System.Action<Gameplayer>> changeLevelCallbacks)
        {
            teamID = id;
            control.AssignTeam(id);
            gameActions = callbacks;
            changeLevel = changeLevelCallbacks;

            object[] data = new object[] { playerID, teamID };
            pv.RPC("RpcAssignTeam", pv.Owner, data);
        }
        #endregion

        #region Unity Native APIs
        private void Awake()
        {
            control = GetComponent<Control>();
            pv = GetComponentInParent<PhotonView>();
            SetCamera();
        }
        public override void OnEnable()
        {
            if (testMode) AssignController(0);
            GameManager.instance.OnPlayerSpawned(this);
        }
        private void Update()
        {
            if (pv.IsMine && input != null)
            {
                ItemHandler();
                MoveHandler();
                CombatHandler();
            }
        }
        #region OnTriggerFuncs
        private void OnTriggerEnter2D(Collider2D other)
        {
            switch (other.tag)
            {
                case "Confiner":
                    control.MyConfiner().m_BoundingShape2D = other.GetComponent<PolygonCollider2D>();
                    break;
                case "PlayerWeapon":
                    Vector2 force = (transform.position - other.transform.parent.position);
                    control.Hurt(force, CaughtCallBack);
                    break;
                case "DashItem":
                    string[] nameSplice = other.name.Split(',');
                    float forceX, forceY = 0;
                    forceX = (string.Compare("n", nameSplice[0]) == 0) ? transform.lossyScale.x : float.Parse(nameSplice[0]);
                    forceY = (string.Compare("n", nameSplice[1]) == 0) ? transform.lossyScale.y : float.Parse(nameSplice[1]);
                    control.DoDash(new Vector2(forceX, forceY));
                    break;
                case "RoomTeleport":
                    if (changeLevel == null || isTeleporting) return;
                    isTeleporting = true;
                    switch (other.name)
                    {
                        case "NextCollider":
                            GoNextRoom();
                            break;
                        case "PrevCollider":
                            GoPrevRoom();
                            break;
                    }
                    this.AbleToDo(0.1f, () => isTeleporting = false);
                    break;
            }
        }
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.tag == "StartItem")
            {
                control.ActiveHintUI(true, other.transform.position);

                // Prevent unregister item.
                switch (other.name)
                {
                    case "IceSkate":
                    case "SlimeShoe":
                    case "SwiftnessBoot":
                    case "RocketShoe":
                    case "Shield":
                    case "EnergyDringk":
                    case "ExtralLife":
                    case "Armor":
                    case "InspectorChance":
                    case "DeathWithStronger":
                    case "Balloon":
                        if (input.GetButtonDown("Item"))
                        {
                            control.GetStartItem(other.gameObject, StartItemCallback);
                        }
                        break;
                    default:
                        Debug.Log("Item is not reigstered in gameplayer, name: \'" + other.name + "\'. Make sure you resigister it on both player and control");
                        break;
                }
            }

            if (other.tag == "GameItem")
            {
                Spawner spawner = other.GetComponent<Spawner>();
                if (spawner.currentItemID == -1 || control.IsItemNull() == false)
                    return;

                control.ActiveHintUI(true, other.transform.position);

                if (input.GetButtonDown("Item"))
                {
                    control.SetGameItem(spawner.currentItemID, spawner.ResetItem);
                }
            }

            if (other.tag == "Flag")
            {
                if (control.IsGoaled()) return;

                control.ActiveHintUI(true, other.transform.position);
                if (input.GetButtonDown("Item"))
                {
                    control.ActiveHintUI(false);
                    control.Goal(GoalCallback);
                }
            }
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag == "StartItem")
            {
                control.ActiveHintUI(false);
            }
            if (other.tag == "GameItem")
            {
                control.ActiveHintUI(false);
            }
            if (other.tag == "Flag")
            {
                control.ActiveHintUI(false);
            }
        }
        #endregion
        #endregion

        #region Input Implement
        private void ItemHandler()
        {
            if (input.GetButtonDown("Item"))
            {
                control.UseGameItem();
            }
        }
        private void MoveHandler()
        {
            if (input.GetButton("MoveR"))
            {
                control.Move(1);
            }
            else if (input.GetButton("MoveL"))
            {
                control.Move(-1);
            }
            else if (input.GetButtonUp("MoveL") || input.GetButtonUp("MoveR"))
            {
                control.Move(0);
            }

            if (input.GetButtonDown("Run"))
            {
                control.Run(true);
            }
            else if (input.GetButtonUp("Run"))
            {
                control.Run(false);
            }

            if (input.GetButtonDown("Jump"))
            {
                control.Jump(true);
            }
            else if (input.GetButton("Jump"))
            {
                control.Jump(true);
            }
            else if (input.GetButtonUp("Jump"))
            {
                control.Jump(false);
            }
        }
        public void CombatHandler()
        {
            if (input.GetButtonDown("Attack"))
            {
                control.Attack();
            }
        }
        #endregion
    }
}