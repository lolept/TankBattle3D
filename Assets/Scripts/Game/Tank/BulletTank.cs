using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;

namespace Game.Tank
{
    public class BulletTank : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public int damage;
    
    
        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case 8:
                    var data = photonEvent.CustomData.Unwrap<string[]>();
                    var id = int.Parse(data[0]);
                    if (id == gameObject.GetPhotonView().ViewID){
                        if (gameObject.name.Equals(data[1]))
                        {
                            Destroy(gameObject);
                        }
                    }
                    break;
            }
        }
    }
}
