using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.EscapeGame
{
    public class RoomPlayer : NetworkBehaviour
    {
        public int id = 0;
        public int selectIndex = 0;
        public int roleIndex = 0;
        public int mapIndex = 0;
        public int selectState = 0;

        public Canvas container;
        public Transform roleUI;
        public Transform mapUI;

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
            if (Input.GetKeyDown(KeyCode.L))
            {
                CmdTest();
            }
        }

        [Command]
        public void CmdTest()
        {
            NetworkManagerLobby lobby = NetworkManager.singleton as NetworkManagerLobby;
            lobby.ResetPlayerID();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (NetworkManager.singleton is NetworkManagerLobby room)
            {
                DontDestroyOnLoad(this);
                room.roomSlots.Add(this);
                room.ResetPlayerID();

                container = room.container;
                roleUI = room.roleUI;
                mapUI = room.mapUI;
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

        }

        public override void OnStopClient()
        {
            base.OnStopClient();
        }
    }

}