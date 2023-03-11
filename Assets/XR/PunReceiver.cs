using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class PunReceiver : MonoBehaviourPunCallbacks 
{
    [SerializeField] private GameObject playerPrefab;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("TestRoom", new RoomOptions(), TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
    }
}
