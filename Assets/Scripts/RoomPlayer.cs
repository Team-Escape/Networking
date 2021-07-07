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
        [SyncVar(hook = nameof(OnSelectChaned))]
        public int selectIndex = 0;
        [SyncVar]
        public string selectedRoleName = "";
        [SyncVar]
        public string selectedMapName = "";

        /// <summary>
        /// Player current select state, 0 : Role, 1 : Map, 2: Waiting For Other players 
        /// </summary>
        [SyncVar(hook = nameof(OnStateChaned))]
        public int selectState = 0;

        public Canvas container;
        public Transform roleUI;
        public Transform mapUI;

        Player input;

        [ClientRpc]
        public void BroadCastToAll(string msg) => Debug.Log(msg);

        [Server]
        public void SetSelectIndex(int val)
        {
            if (isServer == false) return;
            selectIndex = val;
            RpcSetSelectIndex(val);
        }

        [ClientRpc]
        public void RpcSetSelectIndex(int val) => selectIndex = val;

        public void OnStateChaned(int val, int newVal)
        {
            if (isServer == false) return;
            switch (newVal)
            {
                case 0:
                case 1:
                    selectedRoleName = roleUI.GetChild(selectIndex).name;
                    ActiveUI(id, selectIndex, val, false);
                    CmdActiveUI(id, selectIndex, val, false);
                    ActiveUI(id, 0, newVal, true);
                    CmdActiveUI(id, 0, newVal, true);
                    break;
                case 2:
                    selectedMapName = mapUI.GetChild(selectIndex).name;
                    ActiveUI(id, selectIndex, val, false);
                    CmdActiveUI(id, selectIndex, val, false);
                    break;
                default:
                    return;
            }
            SetSelectIndex(0);
        }

        [Command]
        public void CmdCancel()
        {
            switch (selectState)
            {
                case 1:
                case 2:
                    selectState--;
                    Cancel();
                    break;
                case 0:
                default:
                    return;
            }
        }

        public void Cancel()
        {
            if (isServer)
            {
                switch (selectState)
                {
                    case 1:
                    case 2:
                        selectState--;
                        break;
                    case 0:
                    default:
                        return;
                }
            }
            else
            {
                CmdCancel();
            }
        }

        [Command]
        public void CmdConfirm()
        {
            switch (selectState)
            {
                case 0:
                    selectState++;
                    Confirm();
                    break;
                case 1:
                    selectState++;
                    Confirm();
                    break;
                case 2:
                default:
                    return;
            }
        }

        public void Confirm()
        {
            if (isServer)
            {
                switch (selectState)
                {
                    case 0:
                        selectState++;
                        break;
                    case 1:
                        selectState++;
                        break;
                    case 2:
                    default:
                        return;
                }
            }
            else if (isClient)
            {
                CmdConfirm();
            }
        }

        public void OnSelectChaned(int val, int newVal)
        {
            if (isServer == false) return;

            int maxIndex = (selectState == 0) ? roleUI.childCount : mapUI.childCount;

            if (val < maxIndex)
            {
                ActiveUI(id, val, selectState, false);
                RpcActiveUI(id, val, selectState, false);
            }
            if (newVal < maxIndex)
            {
                ActiveUI(id, newVal, selectState, true);
                RpcActiveUI(id, newVal, selectState, true);
            }
        }

        [Command]
        public void CmdSelect(int val)
        {
            selectIndex += val;
            Select(val);
        }

        public void Select(int additive)
        {
            if (selectState == 0 && selectIndex + additive >= roleUI.childCount) return;
            else if (selectState == 1 && selectIndex + additive >= mapUI.childCount) return;
            else if (selectIndex + additive < 0) return;

            if (isServer)
            {
                selectIndex += additive;
            }
            else if (isClient)
            {
                CmdSelect(additive);
            }
        }

        public void SyncUI(List<RoomPlayer> players)
        {
            ResetUI();
            if (isLocalPlayer && players.Count > 1)
                CmdResetUI();
            foreach (RoomPlayer p in players)
            {
                ActiveUI(p.id, p.selectIndex, p.selectState, true);
                if (isLocalPlayer && players.Count > 1)
                    CmdActiveUI(p.id, p.selectIndex, p.selectState, true);
            }
        }

        #region ActiveUI
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
                    if (index >= roleUI.childCount) return;
                    roleUI.GetChild(index).GetChild(id + 1).gameObject.SetActive(isActive);
                    break;
                case 1:
                    if (index >= mapUI.childCount) return;
                    mapUI.GetChild(index).GetChild(id + 1).gameObject.SetActive(isActive);
                    break;
                case 2:
                default:
                    return;
            }
        }
        #endregion

        #region ResetUI
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
        #endregion

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
                    Confirm();
                }
                else if (input.GetButtonDown("Cancel"))
                {
                    Cancel();
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