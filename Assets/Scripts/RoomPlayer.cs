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
        [SyncVar(hook = nameof(OnSelectChanged))]
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

        public System.Action<int, int> OnSelectChangedAction;
        public void OnSelectChanged(int val, int newVal)
        {
            CmdOnSelectedChanged(val, newVal);
        }

        [Command]
        public void CmdOnSelectedChanged(int val, int newVal)
        {
            OnSelectChangedAction(val, newVal);
        }

        public void SelectRole(int additive)
        {
            if (selectState == 0)
                if (selectIndex + additive >= roleUI.childCount || roleIndex + additive < 0)
                    return;

            if (selectState == 1)
                if (selectIndex + additive >= mapUI.childCount || roleIndex + additive < 0)
                    return;

            selectIndex += additive;
        }

        public void SyncUI(List<RoomPlayer> players)
        {
            ResetUI();
            CmdResetUI();
            foreach (RoomPlayer p in players)
            {
                ActiveUI(p.id, p.selectIndex, p.selectState, true);
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

        public void ActiveUI(int id, int index, int select, bool isActive)
        {
            switch (select)
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
            if (input.GetButtonDown("SelectR"))
            {
                selectIndex++;
            }
            if (input.GetButtonDown("SelectL"))
            {
                selectIndex--;
            }
            if (input.GetButtonDown("SelectU"))
            {
            }
            if (input.GetButtonDown("SelectD"))
            {
            }
            if (input.GetButtonDown("Confirm"))
            {
            }
            if (input.GetButtonDown("Cancel"))
            {
            }
        }

        private void Awake()
        {
            input = ReInput.players.GetPlayer(0);
        }

        #region Mirro virtual func
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

                OnSelectChangedAction = room.OnSelectChanged;
            }
        }
        public override void OnStopClient()
        {
            base.OnStopClient();
        }
        #endregion
    }

}