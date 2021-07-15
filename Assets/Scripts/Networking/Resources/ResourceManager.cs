using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.EscapeGame
{
    public class ResourceManager : MonoBehaviour
    {
        [Tooltip("Assets/Resources/[Insert Folder Path]")]
        [SerializeField] string[] folderPaths = null;

        bool isLoaded = false;

        private void Awake()
        {
            if (isLoaded == false)
            {
                if (folderPaths == null || folderPaths.Length <= 0) Debug.Log("No folder path given");
                else
                {
                    for (int i = 0; i < folderPaths.Length; i++)
                    {
                        List<GameObject> temp = Resources.LoadAll(folderPaths[i], typeof(GameObject)).Cast<GameObject>().ToList();
                        if (temp == null || temp.Count <= 0) Debug.Log("Nothing found at : \"/Assets/Resources" + folderPaths[i] + "\". Skipped folder");
                        else
                        {
                            for (int n = 0; n < temp.Count; n++)
                            {
                                GetComponentInChildren<NetworkManagerLobby>().spawnPrefabs.Add(temp[n]);
                            }
                        }
                    }
                }

                isLoaded = true;
            }
        }
    }
}