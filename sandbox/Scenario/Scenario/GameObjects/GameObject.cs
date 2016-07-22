namespace RpgSim.GameObjects
{
    public class GameObject : BaseObject
    {
        public GameObject(string id)
        {
            Id = id;
        }

        public string Id { get; set; }

        public override string ToString()
        {
            return Id;
        }

    }
}