using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
namespace Mirror.EscapeGame
{
    public class RoomPlayer : NetworkBehaviour
    {
        [SyncVar]
        public int id = 0;
        [SyncVar]
        public int selectIndex = 0;
        [SyncVar]
        public int roleIndex = 0;
        [SyncVar]
        public int mapIndex = 0;
        [SyncVar]
        public int selectState = 0;

        public Canvas container;
        public Transform roleUI;
        public Transform mapUI;

        Player input;

        public void OnSelectChaned(int val, int newVal)
        {
            RpcActiveUI(id, val, selectState, false);
            RpcActiveUI(id, newVal, selectState, true);
        }

        [Command]
        public void CmdSetSelect(int val)
        {
            Select(val);
        }

        public void Select(int additive)
        {
            if (isServer)
            {
                if (selectState == 0 && selectIndex + additive >= roleUI.childCount) return;
                if (selectIndex + additive < 0) return;
                BroadCastToAll("Valqowpfjwpqof");
                OnSelectChaned(selectIndex, selectIndex + additive);
                selectIndex += additive;
            }
            else
            {
                CmdSetSelect(selectIndex);
            }
        }

        [ClientRpc]
        public void BroadCastToAll(string msg) => Debug.Log(msg);

        public void SyncUI(List<RoomPlayer> players)
        {
            ResetUI();
            if (isLocalPlayer)
                CmdResetUI();
            foreach (RoomPlayer p in players)
            {
                ActiveUI(p.id, p.selectIndex, p.selectState, true);
                if (isLocalPlayer)
                    CmdActiveUI(p.id, p.selectIndex, p.selectState, true);
            }
        }

        [Command]
        public void CmdActiveUI(int id, int index, int select, bool isActive)
        {
            RpcActiveUI(id, index, select, isActive);
        }

        [ClientRpc]
        public void RpcActiveUI(int id, int index, int select, bool isActive)
        {
            ActiveUI(id, index, select, isActive);
        }

        public void ActiveUI(int id, int index, int selectState, bool isActive)
        {
            switch (selectState)
            {
                case 0:
                    roleUI.GetChild(index).GetChild(id + 1).gameObject.SetActive(isActive);
                    break;
                case 1:
                    mapUI.GetChild(index).GetChild(id + 1).gameObject.SetActive(isActive);
                    break;
                case 2:
                default:
                    return;
            }
        }

        [Command]
        public void CmdResetUI()
        {
            RpcResetUI();
        }

        [ClientRpc]
        public void RpcResetUI()
        {
            ResetUI();
        }

        public void ResetUI()
        {
            int i = 0;
            foreach (Transform container in roleUI)
            {
                for (int j = 0; j < container.childCount; j++)
                {
                    if (j == 0) continue;
                    roleUI.GetChild(i).GetChild(j).gameObject.SetActive(false);
                }
                i++;
            }
            i = 0;
            foreach (Transform container in mapUI)
            {
                for (int j = 0; j < container.childCount; j++)
                {
                    if (j == 0) continue;
                    mapUI.GetChild(i).GetChild(j).gameObject.SetActive(false);
                }
                i++;
            }
        }

        private void Update()
        {
            if (input == null) return;
            if (isLocalPlayer)
            {
                if (input.GetButtonDown("SelectR"))
                {
                    Select(1);
                }
                else if (input.GetButtonDown("SelectL"))
                {
                    Select(-1);
                }
                else if (input.GetButtonDown("SelectU"))
                {
                }
                else if (input.GetButtonDown("SelectD"))
                {
                }
                else if (input.GetButtonDown("Confirm"))
                {
                }
                else if (input.GetButtonDown("Cancel"))
                {
                }
            }

        }

        private void Awake()
        {
            input = ReInput.players.GetPlayer(0);
        }

        #region Mirror virtual func
        public override void OnStartServer()
        {
            base.OnStartServer();
        }
        public override void OnStopServer()
        {
            base.OnStopServer();
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (NetworkManager.singleton is NetworkManagerLobby room)
            {
                DontDestroyOnLoad(this);

                container = room.container;
                roleUI = room.roleUI;
                mapUI = room.mapUI;

                room.roomSlots.Add(this);
                room.ResetPlayerID();
            }
        }
        public override void OnStopClient()
        {
            base.OnStopClient();
            if (NetworkManager.singleton is NetworkManagerLobby room)
            {
                room.roomSlots.Remove(this);
                room.ResetPlayerID();
            }
            ResetUI();
        }
        #endregion
    }

}